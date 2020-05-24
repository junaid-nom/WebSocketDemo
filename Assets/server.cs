using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class Echo : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log("Got msg " + e.Data);

        Send(e.Data + " t: " + System.DateTime.Now.ToString("h:mm:ss tt"));
    }
}

public class DebugLogWriter : System.IO.TextWriter
{
    public override void Write(string value)
    {
        base.Write(value);
        Debug.LogError(value);
    }
    public override void WriteLine(string value)
    {
        base.WriteLine();
        Debug.LogError(value);
    }

    public override System.Text.Encoding Encoding
    {
        get { return System.Text.Encoding.UTF8; }
    }
}

public class server : MonoBehaviour
{
    WebSocketServer wssv = null;
    public bool autoStartServer;
    // Start is called before the first frame update
    void Start()
    {
        if (autoStartServer)
        {
            startServer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //wssv.Log.Error("wtfff");
        //System.Console.Out.WriteLine("poop1");
        //System.Console.Out.Write("poop2");

        //string output = 
        //Debug.LogWarning("Got socket Error: " + output);
    }

    public void startServer()
    {
        System.Console.SetOut(new DebugLogWriter());
        Debug.Log("about to start wssv ");
        wssv = new WebSocketServer("ws://localhost:7268");
        wssv.AddWebSocketService<Echo>("/");

        Debug.Log("starting wssv ");
        wssv.Start();
        Debug.Log("started wssv " + wssv.IsListening);
    }

    void closeStuff()
    {
        if (wssv != null && wssv.IsListening)
        {
            Debug.Log("Closing server");
            wssv.Stop();
            Debug.Log("Closed server listening: " + wssv.IsListening);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Quit...");
        closeStuff();
    }
    void OnDestroy()
    {
        Debug.Log("Destroyed...");
        closeStuff();
    }
}
