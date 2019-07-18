﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PickUp : NetworkBehaviour
{
    [SyncVar]
    public bool held = false; //if the pickup is being held
    public NetworkIdentity holder; //whose holding the pickup
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    [Command]
    public void CmdCheck(NetworkIdentity other, NetworkIdentity pawn)
    {
        
        if (held == true)
        {
            //item is dropped
            rb.isKinematic = false;
            rb.useGravity = true;
            held = false;
            holder = null;
            pawn.GetComponent<BaseControlable>().holding = false;
        }
        else
        {
            //item is picked up
            rb.isKinematic = true;
            rb.useGravity = false;
            held = true;
            holder = other;
            pawn.GetComponent<BaseControlable>().holding = true;
        }
    }

    //makes sure the player is close enough to be able to pick it up
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<BaseControlable>().close = true;
    }
    
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<BaseControlable>().close = false;
    }
}
