using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    public const float charMoveSpeed = 20;

    public const string canMoveState = "defAnim";
    public static readonly string[] charStateNames = { "anim1", "anim2", "anim3_flip" };

    public static GameObject playerCharacterPrefab;

    public const int secondsBeforeDestroyNetworkObject = 10;
    public const float playerWidth = .5f;

    void Awake()
    {
        playerCharacterPrefab = Resources.Load<GameObject>("controlledPlayer");
        blockMovementMask = LayerMask.GetMask(new string[] { "wall", "player" });
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
}

