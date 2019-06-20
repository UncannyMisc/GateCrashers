using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UIElements;

public class MoveCommand : BaseCommand
{
    [Client]
    public override void predict(NetworkIdentity controlable, object[] parameters)
    {
        Vector3 temp =
            new Vector3(-(float)parameters[1]* (float)parameters[0] * Time.deltaTime,controlable.GetComponent<Rigidbody>().velocity.y,-(float)parameters[2]* (float)parameters[0] * Time.deltaTime);
        controlable.GetComponent<Rigidbody>().velocity = temp;
        object[] temp2 = {temp};
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
        controlable.GetComponent<Rigidbody>().velocity = (Vector3)parameters[0];
    }
}
