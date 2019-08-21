using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    private TextMeshPro score;
    public Client client;
    
    
    // Start is called before the first frame update
    void Start()
    {
        score = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        score.SetText(client.score + "/99");
    }
}
