using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System.Text;
using WebSocketSharp.Server;

public struct GotMessage
{
    public string uid;
    public Message m;
    public GotMessage(string uid, Message m)
    {
        this.uid = uid;
        this.m = m;
    }
}

public class StoreMessages : WebSocketBehavior
{
    public static List<GotMessage> newMsgs = new List<GotMessage>();

    protected override void OnMessage(MessageEventArgs e)
    {
        // TODO: Eventually have login and this will prob be username->usermanager and there will be ID -> username or something
        // For now treat every new connection as a completely new user
        

        //NetDebug.printBoth("Server Got msg " + e.Data + " Raw " + Encoding.UTF8.GetString(e.RawData));

        //Send(e.Data + " t: " + System.DateTime.Now.ToString("h:mm:ss tt"));
        Message deser = (Message)BinarySerializer.Deserialize(e.RawData);
        newMsgs.Add(new GotMessage(ID, deser));

        /*
        Send(BinarySerializer.Serialize(new StringMessage(" Server got your msgtype: " + deser.msgType)));
        NetDebug.printBoth("Server got msg type: " + deser.msgType);
        MessageManager.debugMsg(deser);
        CopyMovement cptest = new CopyMovement();
        cptest.anim_state = "anim2";
        cptest.ignoreRotation = false;
        cptest.localPosition = new Vector3(1, 2, 3);
        cptest.localRotation = Quaternion.Euler(10, 20, 30);
        cptest.normalizedTime = .2f;
        Send(BinarySerializer.Serialize(cptest));
        */
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

public class Server : MonoBehaviour
{
    private static Dictionary<string, UserManager> uidToUserM = new Dictionary<string, UserManager>();
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
        // Use while loop and remove 1 at a time so that its more thread safe.
        // If you clear whole list, maybe a message was added right before you cleared.
        while (StoreMessages.newMsgs.Count > 0)
        {
            transferNewMessage(StoreMessages.newMsgs[0]);
            StoreMessages.newMsgs.RemoveAt(0);
        }
        
    }

    void transferNewMessage(GotMessage gm)
    {
        UserManager um = getUserManager(gm.uid);

        if (um == null)
        {
            addUserManager(gm.uid);
            um = Server.getUserManager(gm.uid);
        }
        um.addMessage(gm.m);
    }

    public static UserManager getUserManager(string uid)
    {
        if (uidToUserM.ContainsKey(uid))
        {
            return uidToUserM[uid];
        } else
        {
            return null;
        }
    }

    public void addUserManager(string uid)
    {
        UserManager newum = gameObject.AddComponent<UserManager>();
        uidToUserM.Add(uid, newum);
        newum.startup(uid, Constants.playerCharacterPrefab, new Vector3(0, 0, 0));
    }

    public void startServer()
    {
        System.Console.SetOut(new DebugLogWriter());

        NetDebug.printBoth("about to start wssv ");
        wssv = new WebSocketServer("ws://127.0.0.1:7268");
        wssv.AddWebSocketService<StoreMessages>("/");

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
