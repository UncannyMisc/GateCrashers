using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UIElements;

public class MoveCommand : BaseCommand
{
    [SyncVar]
    public float movespeed;
    //todo this is onlu on the client change
    [SyncVar]
    public float horizontal;
    [SyncVar]
    public float vertical;


    public override void predict(NetworkIdentity controlable)
    {
        controlable.GetComponent<Rigidbody>().velocity = 
            new Vector3(-horizontal* movespeed * Time.deltaTime,controlable.GetComponent<Rigidbody>().velocity.y,-vertical* movespeed * Time.deltaTime);
        //controlable.GetComponent<Rigidbody>().velocity = move;
        Cmdrequest(controlable);
    }

    [Command]
    public void Cmdsyncinput(float a, float b, float speed)
    {
        movespeed = speed;
        horizontal = a;
        vertical = b;
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
        if(!isLocalPlayer)
        controlable.GetComponent<Rigidbody>().velocity = 
            new Vector3(-horizontal* movespeed * (Time.deltaTime+(float)NetworkTime.rtt),controlable.GetComponent<Rigidbody>().velocity.y,-vertical* movespeed * (Time.deltaTime+(float)NetworkTime.rtt));
    }
}
