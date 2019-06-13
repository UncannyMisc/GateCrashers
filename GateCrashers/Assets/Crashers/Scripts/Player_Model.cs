using UnityEngine;
using UnityEngine.AI;
using Mirror;
namespace GateCrashers
{
    public class Player_Model : NetworkBehaviour
    {
        [Header("Movement")]
        public float movementSpeed = 100;
        public KeyCode jumpKey = KeyCode.Space;
    }
}
