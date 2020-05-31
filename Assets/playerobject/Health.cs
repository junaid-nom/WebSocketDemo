using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float startHP;
    float health;
    public HPBar hpbar;
    public float damageTakenMultiplier = 1;

    public ColorChanger invulChange;
    public Color invulColor;
    // Start is called before the first frame update
    void Start()
    {
        health = startHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeHealth(float change)
    {
        health += change * damageTakenMultiplier;
        hpbar.setHPScale(health / startHP);
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
