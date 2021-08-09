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
using HybridWebSocket;

public class ServerTest : MonoBehaviour
{
    

    private static Dictionary<string, UserManager> uidToUserM = new Dictionary<string, UserManager>();
    private static Dictionary<string, List<Message>> uidToMessageQueue = new Dictionary<string, List<Message>>();
    private static List<Message> broadcastMessageQueue = new List<Message>();
    public static bool isOn = false;

    static Dictionary<string, Bot> uidToBot = new Dictionary<string, Bot>();
    static Dictionary<string, WorldItem> objToItems = new Dictionary<string, WorldItem>();

    public static InspectorDebugger inspectorDebugger;

    public static Dictionary<string, List<PlayerCollision>> playerCollisionsThisFrame = new Dictionary<string, List<PlayerCollision>>();


    static WebSocketServer wssv = null;
    bool startedWebSocket = false;
    static List<HybridWebSocket.WebSocket> clients = new List<HybridWebSocket.WebSocket>();
    public static List<string> connIds = new List<string>();

    static float timer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (Constants.testing)
        {
            int mode = 2;

            if (mode == 0)
            {
                for (int i = 0; i < 1000; i++)
                {
                    string uid = "test" + i;
                    var cp = new CopyMovement(new NetworkObjectInfo("test" + i, NetworkObjectType.playerCharacter, uid), new Vector3(0 + i, 1 + i, 2 + i), new SerializableQuaternion(0, 1, 2, 3), "testanime" + i, 1 + i, false, 1 + i, WeaponType.sword, 2 + i, "tester" + i);
                    StoreMessages.addMsg(new GotMessage(uid, cp));
                }

                UpdateServerStyle();
            }
            else if (mode == 1)
            {
                UpdateBasic();
            }
            else if (mode == 2)
            {
                if (!startedWebSocket)
                {
                    Debug.Log("Starting test server");
                    startedWebSocket = true;
                    System.Console.SetOut(new DebugLogWriter());
                    NetDebug.printBoth("about to start wssv at " + Constants.portServer);
                    wssv = new WebSocketServer(Constants.portServer); // NEED to just use this format of just putting port or it wont work properly with remote server
                    wssv.AddWebSocketService<StoreMessages>("/");

                    NetDebug.printBoth("starting wssv ");
                    wssv.Start();
                    NetDebug.printBoth("started wssv " + wssv.IsListening);
                }
                else
                {
                    if (wssv != null && wssv.IsListening)
                    {
                        while (clients.Count < 10)
                        {
                            Debug.Log($"Adding client at {System.DateTime.Now.ToString("h:mm:ss tt")}");
                            startAClient();
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            string uid = "test" + i;
                            var cp = new CopyMovement(new NetworkObjectInfo("test" + i, NetworkObjectType.playerCharacter, uid), new Vector3(0 + i, 1 + i, 2 + i), new SerializableQuaternion(0, 1, 2, 3), "testanime" + i, 1 + i, false, 1 + i, WeaponType.sword, 2 + i, "tester" + i);
                            broadcastMessageQueue.Add(cp);
                            foreach(var connId in connIds)
                            {
                                uidToMessageQueue.AddOrCreate<string, List<Message>, Message>(connId, cp);
                            }
                        }
                        for (int x = 0; x < 100; x++)
                        {
                            //send inputs back to server
                            UserInput inp = new UserInput();
                            inp.buttonsDown = new List<bool>() { false, true, false, true, false };
                            inp.target = new Vector3(0, 1, 2);
                            inp.x = -1;
                            inp.y = 1;
                            foreach (var conn in clients)
                            {
                                if (conn.GetState() == HybridWebSocket.WebSocketState.Open)
                                {
                                    conn.Send(BinarySerializer.Serialize(inp));
                                }
                            }
                        }
                        List<string> dced = new List<string>();
                        foreach (var msgs in uidToMessageQueue)
                        {
                            try
                            {
                                var connids = new List<string>(wssv.WebSocketServices["/"].Sessions.ActiveIDs);
                                if(connids.Contains(msgs.Key)) {
                                    var toSend = BinarySerializer.Serialize(new ListMessage(msgs.Value));
                                    wssv.WebSocketServices["/"].Sessions.SendTo(toSend, msgs.Key);
                                } else
                                {
                                    dced.Add(msgs.Key);
                                }
                            }
                            catch (System.InvalidOperationException)
                            {
                                Debug.Log("Couldnt send msg probably dced player");
                            }
                            msgs.Value.Clear();
                        }
                        dced.ForEach(dc => uidToMessageQueue.Remove(dc));
                        wssv.WebSocketServices["/"].Sessions.Broadcast(BinarySerializer.Serialize(new ListMessage(broadcastMessageQueue)));
                        broadcastMessageQueue.Clear();

                        var mm = StoreMessages.popMsg();
                        while (mm != null)
                        {
                            mm = StoreMessages.popMsg();
                        }
                    }
                    if (Time.time - timer > 180 && false)
                    {
                        timer = Time.time;
                        clients[0].Close();
                        clients.RemoveAt(0);
                        Debug.Log($"Removing client at {System.DateTime.Now.ToString("h:mm:ss tt")}");
                    }
                }
            }
        }
    }

    public void startAClient()
    {
        string connectionTo = $"ws://{(Constants.localServer)}:{Constants.portServer}"; //not sure if this port is fuked
        NetDebug.printBoth($"Connection to: {connectionTo}");
        var ws = WebSocketFactory.CreateInstance(connectionTo); // NOTE FOR SOME INSANO REASON 127.0.0.1 wont work but localhost will with hybridsocket

        ws.OnMessage += (byte[] msg) =>
        {
            //NetDebug.printBoth("Client Received: " + (msg));
            Message deser = (Message)BinarySerializer.Deserialize(msg);
            
            //NetDebug.printBoth("Client got msg type: " + deser.msgType);
            //MessageManager.debugMsg(deser);
        };
        ws.OnOpen += () =>
        {
            //ws.Send(BinarySerializer.Serialize(newName));
        };
        ws.OnError += (string errMsg) => NetDebug.printBoth("got on error " + errMsg);
        ws.OnClose += (WebSocketCloseCode code) => NetDebug.printBoth("got on close " + code);

        NetDebug.printBoth("About to start a client");
        ws.Connect();
        clients.Add(ws);
    }


    MessageManager storeMessages = new MessageManager();
    int updateIndex = 0;
    void UpdateBasic()
    {
        updateIndex += 1;
        for (int i = 0; i < 1000; i++)
        {
            var cp = new CopyMovement(new NetworkObjectInfo("test"+i, NetworkObjectType.playerCharacter, "test"+i), new Vector3(0+i, 1+i, 2+i), new SerializableQuaternion(0, 1, 2, 3), "testanime"+i, 1+i, false, 1+i, WeaponType.sword, 2+i, "tester"+i);
            storeMessages.addMessage(cp);
        }

        if (updateIndex == 5)
        {
            updateIndex = 0;
            if (Random.Range(0f,1f) < .5f)
            {
                var getcp = storeMessages.popMessage<CopyMovement>();
                while (getcp != null)
                {
                    getcp = storeMessages.popMessage<CopyMovement>();
                }
            } else
            {
                var cps = storeMessages.popAllMessages<CopyMovement>();
            }
        }
    }

    void UpdateServerStyle()
    {
        if (true)
        {
            // TODO: If bots are inactive, check if UserManager exists, if it does, send the OnClose message to it
            // TODO: Bots generate artificial messages here. Probably just UserInput msgs.
            // Add the msgs to StoreMessages.newMsgs
            //foreach (Bot bot in uidToBot.Values)
            //{
            //    if (!Bots.botAlive(bot.state, 0, Constants.maxBots))
            //    {
            //        if (uidToUserM.ContainsKey(bot.state.uid))
            //        {
            //            StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, new CloseMessage()));
            //            bot.reset();
            //        }
            //    }
            //    else
            //    {
            //        if (bot.aiList == null)
            //        {
            //            // Bot is just starting up. So set its name
            //            StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, new NameSetMessage(bot.state.playerName)));
            //        }
            //        System.Tuple<AIPriorityList, AIMemory> result = bot.ai(bot.aiList, bot.state);
            //        bot.state.extraState = result.Item2;
            //        UserInput uinp = Bots.getBotAction(result.Item1, bot.state);
            //        if (result.Item1 == null)
            //        {
            //            Debug.Log("null retai");
            //        }
            //        bot.aiList = result.Item1;

            //        if (Constants.inspectorDebugging)
            //        {
            //            inspectorDebugger.addPair(new StringPair(bot.state.uid, bot.ToString()));
            //        }
            //        if (uinp != null)
            //        {
            //            StoreMessages.newMsgs.Add(new GotMessage(bot.state.uid, uinp));
            //        }

            //        // clear msgs because guaranteed to get a fresh state because this runs at the same time as a "server tick"
            //        // msgs is the NEW MESSAGES this tick.
            //        bot.state.msgs.Clear();
            //    }
            //}

            // Add Items
            // First count each item type
            //Dictionary<System.Type, int> currentItems = new Dictionary<System.Type, int>();
            //foreach (var itemT in Constants.worldItemTypes)
            //{
            //    currentItems[itemT] = 0;
            //}
            //foreach (var item in objToItems.Values)
            //{
            //    System.Type t = item.itemInfo.GetType();
            //    if (currentItems.ContainsKey(t))
            //    {
            //        currentItems[t] += 1;
            //    }
            //}
            //// get items equiped by players
            //foreach (var um in uidToUserM.Values)
            //{
            //    var po = um.playerObject;
            //    if (po != null)
            //    {
            //        var witype = weaponTypeToItemType(po.privateInfo.slot1);
            //        if (witype != null && currentItems.ContainsKey(witype))
            //        {
            //            currentItems[witype] += 1;
            //        }
            //        witype = weaponTypeToItemType(po.privateInfo.slot2);
            //        if (witype != null && currentItems.ContainsKey(witype))
            //        {
            //            currentItems[witype] += 1;
            //        }
            //    }
            //}

            // Now for each type, get the number of more to spawn and spawn them
            //int numItems = (int)(Mathf.Max(Constants.maxBots) * Constants.itemToPlayerRatio);

            //foreach (var itemType in currentItems.Keys)
            //{
            //    int toSpawn = numItems - currentItems[itemType];
            //    for (int i = 0; i < toSpawn; i++)
            //    {
            //        // TODO: Maybe put 3 health items on top of each other?
            //        spawnItem(itemType, 1);
            //    }
            //}

            // Use while loop and remove 1 at a time so that its more thread safe.
            // If you clear whole list, maybe a message was added right before you cleared.
            var mm = StoreMessages.popMsg();
            while (mm != null)
            {
                transferNewMessage(mm);
                mm = StoreMessages.popMsg();
            }

            // Call update on all UserManagers.
            UserManager[] copyUM = new UserManager[uidToUserM.Values.Count];
            uidToUserM.Values.CopyTo(copyUM, 0);
            // Since could call deleteSelf, make a copy of the list to iterate through so don't modify list as you loop
            foreach (var um in copyUM)
            {
                um.customUpdate();
                um.clearMessages();
                var cm = um.countMessages();
                //Debug.Log("LeftOver Messages: " + cm);
                if (cm > 0)
                {
                    Debug.Break();
                }
            }

            // Send out worldItem messages. MAKE SURE AFTER CUSTOM UPDATE OF USER SO PICKUPS ARE DONE FIRST
            foreach (var item in objToItems.Values)
            {
                broadcastMessageQueue.Add(item);
            }

            // Send all messages out at once in a big list
            foreach (var msgs in uidToMessageQueue)
            {

                msgs.Value.Clear();
            }
            broadcastMessageQueue.Clear();
        }
    }

    // Update is called once per frame
    void UpdateServerStyleFull()
    {
        if (true)
        {
            // TODO: If bots are inactive, check if UserManager exists, if it does, send the OnClose message to it
            // TODO: Bots generate artificial messages here. Probably just UserInput msgs.
            // Add the msgs to StoreMessages.newMsgs
            foreach (Bot bot in uidToBot.Values)
            {
                if (!Bots.botAlive(bot.state, 0, Constants.maxBots))
                {
                    if (uidToUserM.ContainsKey(bot.state.uid))
                    {
                        StoreMessages.addMsg(new GotMessage(bot.state.uid, new CloseMessage()));
                        bot.reset();
                    }
                }
                else
                {
                    if (bot.aiList == null)
                    {
                        // Bot is just starting up. So set its name
                        StoreMessages.addMsg(new GotMessage(bot.state.uid, new NameSetMessage(bot.state.playerName)));
                    }
                    System.Tuple<AIPriorityList, AIMemory> result = bot.ai(bot.aiList, bot.state);
                    bot.state.extraState = result.Item2;
                    UserInput uinp = Bots.getBotAction(result.Item1, bot.state);
                    if (result.Item1 == null)
                    {
                        Debug.Log("null retai");
                    }
                    bot.aiList = result.Item1;

                    if (Constants.inspectorDebugging)
                    {
                        inspectorDebugger.addPair(new StringPair(bot.state.uid, bot.ToString()));
                    }
                    if (uinp != null)
                    {
                        StoreMessages.addMsg(new GotMessage(bot.state.uid, uinp));
                    }

                    // clear msgs because guaranteed to get a fresh state because this runs at the same time as a "server tick"
                    // msgs is the NEW MESSAGES this tick.
                    bot.state.msgs.Clear();
                }
            }

            // Add Items
            // First count each item type
            Dictionary<System.Type, int> currentItems = new Dictionary<System.Type, int>();
            foreach (var itemT in Constants.worldItemTypes)
            {
                currentItems[itemT] = 0;
            }
            foreach (var item in objToItems.Values)
            {
                System.Type t = item.itemInfo.GetType();
                if (currentItems.ContainsKey(t))
                {
                    currentItems[t] += 1;
                }
            }
            // get items equiped by players
            foreach (var um in uidToUserM.Values)
            {
                var po = um.playerObject;
                if (po != null)
                {
                    var witype = weaponTypeToItemType(po.privateInfo.slot1);
                    if (witype != null && currentItems.ContainsKey(witype))
                    {
                        currentItems[witype] += 1;
                    }
                    witype = weaponTypeToItemType(po.privateInfo.slot2);
                    if (witype != null && currentItems.ContainsKey(witype))
                    {
                        currentItems[witype] += 1;
                    }
                }
            }

            // Now for each type, get the number of more to spawn and spawn them
            int numItems = (int)(Mathf.Max(Constants.maxBots) * Constants.itemToPlayerRatio);

            foreach (var itemType in currentItems.Keys)
            {
                int toSpawn = numItems - currentItems[itemType];
                for (int i = 0; i < toSpawn; i++)
                {
                    // TODO: Maybe put 3 health items on top of each other?
                    spawnItem(itemType, 1);
                }
            }

            // Use while loop and remove 1 at a time so that its more thread safe.
            // If you clear whole list, maybe a message was added right before you cleared.
            var mm = StoreMessages.popMsg();
            while (mm != null)
            {
                transferNewMessage(mm);
                mm = StoreMessages.popMsg();
            }

            // Call update on all UserManagers.
            UserManager[] copyUM = new UserManager[uidToUserM.Values.Count];
            uidToUserM.Values.CopyTo(copyUM, 0);
            // Since could call deleteSelf, make a copy of the list to iterate through so don't modify list as you loop
            foreach (var um in copyUM)
            {
                um.customUpdate();
            }

            // Send out worldItem messages. MAKE SURE AFTER CUSTOM UPDATE OF USER SO PICKUPS ARE DONE FIRST
            foreach (var item in objToItems.Values)
            {
                broadcastMessageQueue.Add(item);
            }

            // Send all messages out at once in a big list
            foreach (var msgs in uidToMessageQueue)
            {
                
                msgs.Value.Clear();
            }
            broadcastMessageQueue.Clear();
        }
    }

    private void FixedUpdate()
    {
        playerCollisionsThisFrame.Clear();
    }

    public static void tryPickUpItem(GameObject pickup, PlayerObject player)
    {
        string objID = pickup.GetInstanceID() + "";
        if (objToItems.ContainsKey(objID))
        {
            var item = objToItems[objID];
            if (item.quantity > 0)
            {
                item.quantity -= 1;

                // actually process the item:
                if (item.itemInfo.GetType() == typeof(HealthItem))
                {
                    var hi = (HealthItem)item.itemInfo;
                    var hp = player.GetComponent<Health>();
                    hp.changeHealth(hi.healthBonus);
                }
                if (Constants.IsSameOrSubclass(typeof(WeaponItem), item.itemInfo.GetType()))
                {
                    var weapon = (WeaponItem)item.itemInfo;
                    player.pickUpWeapon(weapon.weapon, uidToUserM[player.uid].equipedSlot1);
                }

                if (item.quantity <= 0)
                {
                    broadcastMessageQueue.Add(new DeleteMessage(null, item.objectInfo.objectID));
                    objToItems.Remove(objID);
                    Destroy(pickup);
                }
            }
        }
    }

    public static void removeUserManager(string uid)
    {
        uidToUserM.Remove(uid);
        uidToMessageQueue.Remove(uid);
    }

    void transferNewMessage(GotMessage gm)
    {
        // handle pings simply by replying
        if (gm.m != null && gm.m.GetType() == typeof(PingMessage))
        {
            sendToSpecificUser(gm.uid, gm.m);
        }
        else
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
                um = ServerTest.getUserManager(gm.uid);
            }
            var m = gm.m;
            um.addMessage(m);
        }
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
            if (m.GetType() == typeof(CopyMovement)) //m.msgType == 1 && 
            {
                CopyMovement cp = (CopyMovement)m;
                if (cp.objectInfo.uid == uid)
                {
                    uidToBot[uid].state.addCharacterState(new CharacterState(cp));
                }
            }
        }
    }

    public static UserManager getUserManager(string uid)
    {
        if (uidToUserM.ContainsKey(uid))
        {
            return uidToUserM[uid];
        }
        else
        {
            return null;
        }
    }

    public static Vector3 getSpawnLocation()
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

    static void spawnItem(System.Type t, int quantityInOneSpot, Vector3 spawnLocation)
    {

        var w1 = Instantiate<GameObject>(Constants.prefabsFromType[t]);
        w1.transform.position = spawnLocation;
        w1.transform.Rotate(new Vector3(0, 1, 0), Random.Range(0, 360));
        WorldItem wi1 = new WorldItem(new NetworkObjectInfo(w1.GetInstanceID() + "", NetworkObjectType.worldItem, ""), (ItemInfo)System.Activator.CreateInstance(t), spawnLocation, w1.transform.localRotation, 1);

        objToItems.Add(wi1.objectInfo.objectID, wi1);
    }

    static void spawnItem(System.Type t, int quantityInOneSpot)
    {
        spawnItem(t, quantityInOneSpot, getSpawnLocation());
    }

    public static System.Type weaponTypeToItemType(WeaponType wt)
    {
        switch (wt)
        {
            case WeaponType.none:
                return null;
            case WeaponType.sword:
                return null;
            case WeaponType.spear:
                return typeof(SpearItem);
            case WeaponType.greatsword:
                return typeof(GreatSwordItem);
            default:
                Debug.LogError("Got weapontype unknown! " + wt);
                return null;
        }
    }
    public static void dropWeaponAt(WeaponType wt, Vector3 spawnLocation)
    {
        spawnLocation.y = 0;
        System.Type wtype = weaponTypeToItemType(wt);
        if (wtype != null)
        {
            spawnItem(wtype, 1, spawnLocation);
        }
    }

    

}
