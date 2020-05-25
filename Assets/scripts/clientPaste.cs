using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using HybridWebSocket;

public class clientPaste : MonoBehaviour
{
    public string server;
    // ws://echo.websocket.org
    // Use this for initialization
    void Start()
    {

        // Create WebSocket instance
        WebSocket ws = WebSocketFactory.CreateInstance(server);

        // Add OnOpen event listener
        ws.OnOpen += () =>
        {
            NetDebug.printBoth(server + "WS connected!");
            NetDebug.printBoth(server + "WS state: " + ws.GetState().ToString());

            ws.Send(Encoding.UTF8.GetBytes("Hello from Unity 3D!"));
        };

        // Add OnMessage event listener
        ws.OnMessage += (byte[] msg) =>
        {
            NetDebug.printBoth(server + " WS received message: " + Encoding.UTF8.GetString(msg));

            //ws.Close();
        };

        // Add OnError event listener
        ws.OnError += (string errMsg) =>
        {
            NetDebug.printBoth(server + "WS error: " + errMsg);
        };

        // Add OnClose event listener
        ws.OnClose += (WebSocketCloseCode code) =>
        {
            NetDebug.printBoth(server + "WS closed with code: " + code.ToString());
        };

        // Connect to the server
        ws.Connect();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
