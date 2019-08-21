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
    public KeyCode interactKey = KeyCode.Mouse0;

    public string vertAxis = "Vertical";
    public string horiAxis = "Horizontal";
    public bool jumping = false;

    [Header("Possessions")]
    public BaseControlable pawn;

    [Header("Rotating")] 
    public GameObject meshOwner;
    public GameObject meshObj;


    public override void OnStartLocalPlayer()
    {
        meshObj = meshOwner.transform.GetChild(0).gameObject;
        
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

    #region Setuppawnandcamera

    [Client]
    public void SetupPawn(NetworkIdentity a)
    {
        if (isLocalPlayer) Debug.Log("client");
        //GameObject temp = Instantiate(startPlayerPrefab, transform.position, transform.rotation);
        pawn = a.GetComponent<BaseControlable>();
        pawn.OnPosses(this);
        CmdSetupPawn(a);
        //NetworkServer.Spawn(temp);
        //RpcSetupPawn( temp.GetComponent<NetworkIdentity>());
    }

    [Command]
    public void CmdSetupPawn(NetworkIdentity a)
    {
        if (isLocalPlayer) Debug.Log("server");
        pawn = a.GetComponent<BaseControlable>();
        pawn.OnPosses(this);
        RpcSetupPawn(a);
    }

    [ClientRpc]
    public void RpcSetupPawn(NetworkIdentity a)
    {
        pawn = a.GetComponent<BaseControlable>();
        pawn.OnPosses(this);
        if (isLocalPlayer)
        {
            this.ClientCamera = GameObject.FindWithTag("MainCamera").transform.parent.gameObject;
            ClientCamera.transform.position = pawn.transform.position;
            ClientCamera.transform.SetParent(pawn.transform);
        }
    }
    #endregion

    private void OnDestroy()
    {
        // if local, set the camera free
        if(ClientCamera!=null)
            ClientCamera.transform.SetParent(null);
    }

    void Update()
    {
        //hold the item
        if (pawn&&pawn.holding)
        {
            PickUp temp = FindObjectOfType<PickUp>();
            //make the item get picked up
            pawn.interactStrat.Update(Time.deltaTime, pawn.netIdentity, temp.netIdentity);
        }
        
        // movement for local player
        if (!isLocalPlayer) return;

        float vertical = Input.GetAxis(vertAxis);
        float horizontal = Input.GetAxis(horiAxis);
        
        if (pawn.holding)
        {
            //move set weird
            vertical = vertical + ((Mathf.PerlinNoise(Time.time, 1) - 0.5f)*2);
            horizontal = horizontal + ((Mathf.PerlinNoise(Time.time * 2, 1) - 0.5f)*2);
            
            //mess with mesh
            meshObj.transform.Rotate(horizontal, 0, vertical);



        }
        
        //actual movement
        
        if (vertical != 0 || horizontal != 0)
        {
            pawn.moveStrat.Held(Time.deltaTime, pawn.GetComponent<Rigidbody>(),
                new Vector3(horizontal, 0, vertical));
            //moveCom.predict(pawn.netIdentity);
        }

        RaycastHit hit;
        Physics.Raycast(pawn.GetComponent<Rigidbody>().position, Vector3.down, out hit, 0.4f);
        if (hit.collider)
        {
            if (!jumping)
            {
                // jump
                if (Input.GetKeyDown(jumpKey)&&hit.collider)
                {
                    pawn.jumpStrat.JustPressed(Time.deltaTime,pawn.GetComponent<Rigidbody>(),hit.collider);
                    jumping = true;
                }
            }
        }
        else if(jumping)
        {
            jumping = false;
        }

        //pick up the item
        if (Input.GetKeyDown(interactKey))
        {
            Cmdpickup();
        }
    }

    [Command]
    public void Cmdpickup()
    {
        PickUp temp = FindObjectOfType<PickUp>();
        if (!pawn.holding)
        {
            if (!temp.held)
            {
                if (pawn.close) temp.Check(this.netIdentity, pawn.netIdentity);
            }
            else
            {
                temp.Drop();
            }
                
        }
        else temp.Check(this.netIdentity, pawn.netIdentity);
    }
}
