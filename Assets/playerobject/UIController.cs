using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject serverButton;

    public List<GameObject> connectUI = new List<GameObject>();
    public List<GameObject> inGameUI = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        serverButton.SetActive(true);
#else
        serverButton.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggleConnectButtons()
    {
        connectUI.ForEach(g => g.SetActive(!g.activeSelf));
    }

    public void turnOffConnectButtons()
    {
        connectUI.ForEach(g => g.SetActive(false));
    }

    public void turnOnInGameUI()
    {
        inGameUI.ForEach(g => g.SetActive(true));
    }
    public void turnOffInGameUI()
    {
        inGameUI.ForEach(g => g.SetActive(false));
    }

    public void inGameMode()
    {
        turnOffConnectButtons();
        turnOnInGameUI();
    }
}
