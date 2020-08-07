using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    none, sword, spear, greatsword
}

public class PlayerObject : MonoBehaviour
{
    // must be set when created:
    public string uid;
    public int score = Constants.startScore;
    public bool isClientObject = false;
    public string playerName;

    // auto set
    Animator animator;
    public Health health;
    public bool dead = false;

    // set in prefab:
    public GameObject hitbox;
    public GameObject sword;
    public GameObject spear;
    public GameObject greatsword;

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
        weaponObjects = new List<GameObject>() { sword, spear, greatsword };
        weapons = new List<Weapon>() { sword.GetComponentInChildren<Weapon>(), spear.GetComponentInChildren<Weapon>(), greatsword.GetComponentInChildren<Weapon>() };
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        score = Constants.startScore;
    }

    GameObject weaponTypeToWeapon(WeaponType w)
    {
        switch (w)
        {
            case WeaponType.sword:
                return sword;
            case WeaponType.spear:
                return spear;
            case WeaponType.greatsword:
                return greatsword;
            default:
                throw new KeyNotFoundException();
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
        if (Server.isOn)
        {
            if (privateInfo.slot2 == WeaponType.none)
            {
                privateInfo.slot2 = w;
            }
            else
            {
                if (equipedSlot1)
                {
                    Server.dropWeaponAt(privateInfo.slot1, transform.localPosition);
                    privateInfo.slot1 = w;
                }
                else
                {
                    Server.dropWeaponAt(privateInfo.slot2, transform.localPosition);
                    privateInfo.slot2 = w;
                }
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
        if (Server.isOn && !dead)
        {
            score = Constants.startScore;
            dead = true;
            animator.SetBool("dead", true);
            List<GameObject> toDisable = new List<GameObject>(weaponObjects);
            toDisable.Add(hitbox);
            toDisable.Add(gameObject); // For ribid body
            foreach (var gw in toDisable)
            {
                gw.GetComponentInChildren<Collider>().enabled = false;
            }
            // TODO: Not sure if they go back on when respawn?
            foreach (var weap in weaponObjects)
            {
                weap.SetActive(false);
            }

            // drop picked up items
            // function will handle case of none or sword
            Server.dropWeaponAt(privateInfo.slot1, transform.localPosition);
            Server.dropWeaponAt(privateInfo.slot2, transform.localPosition);
        }
    }

    public void respawn()
    {
        if (Server.isOn)
        {
            animator.SetBool("dead", false);
            animator.Play(Constants.canMoveState, 0, 0);
            List<GameObject> toDisable = new List<GameObject>(weaponObjects);
            toDisable.Add(hitbox);
            toDisable.Add(gameObject); // For ribid body
            foreach (var gw in toDisable)
            {
                gw.GetComponentInChildren<Collider>().enabled = true;
            }
            gameObject.transform.position = Server.getSpawnLocation();

            privateInfo = new PrivatePlayerInfo(WeaponType.sword, WeaponType.none);
            health.setHealth(Constants.startHP);
            dead = false;
        }
    }
}
