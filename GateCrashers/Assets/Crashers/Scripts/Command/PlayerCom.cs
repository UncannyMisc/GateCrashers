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
        
        public override void OnStartLocalPlayer()
        {
            Debug.Log("run");
            // movement for local player
            //if(!isServer)body.isKinematic=true;
        }

        public override void OnPosses(Client C)
        {
            if (!hasAuthority&&!isServer)
            {
                //Debug.Log("I'm a client that doesn't own a replicated object");
                body.isKinematic = true;
            }
            else
            {
                //Debug.Log("I'm a client/server that owns an object");
            }
            moveStrat = new MoveStrat();
            jumpStrat = new JumpStrat();
            //todo make a new interface or struct containing information about the control scheme and update client
        }

//todo, maybe have movement methods here to handle different methods, maybe have it tell the command as a state machine to swap states
    }
}
