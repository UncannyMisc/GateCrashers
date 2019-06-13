using UnityEngine;
using UnityEngine.AI;
using Mirror;
namespace GateCrashers
{
    public class Player : NetworkBehaviour
    {
        [Header("Components")]
        public Rigidbody body;
        public Animator animator;

        [Header("Camera")]
        public GameObject CameraPrefab;
        public GameObject ClientCamera;
        
        [Header("Movement")]
        public float movementSpeed = 100;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public GameObject projectilePrefab;
        public Transform projectileMount;

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
            body.velocity = new Vector3(-horizontal* movementSpeed * Time.deltaTime,body.velocity.y,-vertical* movementSpeed * Time.deltaTime);

            animator.SetBool("Moving", body.velocity != Vector3.zero);

            // shoot
            if (Input.GetKeyDown(shootKey))
            {
                CmdFire();
            }
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
            NetworkServer.Spawn(projectile);
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
