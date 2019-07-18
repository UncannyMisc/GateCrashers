using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PickUp : NetworkBehaviour
{
    public bool held = false; //if the pickup is being held
    public NetworkIdentity holder; //whose holding the pickup
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    [Command]
    public void CmdCheck(NetworkIdentity other, Client pawn)
    {
        if (held == true)
        {
            //item is dropped
            rb.isKinematic = false;
            rb.useGravity = true;
            held = false;
            holder = null;
            pawn.DropCall();
        }
        else
        {
            //item is picked up
            rb.isKinematic = false;
            rb.useGravity = false;
            held = true;
            holder = other;
            pawn.PickUpCall();
        }
    }
}
