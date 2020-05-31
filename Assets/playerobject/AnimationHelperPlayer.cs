using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelperPlayer : MonoBehaviour
{
    Weapon weapon;
    Health health;
    // Start is called before the first frame update
    void Start()
    {
        weapon = GetComponentInChildren<Weapon>();
        health = GetComponentInChildren<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDamage(float damage)
    {
        weapon.setDamage(damage);
    }

    public void setDamageTakenMultiplier(float multi)
    {
        health.setDamageTakenMultiplier(multi);
    }
}
