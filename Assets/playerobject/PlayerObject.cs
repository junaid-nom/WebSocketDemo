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

    // set in prefab:
    public GameObject sword;
    public GameObject spear;

    // Server private only:
    public PrivatePlayerInfo privateInfo = new PrivatePlayerInfo(WeaponType.sword, WeaponType.none, true);

    // set automatically:
    List<GameObject> weapons;

    private void Start()
    {
        weapons = new List<GameObject>() { sword, spear };
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
        foreach(var weap in weapons)
        {
            weap.gameObject.SetActive(false);
        }

        weaponTypeToWeapon(w).SetActive(true);
    }

    public void pickUpWeapon(WeaponType w)
    {
        if (privateInfo.slot2 == WeaponType.none)
        {
            privateInfo.slot2 = w;
            privateInfo.equipedSlot1 = false;
        } 
        else
        {
            if (privateInfo.equipedSlot1)
            {
                privateInfo.slot1 = w;
            }
            else
            {
                privateInfo.slot2 = w;
            }
        }
    }

    public void swapWeapon()
    {
        if (privateInfo.slot2 != WeaponType.none)
            privateInfo.equipedSlot1 = !privateInfo.equipedSlot1;
    }
    
    public WeaponType getEquipedWeapon()
    {
        if (privateInfo.equipedSlot1)
        {
            return privateInfo.slot1;
        }
        else
        {
            return privateInfo.slot2;
        }
    }

}
