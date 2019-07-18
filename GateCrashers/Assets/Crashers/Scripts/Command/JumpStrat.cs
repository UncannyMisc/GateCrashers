using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpStrat : IComStrat<Rigidbody,bool>
{
    public float jump = 10;

    public void JustPressed(float totaltimedelta, Rigidbody body, bool values)
    {
        body.AddForce(0,jump,0,ForceMode.VelocityChange);
    }
    public void JustReleased(float totaltimedelta, Rigidbody body, bool values)
    {
    }

    public void Held(float totaltimedelta,Rigidbody body,bool values)
    {
    }
    public void Update(float totaltimedelta, Rigidbody body, bool values)
    {
    }


}
