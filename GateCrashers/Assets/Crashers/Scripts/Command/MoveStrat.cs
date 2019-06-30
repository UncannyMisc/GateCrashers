using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveStrat : IComStrat<Rigidbody,Vector3>
{
    public float movespeed=100;

    public void execute(float totaltimedelta,Rigidbody body,Vector3 values)
    {
        body.velocity = 
            new Vector3(-values.x* movespeed * Time.deltaTime,body.velocity.y,-values.z* movespeed * Time.deltaTime);
    }
}
