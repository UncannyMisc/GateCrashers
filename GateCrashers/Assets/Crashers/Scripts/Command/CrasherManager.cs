using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class CrasherManager : NetworkManager
{
    
    [Header("Spawn Info")]
    [FormerlySerializedAs("m_PlayerClientPrefab")] public GameObject playerClientPrefab;
    
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

        if (playerPrefab != null && playerPrefab.GetComponent<NetworkIdentity>() == null&&playerClientPrefab != null && playerClientPrefab.GetComponent<NetworkIdentity>() == null)
        {
            Debug.LogError("NetworkManager - playerPrefab & playerClientPrefab must have a NetworkIdentity.");
            playerPrefab = null;
            playerClientPrefab = null;
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
        if (playerClientPrefab == null)
        {
            Debug.LogError("The PlayerClientPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
            return;
        }

        if (playerClientPrefab.GetComponent<NetworkIdentity>() == null)
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
        GameObject playerClient = startPos != null
            ? Instantiate(playerClientPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerClientPrefab);

        NetworkServer.AddPlayerForConnection(conn, player);
        NetworkServer.SpawnWithClientAuthority(playerClient,conn);
        player.GetComponent<Client>().SetupPawn(playerClient.GetComponent<NetworkIdentity>());
    }
}
