using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpStrat : IComStrat<Rigidbody,bool>
{
    public float jump = 10;
    
    public void execute(float totaltimedelta,Rigidbody body,bool values)
    {
        body.AddForce(0,jump,0,ForceMode.VelocityChange);
    }
}
