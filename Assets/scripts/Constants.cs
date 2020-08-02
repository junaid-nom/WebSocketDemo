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
    public const int port = 7270;
    public const string remoteServer = "db.mindgamemagicka.com"; // 3.209.19.120 or db.mindgamemagicka.com
    public const string localServer = "localhost";

    public const int maxNameLength = 15;
    public const int maxScoreBoardRank = 2;

    public const float charMoveSpeed = 10;
    public const float startHP = 100;
    public const int baseScore = 100;
    public const int startScore = 0;
    public const float damageScoreFactor = .2f;
    public const float scoreToLifeSteal = .002f;
    public const float maxLifeSteal = .5f;

    public const int inputLifetimeMS = 400;

    public const string canMoveState = "defAnim";
    public const string getHitState = "getHit";
    public const string pickUpState = "pickup";
    public const string deathState = "die";
    public static readonly string[] charUserControlledStateNames = { "attack1", "attack2", "attack3", "dodge", pickUpState, deathState }; // TODO Change to enum... and also use that enum to index CopyMovement ButtonsDown
    public static readonly string[] dodgeFromStates = { getHitState, canMoveState, pickUpState };

    public static RuntimeAnimatorController spear_animator;
    public RuntimeAnimatorController _spear_animator;
    public static RuntimeAnimatorController sword_animator;
    public RuntimeAnimatorController _sword_animator;
    public static RuntimeAnimatorController greatsword_animator;
    public RuntimeAnimatorController _greatsword_animator;

    public static readonly Dictionary<WeaponType, RuntimeAnimatorController> weaponToAnimator = new Dictionary<WeaponType, RuntimeAnimatorController>();

    public const float timeNeededToCounterAttack = .1f;

    public static GameObject playerCharacterPrefab;
    public static Dictionary<System.Type, GameObject> prefabsFromType = new Dictionary<System.Type, GameObject>();

    public static List<System.Type> worldItemTypes = new List<System.Type>() { typeof(HealthItem), typeof(SpearItem), typeof(GreatSwordItem) };
    public const int healthItemPickUpAmount = 15;
    public const float itemToPlayerRatio = .5f; // TODO: This is kinda dumb right now, ideally it should count weapons being help by players as well!

    public const int secondsBeforeDestroyNetworkObject = 10;
    public const float playerWidth = .5f; // used for physics to check if wall in the way

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
        prefabsFromType.Add(typeof(GreatSwordItem), Resources.Load<GameObject>("GreatSwordPickUp"));

        blockMovementMask = LayerMask.GetMask(new string[] { "wall", "player" });
        attackAnimationInfo = _attackAnimationInfo;
        spear_animator = _spear_animator;
        sword_animator = _sword_animator;
        greatsword_animator = _greatsword_animator;

        weaponToAnimator.Add(WeaponType.sword, sword_animator);
        weaponToAnimator.Add(WeaponType.spear, spear_animator);
        weaponToAnimator.Add(WeaponType.greatsword, greatsword_animator);
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

    public static float scoreToLifesteal(int score)
    {
        return Mathf.Min(Constants.scoreToLifeSteal * score, Constants.maxLifeSteal);
    }
}

