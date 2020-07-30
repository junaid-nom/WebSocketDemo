using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject serverButton;
    public GameObject clientButton;
    public GameObject clientNameInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggleConnectButtons()
    {
        serverButton.SetActive(!serverButton.activeSelf);
        clientButton.SetActive(!clientButton.activeSelf);
        clientNameInput.SetActive(!clientNameInput.activeSelf);
    }
}
