using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtCamera : MonoBehaviour
{
    void Update()
    {
        if (!Application.isBatchMode)
        {
            Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.position, Vector3.up);
            transform.rotation = rotation;
        }
    }
}
