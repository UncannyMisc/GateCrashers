// vis2k:
// base class for NetworkTransform and NetworkTransformChild.
// New method is simple and stupid. No more 1500 lines of code.
//
// Server sends current data.
// Client saves it and interpolates last and latest data points.
//   Update handles transform movement / rotation
//   FixedUpdate handles rigidbody movement / rotation
//
// Notes:
// * Built-in Teleport detection in case of lags / teleport / obstacles
// * Quaternion > EulerAngles because gimbal lock and Quaternion.Slerp
// * Syncs XYZ. Works 3D and 2D. Saving 4 bytes isn't worth 1000 lines of code.
// * Initial delay might happen if server sends packet immediately after moving
//   just 1cm, hence we move 1cm and then wait 100ms for next packet
// * Only way for smooth movement is to use a fixed movement speed during
//   interpolation. interpolation over time is never that good.
//
using UnityEngine;

namespace Mirror
{
    public abstract class NetworkBodyPosBase : NetworkBehaviour
    {
        // rotation compression. not public so that other scripts can't modify
        // it at runtime. alternatively we could send 1 extra byte for the mode
        // each time so clients know how to decompress, but the whole point was
        // to save bandwidth in the first place.
        // -> can still be modified in the Inspector while the game is running,
        //    but would cause errors immediately and be pretty obvious.
        [Tooltip("Compresses 16 Byte Quaternion into None=12, Much=3, Lots=2 Byte")]
        [SerializeField] Compression compressRotation = Compression.Much;
        public enum Compression { None, Much, Lots , NoRotation }; // easily understandable and funny

        // server
        Vector3 lastPosition;
        Vector3 lastVelocity;
        Quaternion lastRotation;
        Vector3 lastAngularVelocity;

        // client
        public class DataPoint
        {
            public float timeStamp;
            public Vector3 position;
            public Vector3 velocity;
            public Quaternion rotation;
            public Vector3 angularVelocity;
        }
        // interpolation start and goal
        DataPoint start;
        DataPoint goal;

        // local authority send time
        float lastClientSendTime;
        
        // target rigidbody to sync. can be on a child.
        protected abstract Rigidbody targetRigidbody { get; }

        // serialization is needed by OnSerialize and by manual sending from authority
        static void SerializeIntoWriter(NetworkWriter writer, Vector3 position,Vector3 velocity, Quaternion rotation,Vector3 angularvelocity, Compression compressRotation)
        {
            // serialize position
            writer.Write(position);
            // serialize velocity
            writer.Write(velocity);

            // serialize rotation
            // writing quaternion = 16 byte
            // writing euler angles = 12 byte
            // -> quaternion->euler->quaternion always works.
            // -> gimbal lock only occurs when adding.
            Vector3 euler = rotation.eulerAngles;
            if (compressRotation == Compression.None)
            {
                // write 3 floats = 12 byte
                writer.Write(euler.x);
                writer.Write(euler.y);
                writer.Write(euler.z);
            }
            else if (compressRotation == Compression.Much)
            {
                // write 3 byte. scaling [0,360] to [0,255]
                writer.Write(FloatBytePacker.ScaleFloatToByte(euler.x, 0, 360, byte.MinValue, byte.MaxValue));
                writer.Write(FloatBytePacker.ScaleFloatToByte(euler.y, 0, 360, byte.MinValue, byte.MaxValue));
                writer.Write(FloatBytePacker.ScaleFloatToByte(euler.z, 0, 360, byte.MinValue, byte.MaxValue));
            }
            else if (compressRotation == Compression.Lots)
            {
                // write 2 byte, 5 bits for each float
                writer.Write(FloatBytePacker.PackThreeFloatsIntoUShort(euler.x, euler.y, euler.z, 0, 360));
            }
            // serialize angularvelocity
            writer.Write(angularvelocity);
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            SerializeIntoWriter(writer, targetRigidbody.position,targetRigidbody.velocity, targetRigidbody.rotation,targetRigidbody.angularVelocity, compressRotation);
            return true;
        }

        /*
        // try to estimate movement speed for a data point based on how far it
        // moved since the previous one
        // => if this is the first time ever then we use our best guess:
        //    -> delta based on transform.position
        //    -> elapsed based on send interval hoping that it roughly matches
        static float EstimateMovementAcceleration(DataPoint from, DataPoint to, Rigidbody rigidbody, float sendInterval)
        {
            Vector3 delta = to.velocity - (from != null ? from.velocity : rigidbody.velocity);
            float elapsed = from != null ? to.timeStamp - from.timeStamp : sendInterval;
            return elapsed > 0 ? delta.magnitude / elapsed : 0; // avoid NaN
        }
        static float EstimateAngularAcceleration(DataPoint from, DataPoint to, Rigidbody rigidbody, float sendInterval)
        {
            Vector3 delta = to.angularVelocity - (from != null ? from.angularVelocity : rigidbody.angularVelocity);
            float elapsed = from != null ? to.timeStamp - from.timeStamp : sendInterval;
            return elapsed > 0 ? delta.magnitude / elapsed : 0; // avoid NaN
        }
        */

