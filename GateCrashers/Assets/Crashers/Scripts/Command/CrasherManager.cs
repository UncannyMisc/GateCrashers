using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CrasherManager : NetworkManager
{
    
    [Header("Spawn Info")]
    [FormerlySerializedAs("m_PlayerClientPrefab")] public GameObject playerPawnPrefab;
    public GameObject BeerCratePrefab;
    public GameObject ActiveBeerCrate;
    public Transform crateSpawn;

    public UnityAction restart;

    public override void Start()
    {
        restart += CmdRestart;
        
    }

    [Command] 
    public void CmdRestart()
    {
        ActiveBeerCrate.transform.position = crateSpawn.position;
    }
    
    public override void OnValidate()
    {
        // add transport if there is none yet. makes upgrading easier.
        if (transport == null)
        {
            // was a transport added yet? if not, add one
            transport = GetComponent<Transport>();
            if (transport == null)
            {
                transport = gameObject.AddComponent<TelepathyTransport>();
                Debug.Log("NetworkManager: added default Transport because there was none yet.");
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }

        maxConnections = Mathf.Max(maxConnections, 0); // always >= 0

        if (playerPrefab != null && playerPrefab.GetComponent<NetworkIdentity>() == null&&playerPawnPrefab != null && playerPawnPrefab.GetComponent<NetworkIdentity>() == null)
        {
            Debug.LogError("NetworkManager - playerPrefab & playerClientPrefab must have a NetworkIdentity.");
            playerPrefab = null;
            playerPawnPrefab = null;
        }
    }
    
    public override void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage)
    {
        if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerAddPlayer");

        if (playerPrefab == null)
        {
            Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
            return;
        }

        if (playerPrefab.GetComponent<NetworkIdentity>() == null)
        {
            Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
            return;
        }
        if (playerPawnPrefab == null)
        {
            Debug.LogError("The PlayerClientPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
            return;
        }

        if (playerPawnPrefab.GetComponent<NetworkIdentity>() == null)
        {
            Debug.LogError("The PlayerClientPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
            return;
        }

        if (conn.playerController != null)
        {
            Debug.LogError("There is already a player for this connections.");
            return;
        }

        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);
        GameObject playerPawn = startPos != null
            ? Instantiate(playerPawnPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPawnPrefab);

        NetworkServer.AddPlayerForConnection(conn, player);
        NetworkServer.SpawnWithClientAuthority(playerPawn,conn);
        player.GetComponent<Client>().SetupPawn(playerPawn.GetComponent<NetworkIdentity>());
        // spawn ball if two players
        if (numPlayers == 1)
        {
            ActiveBeerCrate = Instantiate(BeerCratePrefab, crateSpawn.position, crateSpawn.rotation);
            NetworkServer.Spawn(ActiveBeerCrate);
        }
        else if(numPlayers>1&& !ActiveBeerCrate)
        {
            ActiveBeerCrate = Instantiate(BeerCratePrefab, crateSpawn.position, crateSpawn.rotation);
            NetworkServer.Spawn(ActiveBeerCrate);
        }
        
        FindObjectOfType<EndingScript>().gameRestart.AddListener(restart);
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        GameObject temp = conn.playerController.GetComponent<Client>().pawn.gameObject;
        
        // destroy player pawn
        if (temp != null)
            NetworkServer.Destroy(temp);

        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }
}
