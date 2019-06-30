using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class JumpCommand : BaseCommand
{
    
    public override void predict(NetworkIdentity controlable)
    {
        //controlable.GetComponent<Rigidbody>().AddForce(0,jump,0,ForceMode.VelocityChange);
        //controlable.GetComponent<Rigidbody>().velocity = new Vector3(controlable.GetComponent<Rigidbody>().velocity.x,jump,controlable.GetComponent<Rigidbody>().velocity.z);
        Cmdrequest(controlable);
    }
    [Command]
    public override void Cmdrequest(NetworkIdentity controlable)
    {
        //jump = 10;
        //controlable.GetComponent<Rigidbody>().velocity = jump;
        Rpcexecute(controlable);
    }
    [ClientRpc]
    public override void Rpcexecute(NetworkIdentity controlable)
    {
        //Debug.Log(jump+" weird");
        //if(!isLocalPlayer)controlable.GetComponent<Rigidbody>().AddForce(0,jump,0,ForceMode.VelocityChange);
    }
}
