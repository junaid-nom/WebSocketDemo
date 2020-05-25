using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
//using WebSocketSharp;
using HybridWebSocket; // Have to use this because C# websocket libraries dont work with WEBGL.
using System.Runtime.Serialization.Formatters.Binary;

public static class BinarySerializer
{
    private static readonly BinaryFormatter Formatter = new BinaryFormatter();

    public static byte[] Serialize(object toSerialize)
    {
        using (var stream = new System.IO.MemoryStream())
        {
            Formatter.Serialize(stream, toSerialize);
            return stream.ToArray();
        }
    }

    public static object Deserialize(byte[] serialized)
    {
        using (var stream = new System.IO.MemoryStream(serialized))
        {
            var result = Formatter.Deserialize(stream);
            return result;
        }
    }
}

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
        ws = WebSocketFactory.CreateInstance("ws://localhost:7268"); // NOTE FOR SOME INSANO REASON 127.0.0.1 wont work but localhost will with hybridsocket
        UserInput testInp = new UserInput();
        testInp.x = 1;
        testInp.y = -1;
        List<bool> buts = new List<bool>();
        buts.Add(true);
        buts.Add(false);
        buts.Add(true);
        testInp.buttonsDown = buts;
        ws.OnMessage += (byte[] msg) =>
        {
            //NetDebug.printBoth("Client Received: " + (msg));
            Message deser = (Message)BinarySerializer.Deserialize(msg);
            NetDebug.printBoth("Client got msg type: " + deser.msgType);
            MessageManager.debugMsg(deser);
        };
        ws.OnOpen += () =>
        {
            NetDebug.printBoth("Client sending string msg then userinput");
            ws.Send(BinarySerializer.Serialize(new StringMessage("ClientOpenTest")));
            ws.Send(BinarySerializer.Serialize(testInp));
        };
        ws.OnError += (string errMsg) => NetDebug.printBoth("got on error " + errMsg);
        ws.OnClose += (WebSocketCloseCode code) => NetDebug.printBoth("got on close " + code);
        
        NetDebug.printBoth("About to start client");
        ws.Connect();
    }
    

    void closeStuff()
    {
        if (ws != null && ws.GetState() == WebSocketState.Open || ws.GetState() == WebSocketState.Connecting)
        {
            NetDebug.printBoth("Closing client");
            ws.Close();
            NetDebug.printBoth("Closed client: " + ws.GetState());
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
