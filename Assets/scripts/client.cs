﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
//using WebSocketSharp;
using HybridWebSocket; // Have to use this because C# websocket libraries dont work with WEBGL.
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class BinarySerializer
{
    public static readonly BinaryFormatter Formatter = new BinaryFormatter();

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

public class NetworkObjectClient
{
    public GameObject gameObject;
    public NetworkObjectInfo objectInfo;
    public System.DateTime timeSinceHeartbeat; // delete after secondsBeforeDestroyNetworkObject if you don't see any messages about the gobj anymore

    public NetworkObjectClient(GameObject gameObject, NetworkObjectInfo objectInfo, DateTime timeSinceHeartbeat)
    {
        this.gameObject = gameObject;
        this.objectInfo = objectInfo;
        this.timeSinceHeartbeat = timeSinceHeartbeat;
        if (Server.isOn)
        {
            gameObject.SetActive(false);
        }
    }
}

public class Client : MonoBehaviour
{
    // set via alert:
    public bool autoStartClient;
    public Text displayAlert;
    static Text _displayAlert;
    static bool shouldDisplayAlert = false;
    public static bool dead = false;
    static float distanceToAlert = -1;

    public static Dictionary<string, NetworkObjectClient> objIDToObject = new Dictionary<string, NetworkObjectClient>();
    public static MessageManager clientMsgMan = new MessageManager();

    public static string myUID = "";
    public static Dictionary<string, List<string>> myobjsByType = new Dictionary<string, List<string>>();

    public static WebSocket ws;
    public static bool canPickup = false;

    public static PrivatePlayerInfo privateInfo = new PrivatePlayerInfo(WeaponType.none, WeaponType.none);

    public Text slot1;
    public Text slot2;
    public GameObject pickslot1;
    public GameObject pickslot2;

    public static bool equipedSlot1 = true;

    // Start is called before the first frame update
    void Start()
    {
        if (autoStartClient)
        {
            startClient();
        }
        _displayAlert = displayAlert;
    }

    // Update is called once per frame
    void Update()
    {
        StringMessage sm = clientMsgMan.popMessage<StringMessage>();
        while (sm != null)
        {
            if (sm.str.Contains("userid:"))
            {
                myUID = sm.str.Replace("userid:", "");
                Debug.Log("Got myuid:" + myUID);
            }
            else
            {
                NetDebug.printBoth("Client got str message: " + sm.str);
            }
            sm = clientMsgMan.popMessage<StringMessage>();
        }

        PrivatePlayerInfo pi = clientMsgMan.popMessage<PrivatePlayerInfo>();
        while (pi != null)
        {
            privateInfo = pi;
            setPrivateUI(pi);
            pi = clientMsgMan.popMessage<PrivatePlayerInfo>();
        }

        CopyMovement cp = clientMsgMan.popMessage<CopyMovement>();
        while (cp != null)
        {
            processCopyMovement(cp);
            cp = clientMsgMan.popMessage<CopyMovement>();
        }

        // TODO: process world items here
        WorldItem wi = clientMsgMan.popMessage<WorldItem>();
        while (wi != null)
        {
            processWorldItem(wi);
            wi = clientMsgMan.popMessage<WorldItem>();
        }


        // : Check all the values of dict, if their timeSinceHeartBeat is big, delete the game object and remove from the dict
        List<string> toDelete = new List<string>();
        foreach(var n in objIDToObject.Values)
        {
            if (System.DateTime.Now.Subtract(n.timeSinceHeartbeat).TotalSeconds > Constants.secondsBeforeDestroyNetworkObject)
            {
                toDelete.Add(n.objectInfo.objectID);
            }
        }
        DeleteMessage dm = clientMsgMan.popMessage<DeleteMessage>();
        while (dm != null)
        {
            if (dm.objId != null)
            {
                if (objIDToObject.ContainsKey(dm.objId))
                {
                    toDelete.Add(objIDToObject[dm.objId].objectInfo.objectID);
                }
            }
            else
            {
                foreach (var o in objIDToObject.Values)
                {
                    if (o.objectInfo.uid == dm.uid)
                    {
                        toDelete.Add(o.objectInfo.objectID);
                    }
                }
            }
            
            dm = clientMsgMan.popMessage<DeleteMessage>();
        }

        toDelete.ForEach(deleteNetObject);
        toDelete.Clear();
    }

    public void setPrivateUI(PrivatePlayerInfo pi)
    {
        slot1.text = pi.slot1.ToString();
        slot2.text = pi.slot2.ToString();
        pickslot1.SetActive(equipedSlot1);
        pickslot2.SetActive(!equipedSlot1);
    }

    public static void swapWeapon()
    {
        if (privateInfo.slot2 != WeaponType.none)
        {
            equipedSlot1 = !equipedSlot1;
        }
    }

    public static void displayAlertThisFrame(string toDisplay, float distance)
    {
        if (_displayAlert != null && (distanceToAlert < 0 || distance < distanceToAlert))
        {
            _displayAlert.text = toDisplay;
            shouldDisplayAlert = true;
            canPickup = true;
            distanceToAlert = distance;
        }
    }

    private void FixedUpdate()
    {
        if (!shouldDisplayAlert && !dead)
        {
            displayAlert.enabled = false;
        }
        else
        {
            displayAlert.enabled = true;
            if (dead)
            {
                _displayAlert.text = "Press E to respawn";
            }
        }
        shouldDisplayAlert = false;
        canPickup = false;
        distanceToAlert = -1;
    }

    void deleteNetObject(string objID)
    {
        Destroy(objIDToObject[objID].gameObject);
        objIDToObject.Remove(objID);
    }

    public void processCopyMovement(CopyMovement cp)
    {
        string k = cp.objectInfo.objectID;
        if (cp.objectInfo.uid == myUID)
        {
            if (cp.anim_state == Constants.canMoveState)
                cp.ignoreRotation = true;
            if (cp.anim_state == Constants.deathState)
            {
                dead = true;
                equipedSlot1 = true;
            }
            else
            {
                dead = false;
            }
            myobjsByType.AddOrCreate<string, List<string>, string>(Enum.GetName(typeof(NetworkObjectType), NetworkObjectType.playerCharacter), k);
        }
        if (objIDToObject.ContainsKey(k))
        {
            objIDToObject[k].gameObject.GetComponent<copyFromStruct>().setMovement(cp); // TODO: make this a list of copyFromStruct instead of game object so its faster
            objIDToObject[k].timeSinceHeartbeat = System.DateTime.Now;
        } else
        {
            // :
            // create new game object of type blah
            GameObject ng = Instantiate(Constants.playerCharacterPrefab);
            ng.GetComponent<PlayerObject>().uid = cp.objectInfo.uid;
            ng.name = "CLIENT" + ng.name;
            
            Debug.Log("Adding obj k:" + k);
            // add dictionary entry
            objIDToObject.Add(k, new NetworkObjectClient(ng, cp.objectInfo, System.DateTime.Now));
        }
    }

    public void processWorldItem(WorldItem wi)
    {
        string k = wi.objectInfo.objectID;
        if (objIDToObject.ContainsKey(k))
        {
            // TODO: Do I need to do more stuff here for items on the ground?
            objIDToObject[k].timeSinceHeartbeat = System.DateTime.Now;
        }
        else
        {
            GameObject ng = Instantiate(Constants.prefabsFromType[wi.itemInfo.GetType()]);
            ng.GetComponent<PickUp>().myObjId = wi.objectInfo.uid;
            ng.name = "CLIENT" + ng.name;

            // add dictionary entry
            objIDToObject.Add(k, new NetworkObjectClient(ng, wi.objectInfo, System.DateTime.Now));
        }
    }

    public void startClient()
    {
        ws = WebSocketFactory.CreateInstance("ws://localhost:7268"); // NOTE FOR SOME INSANO REASON 127.0.0.1 wont work but localhost will with hybridsocket

        //UserInput testInp = new UserInput();
        //testInp.x = 1;
        //testInp.y = -1;
        //List<bool> buts = new List<bool>();
        //buts.Add(true);
        //buts.Add(false);
        //buts.Add(true);
        //testInp.buttonsDown = buts;

        ws.OnMessage += (byte[] msg) =>
        {
            //NetDebug.printBoth("Client Received: " + (msg));
            Message deser = (Message)BinarySerializer.Deserialize(msg);
            clientMsgMan.addMessage(deser);
            //NetDebug.printBoth("Client got msg type: " + deser.msgType);
            MessageManager.debugMsg(deser);
        };
        ws.OnOpen += () =>
        {
            //NetDebug.printBoth("Client sending string msg then userinput");
            //ws.Send(BinarySerializer.Serialize(new StringMessage("ClientOpenTest")));
            //ws.Send(BinarySerializer.Serialize(testInp));
        };
        ws.OnError += (string errMsg) => NetDebug.printBoth("got on error " + errMsg);
        ws.OnClose += (WebSocketCloseCode code) => NetDebug.printBoth("got on close " + code);
        
        NetDebug.printBoth("About to start client");
        ws.Connect();
    }
    

    void closeStuff()
    {
        if (ws != null && (ws.GetState() == WebSocketState.Open || ws.GetState() == WebSocketState.Connecting))
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
