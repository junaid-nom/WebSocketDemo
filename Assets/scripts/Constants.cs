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

public class Constants : MonoBehaviour
{
    public const float charMoveSpeed = 20;
    public const float startHP = 100;

    public const int inputLifetimeMS = 400;

    public const string canMoveState = "defAnim";
    public const string getHitState = "getHit";
    public static readonly string[] charUserControlledStateNames = { "anim1", "anim2", "anim3_flip", "dodge" }; // TODO Change to enum... and also use that enum to index CopyMovement ButtonsDown
    public static readonly string[] dodgeFromStates = { getHitState, canMoveState };

    public const float timeNeededToCounterAttack = .1f;

    public static GameObject playerCharacterPrefab;

    public const int secondsBeforeDestroyNetworkObject = 10;
    public const float playerWidth = .5f;

    public static AnimationInfo attackAnimationInfo;
    public AnimationInfo _attackAnimationInfo; // set in inspector

    public static readonly WeaponInfo swordInfo = new WeaponInfo(7, 7, 25, 40);

    public const int maxBots = 10;

    public const float spawnXRange = 20;
    public const float spawnZRange = 11;

    void Awake()
    {
        playerCharacterPrefab = Resources.Load<GameObject>("controlledPlayer");
        blockMovementMask = LayerMask.GetMask(new string[] { "wall", "player" });
        attackAnimationInfo = _attackAnimationInfo;
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
}

