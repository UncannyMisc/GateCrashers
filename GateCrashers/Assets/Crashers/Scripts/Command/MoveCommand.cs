using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UIElements;

public class MoveCommand : BaseCommand
{
    public float movespeed;
    public float horizontal;
    public float vertical;

    [SyncVar]
    private Vector3 move;

    public override void predict(NetworkIdentity controlable)
    {
        move =
            new Vector3(-horizontal* movespeed * Time.deltaTime,controlable.GetComponent<Rigidbody>().velocity.y,-vertical* movespeed * Time.deltaTime);
        //controlable.GetComponent<Rigidbody>().velocity = move;
        Cmdrequest(controlable);
    }

    [Command]
    public override void Cmdrequest(NetworkIdentity controlable)
    {
        //controlable.GetComponent<Rigidbody>().velocity = move;
        Rpcexecute(controlable);
    }
    
    [ClientRpc]
    public override void Rpcexecute(NetworkIdentity controlable)
    {
        controlable.GetComponent<Rigidbody>().velocity = move;
    }
}
