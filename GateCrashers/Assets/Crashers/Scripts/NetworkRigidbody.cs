using UnityEngine;

namespace Mirror
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkRigidbody")]
    public class NetworkRigidbody : NetworkBodyPosBase
    {
        protected override Rigidbody targetRigidbody => GetComponent<Rigidbody>();
    }
}
