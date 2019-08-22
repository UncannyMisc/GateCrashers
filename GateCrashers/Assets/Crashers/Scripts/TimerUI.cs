using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GateCrashers;
using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI score;
    public Client client;
    public List<Client> players = new List<Client>();
    
    
    // Start is called before the first frame update
    void Start()
    {
        score = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (client)
        {
            string temp = "your score: "+client.score + "/99"+"\n";
            if (players.Count != 0)
            {
                foreach (Client player in players)
                {
                    temp += player.score + "/99"+"\n";
                }
            }
            score.SetText(temp);
        }
    }

    public void setup()
    {
        players = GameObject.FindObjectsOfType<Client>().ToList();
        players.Remove(client);
    }
}
