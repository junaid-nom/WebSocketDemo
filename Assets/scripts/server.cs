using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System.Text;
using WebSocketSharp.Server;

using BehaviorList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>;
using ConditionalBehavior = System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>;
using Conditionals = System.Collections.Generic.Dictionary<Condition, bool>;
using ConditionalBehaviorList = System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>;
using AIPriorityList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>>;


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
        if (deser == null)
        {
            Debug.LogWarning("Got null msg????" + deser + " raw: " + e.RawData);
        }
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
    public static bool isOn = false;

    static Dictionary<string, Bot> uidToBot = new Dictionary<string, Bot>();

    public static InspectorDebugger inspectorDebugger;

    private void Awake()
    {
        inspectorDebugger = gameObject.GetComponent<InspectorDebugger>();
    }

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
            // TODO: If bots are inactive, check if UserManager exists, if it does, send the OnClose message to it
            // TODO: Bots generate artificial messages here. Probably just UserInput msgs.
            // Add the msgs to StoreMessages.newMsgs
            foreach (Bot bot in uidToBot.Values)
            {
                if (!Bots.botAlive(bot.state, wssv.WebSocketServices["/"].Sessions.Count, Constants.maxBots))
                {
                    if (uidToUserM.ContainsKey(bot.state.uid))
                    {
                        StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, new CloseMessage()));
                        bot.reset();
                    }
                } else
                {
                    System.Tuple<AIPriorityList, AIMemory> result = bot.ai(bot.aiList, bot.state);
                    bot.state.extraState = result.Item2;
                    UserInput uinp = Bots.getBotAction(result.Item1, bot.state);
                    if (result.Item1 == null)
                    {
                        Debug.Log("null retai");
                    }
                    bot.aiList = result.Item1;
                    //NetDebug.printBoth("Got: uinp " + ((uinp !=null) ? uinp.ToString() : "null") + " for: " + bot.state.uid);
                    inspectorDebugger.addPair(new StringPair(bot.state.uid, bot.ToString()));
                    if (uinp != null)
                    {
                        StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, uinp));
                    }

                    // clear msgs because guaranteed to get a fresh state because this runs at the same time as a "server tick"
                    bot.state.msgs.Clear();
                }
            }

            // Use while loop and remove 1 at a time so that its more thread safe.
            // If you clear whole list, maybe a message was added right before you cleared.
            while (StoreMessages.newMsgs.Count > 0)
            {
                transferNewMessage(StoreMessages.newMsgs[0]);
                StoreMessages.newMsgs.RemoveAt(0);
            }

            // Call update on all UserManagers.
            UserManager[] copyUM = new UserManager[uidToUserM.Values.Count];
            uidToUserM.Values.CopyTo(copyUM, 0);
            // Since could call deleteSelf, make a copy of the list to iterate through so don't modify list as you loop
            foreach (var um in copyUM)
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
    
    public static void removeUserManager(string uid)
    {
        uidToUserM.Remove(uid);
    }

    void transferNewMessage(GotMessage gm)
    {
        if (gm.uid == null)
        {
            if (gm.m != null)
                Debug.LogWarning("Got null conn id msg: " + gm.m.GetType());
            else
            {
                Debug.LogWarning("Got null conn id msg: " + gm.m);
            }
            return;
        }
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
        if (uid.Contains(BotState.BOTUIDPREFIX))
        {
            // TODO: if uid is server (bot), then send to them directly
            uidToBot[uid].state.msgs.Add(m);
            tryAddToBotCharState(uid, m);
        }
        else
        {
            string connID = uidToUserM[uid].currentConnID;
            uidToMessageQueue.AddOrCreate<string, List<Message>, Message>(connID, m);
        }
    }

    public static void sendToAll(Message m)
    {
        byte[] serializedMsg = BinarySerializer.Serialize(m);
        //wssv.WebSocketServices["/"].Sessions.Broadcast(serializedMsg);
        broadcastMessageQueue.Add(m);

        // TODO: Send msg directly to all things with UID that has server in it
        foreach (var bot in uidToBot.Values)
        {
            bot.state.msgs.Add(m);
            tryAddToBotCharState(bot.state.uid, m);
        }
    }

    public static void tryAddToBotCharState(string uid, Message m)
    {
        if (uidToBot.ContainsKey(uid))
        {
            if (m.msgType == 1)
            {
                CopyMovement cp = (CopyMovement)m;
                if (cp.objectInfo.uid == uid)
                {
                    uidToBot[uid].state.charState.Insert(0, new CharacterState(cp));
                }
            }
        }
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

    public Vector3 getSpawnLocation()
    {
        float x = Random.Range(-1 * Constants.spawnXRange, Constants.spawnXRange);
        float z = Random.Range(-1 * Constants.spawnZRange, Constants.spawnZRange);
        return new Vector3(x, 0, z);
    }

    public void addUserManager(string uid)
    {
        UserManager newum = gameObject.AddComponent<UserManager>();
        uidToUserM.Add(uid, newum);
        
        newum.startup(uid, uid, Constants.playerCharacterPrefab, getSpawnLocation());
    }

    public void startServer()
    {
        isOn = true;
        System.Console.SetOut(new DebugLogWriter());
        NetDebug.printBoth("about to start wssv ");
        wssv = new WebSocketServer("ws://127.0.0.1:7268");
        wssv.AddWebSocketService<StoreMessages>("/");

        NetDebug.printBoth("starting wssv ");
        wssv.Start();
        NetDebug.printBoth("started wssv " + wssv.IsListening);

        //initialize Bots
        {
            Bot b1 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(0), null);
            Bot b2 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(1), null);
            Bot b3 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(2), null);
            Bot b4 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(3), null);
            Bot b5 = new Bot(Bots.AttackAndChaseOrRunawayBot, new BotState(4), null);

            uidToBot.Add(b1.state.uid, b1);
            uidToBot.Add(b2.state.uid, b2);
            uidToBot.Add(b3.state.uid, b3);
            uidToBot.Add(b4.state.uid, b4);
            uidToBot.Add(b5.state.uid, b5);
        }
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
