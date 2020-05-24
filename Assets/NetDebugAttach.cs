using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NetDebugAttach : MonoBehaviour {
    Text myText;
	// Use this for initialization
	void Awake () {
        myText = GetComponent<Text>();
	}

    void Update()
    {
        myText.text = NetDebug.getText();
    }
}
