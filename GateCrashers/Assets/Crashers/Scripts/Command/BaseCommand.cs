using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BaseCommand : NetworkBehaviour
{
    public virtual void predict(NetworkIdentity controlable)
    {
        return;
    }
    
    [Command]
    public virtual void Cmdrequest(NetworkIdentity controlable)
    {
        return;
    }
    [ClientRpc]
    public virtual void Rpcexecute(NetworkIdentity controlable)
    {
        return;
    }
}
