using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelperPlayer : MonoBehaviour
{
    Health health;
    PlayerObject player;
    // Start is called before the first frame update
    void Start()
    {
        health = GetComponentInChildren<Health>();
        player = GetComponent<PlayerObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDamage(float damage)
    {
        var weapon = player.getActiveWeapon();
        if (weapon != null)
        {
            Debug.Log("active weapon:" + weapon.name);
            weapon.setDamage(damage);
        }
    }

    public void setDamageTakenMultiplier(float multi)
    {
        health.setDamageTakenMultiplier(multi);
    }

    public void pickUpItem()
    {
        if (Server.isOn)
        {
            Debug.Log("collided: " + Server.playerCollisionsThisFrame.Keys.Count);
            // TODO: Get closest item to self.
            if (Server.playerCollisionsThisFrame.ContainsKey(player.uid))
            {
                var colls = Server.playerCollisionsThisFrame[player.uid];
                if (colls.Count > 0)
                {
                    var coll = colls[Constants.findBest<PlayerCollision>(colls, (coll1, coll2) => coll1.distance <= coll2.distance ? coll1 : coll2)];

                    var item = coll.other.GetComponent<PickUp>();
                    // Try to pick that item up (will fail if quantity = 0 already)
                    Server.tryPickUpItem(item.gameObject, coll.player);
                }
            }
        }
    }
}
