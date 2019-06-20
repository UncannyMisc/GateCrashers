using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Client : NetworkBehaviour
{
    [Header("client objects")]
    public GameObject ClientCamera;
    
    [Header("Key Bindings")]
    public KeyCode jumpKey = KeyCode.Space;
    public JumpCommand jumpCom;

    public string vertAxis = "Vertical";
    public string horiAxis = "Horizontal";
    public MoveCommand moveCom;

    [Header("Possessions")]
    public BaseControlable pawn;
    
    public override void OnStartLocalPlayer()
    {
        // movement for local player
        if (!isLocalPlayer) return;
        this.ClientCamera = GameObject.FindWithTag("MainCamera").transform.parent.gameObject;
        ClientCamera.transform.position = transform.position;
        ClientCamera.transform.SetParent(transform);
    }
    void FixedUpdate()
    {
        // movement for local player
        if (!isLocalPlayer) return;

        // move
        float vertical = Input.GetAxis(vertAxis);
        float horizontal = Input.GetAxis(horiAxis);
        if (vertical != 0 || horizontal != 0)
        {
            object[] temp ={pawn.movementSpeed,horizontal,vertical};
            moveCom.predict(this.netIdentity,temp);
        }

        // shoot
        if (Input.GetKeyDown(jumpKey))
        {
            // test for ground
            RaycastHit hit;
            Physics.Raycast(pawn.transform.position,Vector3.down,out hit, 2);
            if (hit.collider)
            {
                object[] temp = { };
                jumpCom.predict(this.netIdentity, temp);
            }
        }
    }
}
