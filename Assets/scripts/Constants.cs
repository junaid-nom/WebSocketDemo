using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponInfo
{
    public float avgRange;
    public float specialRange;
    public float avgDamage;
    public float specialDamage;

    public WeaponInfo(float avgRange, float specialRange, float avgDamage, float specialDamage)
    {
        this.avgRange = avgRange;
        this.specialRange = specialRange;
        this.avgDamage = avgDamage;
        this.specialDamage = specialDamage;
    }
}

public delegate T best<T>(T a, T b); 


public class Constants : MonoBehaviour
{
    public const float charMoveSpeed = 10;
    public const float startHP = 100;

    public const int inputLifetimeMS = 400;

    public const string canMoveState = "defAnim";
    public const string getHitState = "getHit";
    public const string pickUpState = "pickup";
    public static readonly string[] charUserControlledStateNames = { "attack1", "attack2", "attack3", "dodge", pickUpState }; // TODO Change to enum... and also use that enum to index CopyMovement ButtonsDown
    public static readonly string[] dodgeFromStates = { getHitState, canMoveState, pickUpState };

    public static RuntimeAnimatorController spear_animator;
    public RuntimeAnimatorController _spear_animator;
    public static RuntimeAnimatorController sword_animator;
    public RuntimeAnimatorController _sword_animator;

    public static readonly Dictionary<WeaponType, RuntimeAnimatorController> weaponToAnimator = new Dictionary<WeaponType, RuntimeAnimatorController>();

    public const float timeNeededToCounterAttack = .1f;

    public static GameObject playerCharacterPrefab;
    public static Dictionary<System.Type, GameObject> prefabsFromType = new Dictionary<System.Type, GameObject>(); 

    public const int secondsBeforeDestroyNetworkObject = 10;
    public const float playerWidth = .5f;

    public static AnimationInfo attackAnimationInfo;
    public AnimationInfo _attackAnimationInfo; // set in inspector

    public static readonly WeaponInfo swordInfo = new WeaponInfo(7, 7, 25, 40);

    public const int maxBots = 3;

    public const float spawnXRange = 20;
    public const float spawnZRange = 11;

    void Awake()
    {
        playerCharacterPrefab = Resources.Load<GameObject>("controlledPlayer");

        prefabsFromType.Add(typeof(HealthItem), Resources.Load<GameObject>("HealthPickUp"));
        prefabsFromType.Add(typeof(SpearItem), Resources.Load<GameObject>("SpearPickUp"));

        blockMovementMask = LayerMask.GetMask(new string[] { "wall", "player" });
        attackAnimationInfo = _attackAnimationInfo;
        spear_animator = _spear_animator;
        sword_animator = _sword_animator;

        weaponToAnimator.Add(WeaponType.sword, sword_animator);
        weaponToAnimator.Add(WeaponType.spear, spear_animator);
    }

    public static int blockMovementMask;

    public static T getComponentInParentOrChildren<T>(GameObject g) where T: Component
    {
        T r = null;
        r = g.GetComponentInChildren<T>();
        if (r == null)
        {
            r = g.GetComponentInParent<T>();
        }
        return r;
    }

    public static System.TimeSpan timeDiff(System.DateTime timenow, System.DateTime timepast)
    {
        return timenow.Subtract(timepast);
    }

    public static int findBest<T>(List<T> li, best<T> f)
    {
        if (li.Count > 0)
        {
            T best = li[0];
            int ret = 0;
            for (int i = 0; i < li.Count; i++)
            {
                T t = li[i];
                best = f(best, t);
                ret = i;
            }
            return ret;
        }
        else
        {
            return -1;
        }

    }

    public static bool IsSameOrSubclass(System.Type potentialBase, System.Type potentialDescendant)
    {
        return potentialDescendant.IsSubclassOf(potentialBase)
               || potentialDescendant == potentialBase;
    }
}

