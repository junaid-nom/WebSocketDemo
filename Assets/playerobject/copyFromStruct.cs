using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public struct CopyMovementStruct
//{
//    public Vector3 localPosition;
//    public Quaternion localRotation;
//    public string anim_state;
//    public float normalizedTime;
//    public bool ignoreRotation;
//}

public class copyFromStruct : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setMovement(CopyMovement mv)
    {
        transform.localPosition = mv.localPosition;
        if (!mv.ignoreRotation)
        {
            transform.localRotation = mv.localRotation;
        }
        if (mv.anim_state != null)
        {
            animator.StopPlayback();
            animator.Play(mv.anim_state, 0, mv.normalizedTime);
        }
    }

    public static IEnumerator SendAfterTime(float time, CopyMovement cp, copyFromStruct obj)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        obj.setMovement(cp);
    }
}