        // serialization is needed by OnSerialize and by manual sending from authority
        void DeserializeFromReader(NetworkReader reader)
        {
            // put it into a data point immediately
            DataPoint temp = new DataPoint
            {
                // deserialize position
                position = reader.ReadVector3(),
                // deserialize velocity
                velocity = reader.ReadVector3()
            };

            // deserialize rotation
            if (compressRotation == Compression.None)
            {
                // read 3 floats = 16 byte
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                temp.rotation = Quaternion.Euler(x, y, z);
            }
            else if (compressRotation == Compression.Much)
            {
                // read 3 byte. scaling [0,255] to [0,360]
                float x = FloatBytePacker.ScaleByteToFloat(reader.ReadByte(), byte.MinValue, byte.MaxValue, 0, 360);
                float y = FloatBytePacker.ScaleByteToFloat(reader.ReadByte(), byte.MinValue, byte.MaxValue, 0, 360);
                float z = FloatBytePacker.ScaleByteToFloat(reader.ReadByte(), byte.MinValue, byte.MaxValue, 0, 360);
                temp.rotation = Quaternion.Euler(x, y, z);
            }
            else if (compressRotation == Compression.Lots)
            {
                // read 2 byte, 5 bits per float
                float[] xyz = FloatBytePacker.UnpackUShortIntoThreeFloats(reader.ReadUInt16(), 0, 360);
                temp.rotation = Quaternion.Euler(xyz[0], xyz[1], xyz[2]);
            }

            // deserialize angular velocity
            temp.angularVelocity = reader.ReadVector3();

            temp.timeStamp = Time.fixedTime;

            // movement speed: based on how far it moved since last time
            // has to be calculated before 'start' is overwritten
            //temp.acceleration = EstimateMovementAcceleration(goal, temp, targetRigidbody, syncInterval);
            //temp.angularAcceleration = EstimateAngularAcceleration(goal, temp, targetRigidbody, syncInterval);

            // reassign start wisely
            // -> first ever data point? then make something up for previous one
            //    so that we can start interpolation without waiting for next.
            if (start == null)
            {
                start = new DataPoint{
                    timeStamp = Time.fixedTime - syncInterval,
                    position = targetRigidbody.position,
                    velocity = targetRigidbody.velocity,
                    //acceleration = temp.acceleration,
                    rotation = targetRigidbody.rotation,
                    angularVelocity = targetRigidbody.angularVelocity//,
                    //angularAcceleration = temp.angularAcceleration//,
                    //movementSpeed = temp.movementSpeed
                };
            }
            // -> second or nth data point? then update previous, but:
            //    we start at where ever we are right now, so that it's
            //    perfectly smooth and we don't jump anywhere
            //
            //    example if we are at 'x':
            //
            //        A--x->B
            //
            //    and then receive a new point C:
            //
            //        A--x--B
            //              |
            //              |
            //              C
            //
            //    then we don't want to just jump to B and start interpolation:
            //
            //              x
            //              |
            //              |
            //              C
            //
            //    we stay at 'x' and interpolate from there to C:
            //
            //           x..B
            //            \ .
            //             \.
            //              C
            //
            else
            {
                float oldDistance = Vector3.Distance(start.position, goal.position);
                float newDistance = Vector3.Distance(goal.position, temp.position);
                
                float oldAccel = Vector3.Distance(start.velocity, goal.velocity);
                float newAccel = Vector3.Distance(goal.velocity, temp.velocity);

                start = goal;

                // teleport / lag / obstacle detection: only continue at current
                // position if we aren't too far away
                if (Vector3.Distance(targetRigidbody.position, start.position) < oldDistance + newDistance||
                    Vector3.Distance(targetRigidbody.velocity,start.velocity)<oldAccel+newAccel)
                {
                    start.position = targetRigidbody.position;
                    start.velocity = targetRigidbody.velocity;
                    start.rotation = targetRigidbody.rotation;
                    start.angularVelocity = targetRigidbody.angularVelocity;
                }
            }

            // set new destination in any case. new data is best data.
            goal = temp;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            // deserialize
            DeserializeFromReader(reader);
        }

        // local authority client sends sync message to server for broadcasting
        [Command]
        void CmdClientToServerSync(byte[] payload)
        {
            // deserialize payload
            NetworkReader reader = new NetworkReader(payload);
            DeserializeFromReader(reader);

            // server-only mode does no interpolation to save computations,
            // but let's set the position directly
            if (isServer && !isClient)
                ApplyPositionAndRotationAndVelocityAndAngularVelocity(goal.position,goal.velocity, goal.rotation,goal.angularVelocity);

            // set dirty so that OnSerialize broadcasts it
            SetDirtyBit(1UL);
        }

