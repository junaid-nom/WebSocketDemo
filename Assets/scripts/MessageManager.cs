using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NetworkObjectType
{
    playerCharacter, worldItem
}

[Serializable]
public class Message
{
    //public byte msgType = 0;
}

[Serializable]
public class NetworkObjectInfo
{
    public string objectID; //gameobject id on server usually
    public NetworkObjectType objectType;
    public string uid; // user id of the "owner" sometimes blank for npc style objects

    public NetworkObjectInfo(string objectID, NetworkObjectType objectType, string uid)
    {
        this.objectID = objectID;
        this.objectType = objectType;
        this.uid = uid;
    }
}

[Serializable]
public class ItemInfo
{
    
}
[Serializable]
public class HealthItem : ItemInfo
{
    public float healthBonus;
}
[Serializable]
public class WorldItem : Message
{
    public NetworkObjectInfo objectInfo;
    public ItemInfo itemInfo;
    public SerializableVector3 localPosition;
    public int quantity;

    public WorldItem(ItemInfo itemInfo, SerializableVector3 localPosition, int quantity)
    {
        this.itemInfo = itemInfo;
        this.localPosition = localPosition;
        this.quantity = quantity;
    }

    public override string ToString()
    {
        return "World Item: " + itemInfo.ToString() + " x:" + localPosition.x + " z:" + localPosition.z + " #:" + quantity;
    }
}

[Serializable]
public class CopyMovement : Message
{
    public NetworkObjectInfo objectInfo;
    public SerializableVector3 localPosition;
    public SerializableQuaternion localRotation;
    public string anim_state;
    public float normalizedTime;
    public bool ignoreRotation;
    public float health; // TODO THIS IS UNUSED NEED TO ACTUALLY DO STUFF! add Health component to copyFromtStruct. Also need to change inputToMovement function

    public CopyMovement()
    {
        //msgType = 1;
    }

    public CopyMovement(NetworkObjectInfo objectInfo, SerializableVector3 localPosition, SerializableQuaternion localRotation, string anim_state, float normalizedTime, bool ignoreRotation, float health)
    {
        //msgType = 1;
        this.objectInfo = objectInfo;
        this.localPosition = localPosition;
        this.localRotation = localRotation;
        this.anim_state = anim_state;
        this.normalizedTime = normalizedTime;
        this.ignoreRotation = ignoreRotation;
        this.health = health;
    }

    public override string ToString()
    {
        return "loc:" + localPosition.ToString() + " rot: " + localRotation.ToString() + " anim: " + anim_state + " ntime: " + normalizedTime
        + " ignoreRot: " + ignoreRotation + " health: " + health;
        ;
    }
}

[Serializable]
public class UserInput : Message
{
    public float x;
    public float y;
    public List<bool> buttonsDown;
    public SerializableVector3 target;

    public UserInput ()
    {
        //msgType = 2;
    }

    public override string ToString()
    {
        string ret = "" + x + "," + y;
        if (buttonsDown != null)
        {
            for (int i = 0; i < buttonsDown.Count; i++)
            {
                ret += " b" + i + ":" + buttonsDown[i];
            }
        }
        ret += " target: " + target.x + "," + target.y;
        return ret;
    }
}

[Serializable]
public class StringMessage : Message
{
    public string str;
    public StringMessage()
    {
        //msgType = 3;
    }
    public StringMessage(string toSend)
    {
        //msgType = 3;
        str = toSend;
    }
}

// Using these 2 for... silly reasons basically a way to communicate from the websocket to the unity thread
[Serializable]
public class CloseMessage : Message
{
    public CloseMessage()
    {
       // msgType = 4;
    }
}
[Serializable]
public class OpenMessage : Message
{
    public OpenMessage()
    {
        //msgType = 5;
    }
}

// combine messages into one big list to make traffic less crazy
[Serializable]
public class ListMessage : Message
{
    public List<Message> messageArray;
    public ListMessage()
    {
        //msgType = 6;
    }
    public ListMessage(List<Message> messageArray)
    {
        //msgType = 6;
        this.messageArray = messageArray;
    }
}


