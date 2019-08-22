using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScript : NetworkBehaviour
{
    // when someones score == 99, their UI becomes YOU WIN.  Others becomes YOU LOSE.  pressing any key makes the scene reload

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
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }
    }
}