        // where are we in the timeline between start and goal? [0,1]
        static float CurrentInterpolationFactor(DataPoint start, DataPoint goal)
        {
            if (start != null)
            {
                float difference = goal.timeStamp - start.timeStamp;

                // the moment we get 'goal', 'start' is supposed to
                // start, so elapsed time is based on:
                // also known as how long it took for the goal packet to reach the receiver
                float elapsed = Time.fixedTime - goal.timeStamp;
                //return (float)(NetworkTime.rtt / 2)+Time.fixedUnscaledDeltaTime;
                return difference > 0 ? elapsed / difference : 0; // avoid NaN
            }
            return 0;
        }

        static Vector3 InterpolateVelocity(DataPoint start, DataPoint goal, Vector3 currentVelocity)
        {
            float t = CurrentInterpolationFactor(start,goal);
            return Vector3.Lerp(start.velocity, goal.velocity,t);
            //float acceleration = Mathf.Max(start.acceleration, goal.acceleration);
            //return Vector3.MoveTowards(currentVelocity, goal.velocity,acceleration*Time.deltaTime);
        }

        static Vector3 InterpolatePosition(DataPoint start, DataPoint goal, Vector3 currentPosition,Vector3 currentVelocity)
        {
            if (start != null)
            {
                // Option 1: simply interpolate based on time. but stutter
                // will happen, it's not that smooth. especially noticeable if
                // the camera automatically follows the player
                //   float t = CurrentInterpolationFactor();
                //   return Vector3.Lerp(start.position, goal.position, t);

                // Option 2: always += speed
                // -> speed is 0 if we just started after idle, so always use max
                //    for best results
                //float acceleration = Mathf.Max(start.acceleration, goal.acceleration);
                
                //Vector3 velocity = InterpolateVelocity(start, goal, currentVelocity);
                
                //return currentPosition + (InterpolateVelocity(start,goal,currentVelocity)*Time.deltaTime);
                //float speed = velocity.magnitude;
                //return Vector3.MoveTowards(currentPosition, goal.position, speed * Time.deltaTime);
                
                
                float t = CurrentInterpolationFactor(start,goal);
                float d = goal.timeStamp - start.timeStamp;
                Vector3 vel = Vector3.Lerp(start.position, start.position+(start.velocity*d),t);
                Vector3 vel2 = Vector3.Lerp(goal.position-(goal.velocity*d), goal.position,t);
                return Vector3.Lerp(vel, vel2,t);
            }
            return currentPosition;
        }
        
        static Vector3 InterpolateAngularVelocity(DataPoint start, DataPoint goal, Vector3 currentAngularVelocity)
        {
            float t = CurrentInterpolationFactor(start,goal);
            return Vector3.Lerp(start.angularVelocity, goal.angularVelocity,t);
            //float angularAcceleration = Mathf.Max(start.angularAcceleration, goal.angularAcceleration);
            //return Vector3.MoveTowards(currentAngularVelocity, goal.angularVelocity,angularAcceleration*Time.deltaTime);
        }
        
        static Quaternion InterpolateRotation(DataPoint start, DataPoint goal, Quaternion defaultRotation, Vector3 currentAngularVelocity)
        {
            if (start != null)
            {
                float t = CurrentInterpolationFactor(start, goal);
                float d = goal.timeStamp - start.timeStamp;
                Quaternion vel = Quaternion.Slerp(start.rotation, start.rotation*Quaternion.AngleAxis(start.angularVelocity.magnitude*d,start.angularVelocity),t);
                Quaternion vel2 = Quaternion.Slerp(goal.rotation*Quaternion.AngleAxis(goal.angularVelocity.magnitude*d,goal.angularVelocity), goal.rotation,t);
                return Quaternion.Slerp(vel, vel2,t);
                //return (defaultRotation * Quaternion.Euler(InterpolateAngularVelocity(start,goal,currentAngularVelocity)));//Quaternion.Slerp(start.rotation, goal.rotation, t);
            }
            return defaultRotation;
        }

        // teleport / lag / stuck detection
        // -> checking distance is not enough since there could be just a tiny
        //    fence between us and the goal
        // -> checking time always works, this way we just teleport if we still
        //    didn't reach the goal after too much time has elapsed
        bool NeedsTeleport()
        {
            // calculate time between the two data points
            float startTime = start != null ? start.timeStamp : Time.fixedTime - syncInterval;
            float goalTime = goal != null ? goal.timeStamp : Time.fixedTime;
            float difference = goalTime - startTime;
            float timeSinceGoalReceived = Time.fixedTime - goalTime;
            return timeSinceGoalReceived > difference * 5;
        }

