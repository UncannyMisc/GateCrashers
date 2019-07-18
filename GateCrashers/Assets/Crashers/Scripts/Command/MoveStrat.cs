using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveStrat : IComStrat<Rigidbody,Vector3>
{
    public float movespeed=100;

    public void JustPressed(float totaltimedelta, Rigidbody body, Vector3 values)
    {
    }
    public void JustReleased(float totaltimedelta, Rigidbody body, Vector3 values)
    {
    }

    public void Held(float totaltimedelta,Rigidbody body,Vector3 values)
    {
        body.velocity = 
            new Vector3(-values.x* movespeed * Time.deltaTime,body.velocity.y,-values.z* movespeed * Time.deltaTime);
    }

    public void NotHeld(float totaltimedelta, Rigidbody pawn, Vector3 values)
    {
    }

}
