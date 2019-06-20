using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BaseCommand : NetworkBehaviour
{
    public virtual void predict(NetworkIdentity controlable,object[] parameters)
    {
        return;
    }
    public virtual void Cmdrequest(NetworkIdentity controlable,object[] parameters)
    {
        return;
    }
    public virtual void Rpcexecute(NetworkIdentity controlable,object[] parameters)
    {
        return;
    }
}
