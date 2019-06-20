using UnityEngine;
using UnityEngine.AI;
using Mirror;
namespace GateCrashers
{
    public class PlayerCom : BaseControlable
    {
        [Header("Components")]
        public Rigidbody body;
        public Animator animator;
        

        [Header("Firing")]
        public GameObject projectilePrefab;
        public Transform projectileMount;

//todo, maybe have movement methods here to handle different methods, maybe have it tell the command as a state machine to swap states
    }
}
