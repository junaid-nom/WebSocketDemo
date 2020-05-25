using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System.Text;
using WebSocketSharp.Server;


public class Echo : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {   
        //NetDebug.printBoth("Server Got msg " + e.Data + " Raw " + Encoding.UTF8.GetString(e.RawData));

        //Send(e.Data + " t: " + System.DateTime.Now.ToString("h:mm:ss tt"));
        Message deser = (Message)BinarySerializer.Deserialize(e.RawData);
        NetDebug.printBoth("Server got msg type: " + deser.msgType);
        MessageManager.debugMsg(deser);

        Send(BinarySerializer.Serialize(new StringMessage(" Server got your msgtype: " + deser.msgType)));

        CopyMovement cptest = new CopyMovement();
        cptest.anim_state = "anim2";
        cptest.ignoreRotation = false;
        cptest.localPosition = new Vector3(1, 2, 3);
        cptest.localRotation = Quaternion.Euler(10, 20, 30);
        cptest.normalizedTime = .2f;
        Send(BinarySerializer.Serialize(cptest));
    }
}

public class DebugLogWriter : System.IO.TextWriter
{
    public override void Write(string value)
    {
        base.Write(value);
        Debug.LogError(value);
        NetDebug.printBoth(value);
    }
    public override void WriteLine(string value)
    {
        base.WriteLine();
        Debug.LogError(value);
        NetDebug.printBoth(value);
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

        NetDebug.printBoth("about to start wssv ");
        wssv = new WebSocketServer("ws://127.0.0.1:7268");
        wssv.AddWebSocketService<Echo>("/");

        NetDebug.printBoth("starting wssv ");
        wssv.Start();
        NetDebug.printBoth("started wssv " + wssv.IsListening);
    }

    void closeStuff()
    {
        if (wssv != null && wssv.IsListening)
        {
            NetDebug.printBoth("Closing server");
            wssv.Stop();
            NetDebug.printBoth("Closed server listening: " + wssv.IsListening);
        }
    }

    void OnApplicationQuit()
    {
        NetDebug.printBoth("Quit Server...");
        closeStuff();
    }
    void OnDestroy()
    {
        NetDebug.printBoth("Destroyed Server...");
        closeStuff();
    }
}
