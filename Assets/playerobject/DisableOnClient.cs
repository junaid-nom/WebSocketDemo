using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnClient : MonoBehaviour
{
    List<Collider> toDisableColliders;
    List<Rigidbody> toDisableRigid;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!Server.isOn)
        {
            toDisableColliders = new List<Collider>(GetComponentsInChildren<Collider>());
            toDisableRigid = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());

            toDisableColliders.ForEach(c => c.enabled = false);
            toDisableRigid.ForEach(c => c.isKinematic = true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
