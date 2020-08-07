using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnClient : MonoBehaviour
{
    List<Collider> toDisableColliders;
    List<Rigidbody> toDisableRigid;
    public List<Collider> dontDisableColliders;
    public List<Rigidbody> dontDisableRigidBodies;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!Server.isOn)
        {
            var po = GetComponent<PlayerObject>();
            
            toDisableColliders = new List<Collider>(GetComponentsInChildren<Collider>());
            toDisableRigid = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());

            toDisableColliders.ForEach(c => c.enabled = false);
            //toDisableRigid.ForEach(c => c.isKinematic = true);
            if (po.uid == Client.myUID)
            {
                // Need to enable capsule collider just for the sake of picking up items
                // Maybe should instead have server send message? But seems like extra traffic and stuff and lag.
                // TODO: Not sure if this could cause bugs with things hitting each other client side that shouldn't?
                dontDisableColliders.ForEach(c => c.enabled = true);
                //dontDisableRigidBodies.ForEach(c => c.isKinematic = false); // For triggers at least one thing has to have rigid body that is not kinematic
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
