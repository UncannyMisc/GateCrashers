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
    
    public void Check(NetworkIdentity other, Client pawn)
    {
        if (held == true)
        {
            //item is dropped
            rb.isKinematic = true;    
            held = false;
            holder = null;
        }
        else
        {
            //item is picked up
            rb.isKinematic = false;
            held = true;
            holder = other;
            pawn.PickUpCall(this.netIdentity);
        }
    }
}
