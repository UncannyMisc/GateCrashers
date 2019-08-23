using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class PickUp : NetworkBehaviour
{
    [SyncVar]
    public bool held = false; //if the pickup is being held
    [SyncVar]
    public NetworkIdentity holder; //whose holding the pickup
    private Rigidbody rb;

    public UnityEvent Dropped;
    public UnityAction restarting;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        restarting += CmdResetPos;
        FindObjectOfType<EndingScript>().gameRestart.AddListener(restarting);
        
        if (Dropped == null) Dropped = new UnityEvent();

    }

    [Command]
    public void CmdResetPos()
    {
        this.transform.position = FindObjectOfType<CrasherManager>().crateSpawn.position;
    }

    public void PickUpBox(NetworkIdentity other)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        held = true;
        holder = other;
    }

    public void ForceDrop()
    {
        //run pause in holder
        //holder.GetComponent<BaseControlable>().holding = false;
        Dropped.Invoke();
        held = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        holder = null;
    }

    public void Drop()
    {
        //holder.GetComponent<BaseControlable>().holding = false;
        Dropped.Invoke();
        held = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        holder = null;
    }
    
    /*public void Check(NetworkIdentity other, NetworkIdentity pawn)
    {
        if (held)
        {
            //item is dropped
            rb.isKinematic = false;
            rb.useGravity = true;
            held = false;
            holder.GetComponentInChildren<BaseControlable>().holding = false;
            holder = null;

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
    }*/
    //makes sure the player is close enough to be able to pick it up
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BaseControlable>() != null)
        {
            other.GetComponent<BaseControlable>().close = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BaseControlable>() != null)
        {
            other.GetComponent<BaseControlable>().close = true;
        }
    }
}
