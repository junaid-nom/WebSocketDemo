using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class client : MonoBehaviour
{
    public bool autoStartClient;

    WebSocket ws;
    // Start is called before the first frame update
    void Start()
    {
        if (autoStartClient)
        {
            startClient();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startClient()
    {
        ws = new WebSocket("ws://localhost:7268");
        ws.OnMessage += (sender, e) => NetDebug.printBoth("Client Received: " + e.Data);
        ws.OnOpen += (sender, e) => ws.Send("clientTest");
        ws.Connect();
    }

    void closeStuff()
    {
        if (ws != null && ws.IsAlive)
        {
            NetDebug.printBoth("Closing client");
            ws.Close();
            NetDebug.printBoth("Closed client: " + ws.IsAlive);
        }
    }

    void OnApplicationQuit()
    {
        NetDebug.printBoth("Quit Client...");
        closeStuff();
    }
    void OnDestroy()
    {
        NetDebug.printBoth("Destroyed Client...");
        closeStuff();
    }
}
