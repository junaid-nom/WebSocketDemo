﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    float startHP = Constants.startHP;
    float health = Constants.startHP;
    public HPBar hpbar;
    public GameObject hpBarParent;
    public float damageTakenMultiplier = 1;

    public ColorChanger invulChange;
    public Color invulColor;

    PlayerObject playerObject;

    public Animator getHitAnimator;
    // Start is called before the first frame update
    void Start()
    {
        //startHP = Constants.startHP;
        //health = startHP;
        playerObject = GetComponent<PlayerObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            hpBarParent.SetActive(false);
            // Solves rare bug where might get stuck and not display death anim sometimes
            if (getHitAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name != Constants.deathState)
            {
                getHitAnimator.StopPlayback();
                getHitAnimator.Play(Constants.deathState, 0, 0);
                // TODO: disable health bar. set colliders to off. show "Press E to revive"
                // disable all colliders
                playerObject.die();
            }
        }
        else
        {
            hpBarParent.SetActive(true);
        }
    }

    // Should only be used by client-side to set health based on what server says
    public void setHealth(float hp)
    {
        health = hp;
        hpbar.setHPScale(health / startHP);
    }

    public float getHealth()
    {
        return health;
    }

    public float changeHealth(float change)
    {
        float changeApply = change * (change < 0 ? damageTakenMultiplier : 1);
        if (changeApply != 0)
        {
            health += changeApply;
            health = Mathf.Min(health, startHP);
            hpbar.setHPScale(health / startHP);
            if (changeApply < 0)
            {
                if (health <= 0)
                {
                    getHitAnimator.StopPlayback();
                    getHitAnimator.Play(Constants.deathState, 0, 0);
                    // TODO: disable health bar. set colliders to off. show "Press E to revive"
                    // disable all colliders
                    playerObject.die();
                }
                else
                {
                    // APPLY GET HIT HERE...
                    getHitAnimator.StopPlayback();
                    getHitAnimator.Play(Constants.getHitState, 0, 0);
                }
            }
        }
        return changeApply;
    }

    public void setDamageTakenMultiplier(float multi)
    {
        damageTakenMultiplier = multi;
        if (damageTakenMultiplier == 0)
        {
            invulChange.setColor(invulColor);
        } else
        {
            invulChange.resetColor();
        }
    }
}