        // moved since last time we checked it?
        bool HasMovedOrRotatedOrAccelerated()
        {
            // moved or rotated?
            bool moved = lastPosition != targetRigidbody.position;
            bool rotated = lastRotation != targetRigidbody.rotation;
            bool accelerated = lastVelocity != targetRigidbody.velocity;
            accelerated = accelerated || lastAngularVelocity != targetRigidbody.angularVelocity;

            // save last for next frame to compare
            // (only if change was detected. otherwise slow moving objects might
            //  never sync because of C#'s float comparison tolerance. see also:
            //  https://github.com/vis2k/Mirror/pull/428)
            bool change = moved || rotated || accelerated;
            if (change)
            {
                lastPosition = targetRigidbody.position;
                lastRotation = targetRigidbody.rotation;
                lastVelocity = targetRigidbody.velocity;
                lastAngularVelocity = targetRigidbody.angularVelocity;
            }
            return change;
        }

        // set position carefully depending on the target component
        void ApplyPositionAndRotationAndVelocityAndAngularVelocity(Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 angularVelocity)
        {
            targetRigidbody.position = position;
            targetRigidbody.velocity = velocity;
            if (Compression.NoRotation != compressRotation)
            {
                targetRigidbody.rotation = rotation;
            }

            targetRigidbody.angularVelocity = angularVelocity;
        }
        void FixedUpdate()
        {
            // if server then always sync to others.
            if (isServer)
            {
                // just use OnSerialize via SetDirtyBit only sync when position
                // changed. set dirty bits 0 or 1
                SetDirtyBit(HasMovedOrRotatedOrAccelerated() ? 1UL : 0UL);
            }

            // no 'else if' since host mode would be both
            if (isClient)
            {
                // send to server if we have local authority (and aren't the server)
                // -> only if connectionToServer has been initialized yet too
                if (!isServer && hasAuthority)
                {
                    // check only each 'syncInterval'
                    if (Time.fixedTime - lastClientSendTime >= syncInterval)
                    {
                        if (HasMovedOrRotatedOrAccelerated())
                        {
                            // serialize
                            NetworkWriter writer = new NetworkWriter();
                            SerializeIntoWriter(writer, targetRigidbody.position,targetRigidbody.velocity, targetRigidbody.rotation,targetRigidbody.angularVelocity, compressRotation);

                            // send to server
                            CmdClientToServerSync(writer.ToArray());
                        }
                        lastClientSendTime = Time.fixedTime;
                    }
                }

                // apply interpolation on client for all players
                // unless this client has authority over the object. could be
                // himself or another object that he was assigned authority over
                if (!hasAuthority)
                {
                    // received one yet? (initialized?)
                    if (goal != null)
                    {
                        // teleport or interpolate
                        if (NeedsTeleport())
                        {
                            ApplyPositionAndRotationAndVelocityAndAngularVelocity(goal.position,goal.velocity, goal.rotation,goal.angularVelocity);
                        }
                        else
                        {
                            ApplyPositionAndRotationAndVelocityAndAngularVelocity(InterpolatePosition(start, goal, targetRigidbody.position,targetRigidbody.velocity),InterpolateVelocity(start,goal,targetRigidbody.velocity),
                                                     InterpolateRotation(start, goal, targetRigidbody.rotation,targetRigidbody.angularVelocity),InterpolateAngularVelocity(start,goal,targetRigidbody.angularVelocity));
                        }
                    }
                }
            }
        }

        static void DrawDataPointGizmo(DataPoint data, Color color)
        {
            // use a little offset because transform.position might be in
            // the ground in many cases
            Vector3 offset = Vector3.up * 0.01f;

            // draw position
            Gizmos.color = color;
            Gizmos.DrawSphere(data.position + offset, 0.5f);

            // draw forward and up
            Gizmos.color = Color.blue; // like unity move tool
            Gizmos.DrawRay(data.position + offset, data.rotation * Vector3.forward);

            Gizmos.color = Color.green; // like unity move tool
            Gizmos.DrawRay(data.position + offset, data.rotation * Vector3.up);
            
            Gizmos.color = Color.yellow; // like unity move tool
            Gizmos.DrawRay(data.position + offset, data.velocity);
        }

        static void DrawLineBetweenDataPoints(DataPoint data1, DataPoint data2, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(data1.position, data1.velocity);
            Gizmos.DrawLine(data2.position, data2.velocity);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(data1.position, data2.position);
        }

        // draw the data points for easier debugging
        void OnDrawGizmos()
        {
            // draw start and goal points
            if (start != null) DrawDataPointGizmo(start, Color.gray);
            if (goal != null) DrawDataPointGizmo(goal, Color.white);

            // draw line between them
            if (start != null && goal != null) DrawLineBetweenDataPoints(start, goal, Color.cyan);
        }
    }
}
