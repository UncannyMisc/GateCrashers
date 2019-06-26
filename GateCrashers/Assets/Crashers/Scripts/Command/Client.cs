using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Client : NetworkBehaviour
{
    [Header("client objects")]
    public GameObject ClientCamera;

    public GameObject startPlayerPrefab;
    
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
        bool temp = true;
        /*
        while (temp)
        {
            if (pawn)
            {
                ClientCamera.transform.position = pawn.transform.position;
                ClientCamera.transform.SetParent(pawn.transform);
                temp = false;
            }
        }
        */
        //CmdSetupPawn();
    }

    [Client]
    public void SetupPawn(NetworkIdentity a)
    {
        if (isLocalPlayer) Debug.Log("client");
        //GameObject temp = Instantiate(startPlayerPrefab, transform.position, transform.rotation);
        pawn = a.GetComponent<BaseControlable>();
        CmdSetupPawn(a);
        //NetworkServer.Spawn(temp);
        //RpcSetupPawn( temp.GetComponent<NetworkIdentity>());
    }

    [Command]
    public void CmdSetupPawn(NetworkIdentity a)
    {
        if (isLocalPlayer) Debug.Log("server");
        pawn = a.GetComponent<BaseControlable>();
        RpcSetupPawn(a);
    }

    [ClientRpc]
    public void RpcSetupPawn(NetworkIdentity a)
    {
        pawn = a.GetComponent<BaseControlable>();
        if (isLocalPlayer)
        {
            this.ClientCamera = GameObject.FindWithTag("MainCamera").transform.parent.gameObject;
            ClientCamera.transform.position = pawn.transform.position;
            ClientCamera.transform.SetParent(pawn.transform);
        }
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
            moveCom.vertical = vertical;
            moveCom.horizontal = horizontal;
            moveCom.movespeed = pawn.movementSpeed;
            moveCom.predict(pawn.netIdentity);
        }

        // shoot
        if (Input.GetKeyDown(jumpKey))
        {
            // test for ground
            RaycastHit hit;
            Physics.Raycast(pawn.transform.position,Vector3.down,out hit, 2);
            if (hit.collider)
            {
                jumpCom.predict(pawn.netIdentity);
            }
        }
    }
}
