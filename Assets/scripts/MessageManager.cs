using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Message
{
    public byte msgType = 0;
}

[Serializable]
public class CopyMovement : Message
{
    public SerializableVector3 localPosition;
    public SerializableQuaternion localRotation;
    public string anim_state;
    public float normalizedTime;
    public bool ignoreRotation;

    public CopyMovement()
    {
        msgType = 1;
    }

    public override string ToString()
    {
        return "loc:" + localPosition.ToString() + " rot: " + localRotation.ToString() + " anim: " + anim_state + " ntime: " + normalizedTime
        + " ignoreRot: " + ignoreRotation
        ;
    }
}

[Serializable]
public class UserInput : Message
{
    public float x;
    public float y;
    public List<bool> buttonsDown;

    public UserInput ()
    {
        msgType = 2;
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
        return ret;
    }
}

[Serializable]
public class StringMessage : Message
{
    public string str;
    public StringMessage()
    {
        msgType = 3;
    }
    public StringMessage(string toSend)
    {
        msgType = 3;
        str = toSend;
    }
}

// meant to hold msgs per User
public class MessageManager
{
    private Dictionary<System.Type, List<Message>> msgs = new Dictionary<System.Type, List<Message>>();
    private DateTime lastMessageTime;

    public DateTime LastMessageTime { get => lastMessageTime; }

    public void addMessage(Message msg)
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
        if (deser.msgType == 3)
        {
            NetDebug.printBoth("Got stringmsg: " + ((StringMessage)deser).str);
        }
        else if (deser.msgType == 2)
        {
            NetDebug.printBoth("Got UserInput: " + ((UserInput)deser).ToString());
        }
        else if (deser.msgType == 1)
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