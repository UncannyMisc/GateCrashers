using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InteractStrat : IComStrat<NetworkIdentity,NetworkIdentity>
{
    
    public void JustPressed(float totaltimedelta, NetworkIdentity pawn, NetworkIdentity values)
    {
        
    }
    public void JustReleased(float totaltimedelta, NetworkIdentity pawn, NetworkIdentity values)
    {
    }

    public void Held(float totaltimedelta, NetworkIdentity pawn, NetworkIdentity values)
    {
    }

    public void Update(float totaltimedelta, NetworkIdentity pawn, NetworkIdentity values)
    {
        Rigidbody temp = values.GetComponent<Rigidbody>();
        temp.position = Vector3.Lerp(temp.position, pawn.GetComponent<Rigidbody>().position + Vector3.up * 2,
            Time.deltaTime * 10);

    }


}
