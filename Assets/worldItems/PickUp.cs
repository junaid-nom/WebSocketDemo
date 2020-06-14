using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public string display; // set via inspector
    Collider coll;
    public string myObjId = null; // Need to set this on creation

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerStay(Collider other)
    {
        Debug.Assert(myObjId != null);
        if (Client.ws != null && Client.ws.GetState() == HybridWebSocket.WebSocketState.Open)
        {
            Debug.Log("" + gameObject.name + " Hit:" + other.gameObject.name);
            //Health hOther = Constants.getComponentInParentOrChildren<Health>(other.gameObject);
            PlayerObject p = Constants.getComponentInParentOrChildren<PlayerObject>(other.gameObject);
            
            if (p != null && p.uid == Client.myUID)
            {
                Client.displayAlertThisFrame(display, Vector3.Distance(p.gameObject.transform.position, transform.position));
            }
        }
        
        if (Server.isOn)
        {
            PlayerObject p = Constants.getComponentInParentOrChildren<PlayerObject>(other.gameObject);

            if (p != null)
            {
                Server.playerCollisionsThisFrame.AddOrCreate<string, List<PlayerCollision>, PlayerCollision>(p.uid, new PlayerCollision(p, gameObject, Vector3.Distance(p.gameObject.transform.position, transform.position)));
            }
        }
    }
}
