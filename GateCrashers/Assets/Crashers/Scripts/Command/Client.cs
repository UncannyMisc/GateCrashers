using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Client : NetworkBehaviour
{
    [Header("client objects")] public GameObject ClientCamera;

    public GameObject startPlayerPrefab;

    [Header("Key Bindings")] public KeyCode jumpKey = KeyCode.Space;
    public KeyCode interactKey = KeyCode.Mouse0;

    public string vertAxis = "Vertical";
    public string horiAxis = "Horizontal";
    public bool jumping = false;

    [Header("Possessions")] public BaseControlable pawn;

    [Header("Movement")] [SyncVar] public float vertical;
    [SyncVar] public float horizontal;
    public GameObject meshObj;
    public float wobble = 20;
    [SyncVar] public int score = 0;

    public float time = 0;

    //the unsorted
    public UnityAction dropCall;
    public UnityAction restartCall;
    [FormerlySerializedAs("forceDroppped")] public bool forceDrop;
    public int timer2;

    public Material myMat;

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
        meshObj = pawn.transform.GetChild(0).gameObject;
        dropCall += CmdDrop;
        restartCall += CmdRestart;
        FindObjectOfType<EndingScript>().gameRestart.AddListener(restartCall);

        myMat = meshObj.GetComponentInChildren<Renderer>().material;
        float temp = Random.Range(0f, 1f);
        myMat.SetFloat("Vector1_6FD2A65A", temp);
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
            ClientCamera.GetComponentInChildren<TimerUI>().client = this;
            ClientCamera.GetComponentInChildren<TimerUI>().setup();

            Debug.Log("setupcamera");
            
        }
        else
        {
            GameObject temp = GameObject.FindWithTag("MainCamera").transform.parent.gameObject;
            temp.GetComponentInChildren<TimerUI>().players.Add(this);
        }

        meshObj = pawn.transform.GetChild(0).gameObject;
        dropCall += CmdDrop;
        restartCall += CmdRestart;
        FindObjectOfType<EndingScript>().gameRestart.AddListener(restartCall);
        
    }

    #endregion

    private void OnDestroy()
    {
        // if local, set the camera free

        if (ClientCamera != null)
            ClientCamera.transform.SetParent(null);
    }
    [Command] 
    public void CmdRestart()
    {
        score = 0;
        wobble = 20;
        
        PickUp temp = FindObjectOfType<PickUp>();
        FindObjectOfType<PickUp>().Dropped.AddListener(dropCall);
        forceDrop = false;
        temp.Drop();

    }

    void Update()
    {
        //hold the item
        if (pawn && pawn.holding)
        {
            PickUp temp = FindObjectOfType<PickUp>();
            //make the item get picked up
            pawn.interactStrat.Update(Time.deltaTime, pawn.netIdentity, temp.netIdentity);
        }

        //mess with mesh
        meshObj.transform.rotation = Quaternion.Euler(-vertical * wobble, 0, horizontal * wobble);
        if (isServer)
        {
            if (score < 99)
            {
                if (pawn.holding)
                {
                    if (time >= 2)
                    {
                        time = 0;
                        score++;
                        wobble++;
                    }

                    time += Time.deltaTime;
                }
                else
                {
                    time = 0;
                }
            }
            else
            {
                EndingScript end = FindObjectOfType<EndingScript>();
                end.gameEnded = true;
                //change ui
            }
        }

        // movement for local player
        if (!isLocalPlayer) return;

        if (pawn.movementSpeed == 0)
        {
            if (timer2 >= 2)
            {
                timer2 = 0;
                pawn.movementSpeed = 200;
            }
            else
            {
                time += Time.deltaTime;
            }
        }


        vertical = Input.GetAxis(vertAxis);
        horizontal = Input.GetAxis(horiAxis);

        if (pawn.holding)
        {
            //move set weird
            vertical = vertical + ((Mathf.PerlinNoise(Time.time, 1) - 0.5f) * 2);
            horizontal = horizontal + ((Mathf.PerlinNoise(Time.time * 2, 1) - 0.5f) * 2);
        }

        CmdSync(horizontal, vertical);

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
                if (Input.GetKeyDown(jumpKey) && hit.collider)
                {
                    pawn.jumpStrat.JustPressed(Time.deltaTime, pawn.GetComponent<Rigidbody>(), hit.collider);
                    jumping = true;
                }
            }
        }
        else if (jumping)
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
    public void CmdSync(float xaxis, float yaxis)
    {
        vertical = yaxis;
        horizontal = xaxis;
    }

    [Command]
    public void CmdDrop()
    {
        FindObjectOfType<PickUp>().Dropped.RemoveListener(dropCall);
        pawn.holding = false;
        pawn.close = false;
        myMat.SetInt("Boolean_D7C5BB61", 0);
        if (forceDrop)
        {
            pawn.movementSpeed = 0;

        }
        else forceDrop = true;
    }

    [Command]
    public void Cmdpickup()
    {
        PickUp temp = FindObjectOfType<PickUp>();
        FindObjectOfType<PickUp>().Dropped.AddListener(dropCall);
        if (temp.holder != this.netIdentity)
        {
            if (pawn.close)
            {
                if (!temp.held)
                {
                    pawn.holding = true;
                    temp.held = true;
                    temp.PickUpBox(this.netIdentity);
                    myMat.SetInt("Boolean_D7C5BB61", 1);
                }
                else
                {
                    temp.ForceDrop();
                }
            }
        }

        else
        {
            forceDrop = false;
            temp.Drop();
        }

    }

}
