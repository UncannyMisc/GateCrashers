using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class PartyPerson : NetworkBehaviour
{
    private void Start()
    {
        float temp = Random.Range(0f, 1f);
        GetComponentInChildren<Renderer>().material.SetFloat("Vector1_ACD294B7", temp);
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            if (Random.value > 0.99f)
            {
                RpcJump();
            }
        }
    }

    [ClientRpc]
    public void RpcJump()
    {
        GetComponent<Rigidbody>().AddForce(0,4,0, ForceMode.VelocityChange);
    }
}
