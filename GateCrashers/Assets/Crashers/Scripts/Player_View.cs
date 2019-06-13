using UnityEngine;
using UnityEngine.AI;
using Mirror;
namespace GateCrashers
{
    public class Player_View : NetworkBehaviour
    {
        [Header("Model")]
        public Player_Model Model;
        [Header("Controller")]
        public Player_Controller Controller;
        [Header("Components")]
        public Rigidbody body;
        public Animator animator;

        [Header("Camera")]
        public GameObject CameraPrefab;
        public GameObject ClientCamera;

        public override void OnStartLocalPlayer()
        {
            // movement for local player
            if (!isLocalPlayer) return;
            this.ClientCamera = Instantiate(CameraPrefab,transform);
        }

        void FixedUpdate()
        {
            // movement for local player
            if (!isLocalPlayer) return;

            // move
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");
            body.velocity = new Vector3(-horizontal* Model.movementSpeed * Time.deltaTime,body.velocity.y,-vertical* Model.movementSpeed * Time.deltaTime);

            animator.SetBool("Moving", body.velocity != Vector3.zero);

            // shoot
            if (Input.GetKeyDown(Model.jumpKey))
            {
                if (Controller.Grounded(transform.position))
                {
                    CmdJump();
                }
            }
        }

        // this is called on the server
        [Command]
        void CmdJump()
        {
        /*
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
            NetworkServer.Spawn(projectile);
            */
            body.AddForce(Vector3.up*200);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }
    }
}
