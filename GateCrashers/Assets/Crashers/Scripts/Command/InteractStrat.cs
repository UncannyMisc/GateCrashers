﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InteractStrat : IComStrat<NetworkIdentity,NetworkIdentity>
{
    public void execute(float totaltimedelta, NetworkIdentity pawn, NetworkIdentity values)
    {
        Rigidbody temp = values.GetComponent<Rigidbody>();
        temp.position = Vector3.Lerp(temp.position,pawn.GetComponent<Rigidbody>().position+Vector3.up*2,Time.deltaTime);
    }

    //add a way to make the temp.pos be called on an update
    
    /*pick up box
    [Command]
    public void CmdPickup(NetworkIdentity pawn, NetworkIdentity values)
    {
        Rigidbody temp = values.GetComponent<Rigidbody>();
        temp.isKinematic = true;
        temp.position = Vector3.Lerp(temp.position,pawn.GetComponent<Rigidbody>().position+Vector3.up*2,Time.deltaTime);
    }
    
    //drop box
    [Command]
    public void CmdDrop(NetworkIdentity pawn, NetworkIdentity values)
    {
        Rigidbody temp = values.GetComponent<Rigidbody>();
        temp.isKinematic = false;    
    }*/


}
