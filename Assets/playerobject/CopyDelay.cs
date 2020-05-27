using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyDelay : MonoBehaviour
{
    public GameObject source;
    Animator sourceAnimator;
    public copyFromStruct destination;
    public bool ignoreRotation;

    float pingSecs = .05f;
    float tickTime = .0333f; // 30 fps = .0333...
    float pingNoiseMin = .01f;
    float pingNoiseMax = .03f;
    float nextTickTime;

    // Start is called before the first frame update
    void Start()
    {
        sourceAnimator = source.GetComponent<Animator>();
        setNextTick();
    }

    void setNextTick()
    {
        nextTickTime = Time.time + tickTime;
    }

    

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextTickTime)
        {
            setNextTick();
            CopyMovement cp = new CopyMovement();
            cp.ignoreRotation = ignoreRotation;
            cp.localPosition = source.transform.localPosition;
            cp.localRotation = source.transform.localRotation;
            if (sourceAnimator.GetCurrentAnimatorClipInfo(0).Length > 0)
            {
                cp.anim_state = sourceAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name; // only works if anim clip name is same as state name
            }            cp.normalizedTime = sourceAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            StartCoroutine(copyFromStruct.SendAfterTime(pingSecs + Random.Range(pingNoiseMin, pingNoiseMax), cp, destination));
        }
    }
}
