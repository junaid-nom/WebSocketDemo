using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 0;
    Collider coll;
    List<Health> hitAlready = new List<Health>();
    PlayerObject myp;
    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider>();
        myp = Constants.getComponentInParentOrChildren<PlayerObject>(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Server.isOn)
        {
            if (damage == 0)
            {
                coll.enabled = false;
            }
            else
            {
                coll.enabled = true;
            }
        } else
        {
            coll.enabled = false;
        }
    }

    // Animation event for setting damage
    public void setDamage(float damage)
    {
        this.damage = damage;
        hitAlready.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("" + myp.gameObject.name + " Hit:" + other.gameObject.name);
        Health hOther = Constants.getComponentInParentOrChildren<Health>(other.gameObject);
        PlayerObject otherP = Constants.getComponentInParentOrChildren<PlayerObject>(other.gameObject);
        if ((otherP== null || otherP.uid != myp.uid) && hOther != null && !hitAlready.Contains(hOther))
        {
            float damageDone = Mathf.Abs(hOther.changeHealth(-1 * damage));
            if (damageDone != 0)
            {
                // life steal
                float lifeGained = Constants.scoreToLifesteal(myp.score) * damageDone;
                myp.health.changeHealth(lifeGained);

                // Update score
                int scoreEffective = (otherP.score + Constants.baseScore);
                myp.score += (int)(((damageDone / Constants.startHP) * scoreEffective) * Constants.damageScoreFactor);
                if (hOther.getHealth() <= 0)
                {
                    myp.score += (int)(scoreEffective * Constants.damageScoreFactor);
                }
            }
            hitAlready.Add(hOther);
        }
    }
}
