using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessagePrinter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UserInput newmsg = MessageManager.popMessage<UserInput>();
        if (newmsg != null)
        {
            NetDebug.printBoth("DESERIALIZED USER INPUT " + newmsg.ToString());
        }
    }
}