[Serializable]
public class DeleteMessage : Message
{
    public string uid;
    public string objId;
    public DeleteMessage()
    {
        //msgType = 7;
    }
    public DeleteMessage(string uid, string objId)
    {
        this.uid = uid;
        this.objId = objId;
    }
}

// meant to hold msgs that will be READ not sent
public class MessageManager
{
    private Dictionary<System.Type, List<Message>> msgs = new Dictionary<System.Type, List<Message>>();
    private DateTime lastMessageTime;

    public DateTime LastMessageTime { get => lastMessageTime; }

    public void addMessage(Message msg)
    {
        System.Type msgType = msg.GetType();
        if (msgType == typeof(ListMessage))
        {
            ListMessage lmsg = (ListMessage)msg;
            lmsg.messageArray.ForEach(addSingleMsg);
        }
        else
        {
            addSingleMsg(msg);
        }
        
    }

    void addSingleMsg(Message msg)
    {
        System.Type msgType = msg.GetType();
        if (!msgs.ContainsKey(msgType))
        {
            msgs[msgType] = new List<Message>();
        }
        msgs[msgType].Add(msg);
        lastMessageTime = System.DateTime.Now;
    }

    public T popMessage<T> () where T:Message
    {
        System.Type msgType = typeof(T);
        if (!msgs.ContainsKey(msgType) || msgs[msgType].Count <= 0)
        {
            return default(T);
        }
        else
        {
            T ret = (T)msgs[msgType][0];
            msgs[msgType].RemoveAt(0);
            return ret;
        }
    }

    public List<T> popAllMessages<T>() where T : Message
    {
        System.Type msgType = typeof(T);
        if (!msgs.ContainsKey(msgType) || msgs[msgType].Count <= 0)
        {
            return null;
        }
        else
        {
            List<T> ret = new List<T>();
            // Casting forces manually casting each element
            while (msgs[msgType].Count > 0)
            {
                ret.Add((T)msgs[msgType][0]);
                msgs[msgType].RemoveAt(0);
            }
            return ret;
        }
    }

    public static void debugMsg(Message deser)
    {
        if (deser.GetType() == typeof(StringMessage) ) //deser.msgType == 3
        {
            NetDebug.printBoth("Got stringmsg: " + ((StringMessage)deser).str);
        }
        else if (deser.GetType() == typeof(UserInput)) // deser.msgType == 2
        {
            NetDebug.printBoth("Got UserInput: " + ((UserInput)deser).ToString());
        }
        else if (deser.GetType() == typeof(CopyMovement)) // deser.msgType == 1
        {
              NetDebug.printBoth("Got CopyMovement: " + ((CopyMovement)deser).ToString());
        }
    }
}









/// <summary>
/// Since unity doesn't flag the Vector3 as serializable, we
/// need to create our own version. This one will automatically convert
/// between Vector3 and SerializableVector3
/// </summary>
[System.Serializable]
public struct SerializableVector3
{
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;

    /// <summary>
    /// z component
    /// </summary>
    public float z;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}

/// <summary>
/// Since unity doesn't flag the Quaternion as serializable, we
/// need to create our own version. This one will automatically convert
/// between Quaternion and SerializableQuaternion
/// </summary>
[System.Serializable]
public struct SerializableQuaternion
{
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;

    /// <summary>
    /// z component
    /// </summary>
    public float z;

    /// <summary>
    /// w component
    /// </summary>
    public float w;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    /// <param name="rW"></param>
    public SerializableQuaternion(float rX, float rY, float rZ, float rW)
    {
        x = rX;
        y = rY;
        z = rZ;
        w = rW;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
    }

    /// <summary>
    /// Automatic conversion from SerializableQuaternion to Quaternion
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Quaternion(SerializableQuaternion rValue)
    {
        return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
    }

    /// <summary>
    /// Automatic conversion from Quaternion to SerializableQuaternion
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableQuaternion(Quaternion rValue)
    {
        return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
    }
}