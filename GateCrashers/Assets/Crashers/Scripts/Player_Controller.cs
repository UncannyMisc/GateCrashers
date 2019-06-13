using UnityEngine;
using UnityEngine.AI;
using Mirror;
namespace GateCrashers
{
    public class Player_Controller : NetworkBehaviour
    {
        [Header("Components")]
        public Player_Model Model;

        public bool Grounded(Vector3 position)
        {
            RaycastHit hit;
            Physics.Raycast(position, Vector3.down, out hit,2);
            return (hit.collider!=null);
        }
    }
}
