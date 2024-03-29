﻿using System.Collections;
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
    public Collider playerHitBox;
    public Health health;
    public PlayerObject playerObject;

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
        if (Server.isOn)
        {
            RaycastHit hit;
            Vector3 start = transform.position;
            Vector3 dir = (mv.localPosition - transform.localPosition);
            float dist = (mv.localPosition - transform.localPosition).magnitude + Constants.playerWidth; //1 being size of capsule rougly
            int layermask = Constants.blockMovementMask;
            bool gotHit = Physics.Raycast(start, dir, out hit, dist, layermask);
            
            Debug.DrawRay(start, dir * dist, Color.red, 1);

            if (!gotHit)
            {
                transform.localPosition = mv.localPosition;
            } else
            {
                if (hit.collider != null && hit.collider.gameObject != playerHitBox.gameObject)
                {
                    //Debug.Log(playerHitBox.gameObject.GetInstanceID() + " " + this.gameObject.GetInstanceID() + " Got hit:" + hit.collider.gameObject.name + " " + hit.collider.gameObject.GetInstanceID());
                    if (hit.collider.isTrigger)
                    {
                        transform.localPosition = mv.localPosition;
                    }
                }
                else
                {
                    Debug.Log("wthit:" + layermask);
                }
            }
        } else
        {
            transform.localPosition = mv.localPosition;
        }
        
        if (!mv.ignoreRotation)
        {
            transform.localRotation = mv.localRotation;
        }
        if (mv.anim_state == null) // basically only want to do this when not animating, or after a new animation starts
            playerObject.enableWeapon(mv.weapon);
        if (mv.anim_state != null)
        {
            // Speed up animations based off of half ping so more accurate to what is on the server at the moment to compensate for lag.
            float expectedNormalizedTime = Server.isOn ? mv.normalizedTime : ((Client.lastPingDiff / 2) + mv.normalizedTime - .01f);
            float animCurrentTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if ((animCurrentTime < expectedNormalizedTime || animCurrentTime - expectedNormalizedTime > .1) || !animator.GetCurrentAnimatorStateInfo(0).IsName(mv.anim_state))
            {
                animator.StopPlayback();
                // animator set controller
                playerObject.enableWeapon(mv.weapon);
                animator.runtimeAnimatorController = Constants.weaponToAnimator[mv.weapon];
                animator.Play(mv.anim_state, 0, expectedNormalizedTime);
            }
        } else if(playerObject.isClientObject)
        {
            // only revert to default for client version because otherwise weird shit will happen for gethit animation because it will be OVERWRITTEN by the copystruct
            // This would fuck up any animation set not by copystruct here on server.
            animator.Play(Constants.canMoveState, 0, mv.normalizedTime);
        }
        health.setHealth(mv.health);
        playerObject.score = mv.score;
        playerObject.playerName = mv.playerName;
    }

    public static IEnumerator SendAfterTime(float time, CopyMovement cp, copyFromStruct obj)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
        obj.setMovement(cp);
    }
}
