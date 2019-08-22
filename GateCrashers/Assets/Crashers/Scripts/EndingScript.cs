using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScript : NetworkBehaviour
{
    [SyncVar]
    public bool gameEnded;

    public void Update()
    {
        //scene reloads if player hits a button - currently completely goes out of game
        //both server and client can do this so that neither can still move in game
        if (gameEnded)
        {
            if (Input.anyKey)
            {
                NetworkServer.DisconnectAll();
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }
    }
}
