using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class nameDisplay : MonoBehaviour
{
    public PlayerObject playerObject;
    public TextMeshPro textMeshPro;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = "" + playerObject.playerName;
    }
}
