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

    protected override void OnOpen()
    {
        newMsgs.Add(new GotMessage(ID, new OpenMessage()));
    }

    protected override void OnClose(CloseEventArgs e)
    {
        newMsgs.Add(new GotMessage(ID, new CloseMessage()));
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

public static class BetterDict
{
    public static void AddOrCreate<TKey, TCollection, TValue>(
    this Dictionary<TKey, TCollection> dictionary, TKey key, TValue value)
    where TCollection : ICollection<TValue>, new()
    {
        TCollection collection;
        if (!dictionary.TryGetValue(key, out collection))
        {
            collection = new TCollection();
            dictionary.Add(key, collection);
        }
        collection.Add(value);
    }
}



public class Server : MonoBehaviour
{
    private static Dictionary<string, UserManager> uidToUserM = new Dictionary<string, UserManager>();
    private static Dictionary<string, List<Message>> uidToMessageQueue = new Dictionary<string, List<Message>>();
    private static List<Message> broadcastMessageQueue = new List<Message>();
    static WebSocketServer wssv = null;
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
        if (wssv != null && wssv.IsListening)
        {
            // Use while loop and remove 1 at a time so that its more thread safe.
            // If you clear whole list, maybe a message was added right before you cleared.
            while (StoreMessages.newMsgs.Count > 0)
            {
                transferNewMessage(StoreMessages.newMsgs[0]);
                StoreMessages.newMsgs.RemoveAt(0);
            }

            // Call update on all UserManagers.
            foreach (var um in uidToUserM.Values)
            {
                um.customUpdate();
            }

            // Send all messages out at once in a big list
            foreach (var msgs in uidToMessageQueue)
            {
                wssv.WebSocketServices["/"].Sessions.SendTo(BinarySerializer.Serialize(new ListMessage(msgs.Value)), msgs.Key);
                msgs.Value.Clear();
            }
            wssv.WebSocketServices["/"].Sessions.Broadcast(BinarySerializer.Serialize(new ListMessage(broadcastMessageQueue)));
            broadcastMessageQueue.Clear();
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

    public static void sendToSpecificUser(string uid, Message m)
    {
        string connID = uidToUserM[uid].currentConnID;
        //byte[] serializedMsg = BinarySerializer.Serialize(m);
        uidToMessageQueue.AddOrCreate<string, List<Message>, Message> (connID, m);
        //wssv.WebSocketServices["/"].Sessions.SendTo(serializedMsg, connID);
    }

    public static void sendToAll(Message m)
    {
        byte[] serializedMsg = BinarySerializer.Serialize(m);
        //wssv.WebSocketServices["/"].Sessions.Broadcast(serializedMsg);
        broadcastMessageQueue.Add(m);
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
        newum.startup(uid, uid, Constants.playerCharacterPrefab, new Vector3(0, 0, 0));
    }

    public void removeUserManager(string uid)
    {
        // Remove from both uid dicts
        // Delete component
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
