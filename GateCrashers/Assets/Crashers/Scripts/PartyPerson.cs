using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PartyPerson : NetworkBehaviour
{
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
