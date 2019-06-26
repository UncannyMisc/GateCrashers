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
    public virtual void Cmdrequest(NetworkIdentity controlable)
    {
        return;
    }
    public virtual void Rpcexecute(NetworkIdentity controlable)
    {
        return;
    }
}
