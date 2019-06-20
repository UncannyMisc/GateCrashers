using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class JumpCommand : BaseCommand
{
    [Client]
    public override void predict(NetworkIdentity controlable, object[] parameters)
    {
        Vector3 temp = 
            new Vector3(controlable.GetComponent<Rigidbody>().velocity.x,200,controlable.GetComponent<Rigidbody>().velocity.z);
        controlable.GetComponent<Rigidbody>().velocity = temp;
        object[] temp2 = { temp};
        Cmdrequest(controlable,temp2);
    }
    [Command]
    public override void Cmdrequest(NetworkIdentity controlable,object[] parameters)
    {
        Rpcexecute(controlable,parameters);
    }
    [ClientRpc]
    public override void Rpcexecute(NetworkIdentity controlable,object[] parameters)
    {
        controlable.GetComponent<Rigidbody>().velocity = 
            new Vector3(controlable.GetComponent<Rigidbody>().velocity.x,200,controlable.GetComponent<Rigidbody>().velocity.z);
    }
}
