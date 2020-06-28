using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    none, sword, spear
}

public class PlayerObject : MonoBehaviour
{
    // must be set when created:
    public string uid;

    // auto set
    Animator animator;
    Health health;
    public bool dead = false;

    // set in prefab:
    public GameObject hitbox;
    public GameObject sword;
    public GameObject spear;

    // Server private only:
    public PrivatePlayerInfo privateInfo = new PrivatePlayerInfo(WeaponType.sword, WeaponType.none);

    // set automatically:
    List<Weapon> weapons;
    List<GameObject> weaponObjects;

    private void Start()
    {
        if (weapons == null)
        {
            startup();
        }
    }

    private void startup()
    {
        weaponObjects = new List<GameObject>() { sword, spear };
        weapons = new List<Weapon>() { sword.GetComponentInChildren<Weapon>(), spear.GetComponentInChildren<Weapon>() };
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    GameObject weaponTypeToWeapon(WeaponType w)
    {
        switch (w)
        {
            case WeaponType.sword:
                return sword;
                break;
            case WeaponType.spear:
                return spear;
                break;
            default:
                throw new KeyNotFoundException();
                return null; // should never happen
        }
    }

    public void enableWeapon(WeaponType w)
    {
        if (weapons == null)
        {
            startup();
        }
        foreach (var weap in weaponObjects)
        {
            weap.SetActive(false);
        }

        weaponTypeToWeapon(w).SetActive(true);
    }

    public void pickUpWeapon(WeaponType w, bool equipedSlot1)
    {
        Debug.Log("Got equiped:" + equipedSlot1);
        if (privateInfo.slot2 == WeaponType.none)
        {
            privateInfo.slot2 = w;
        } 
        else
        {
            if (equipedSlot1)
            {
                privateInfo.slot1 = w;
            }
            else
            {
                privateInfo.slot2 = w;
            }
        }
    }

    public Weapon getActiveWeapon()
    {
        foreach (var weap in weapons)
        {
            if (weap.gameObject.activeInHierarchy)
            {
                return weap;
            }
        }
        return null;
    }
    
    public WeaponType getEquipedWeapon(bool equipedSlot1)
    {
        if (equipedSlot1)
        {
            return privateInfo.slot1;
        }
        else
        {
            return privateInfo.slot2;
        }
    }

    public void die()
    {
        dead = true;
        animator.SetBool("dead", true);
        List<GameObject> toDisable = new List<GameObject>(weaponObjects);
        toDisable.Add(hitbox);
        toDisable.Add(gameObject); // For ribid body
        foreach (var gw in toDisable)
        {
            gw.GetComponentInChildren<Collider>().enabled = false;
            var rb = gw.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    public void respawn()
    {
        animator.SetBool("dead", false);
        animator.Play(Constants.canMoveState, 0, 0);
        List<GameObject> toDisable = new List<GameObject>(weaponObjects);
        toDisable.Add(hitbox);
        toDisable.Add(gameObject); // For ribid body
        foreach (var gw in toDisable)
        {
            gw.GetComponentInChildren<Collider>().enabled = true;
            var rb = gw.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
        gameObject.transform.position = Server.getSpawnLocation();

        privateInfo = new PrivatePlayerInfo(WeaponType.sword, WeaponType.none);
        health.setHealth(Constants.startHP);
        dead = false;
        
    }
}
