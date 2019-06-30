using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class BaseControlable : NetworkBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 100;
    [Header("CommandStrategies")]
    public IComStrat<Rigidbody,Vector3> moveStrat;
    public IComStrat<Rigidbody,bool> jumpStrat;
    public abstract void OnPosses(Client C);
}
