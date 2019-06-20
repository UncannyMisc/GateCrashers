using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BaseControlable : NetworkBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 100;
}
