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

    void Awake()
    {
        playerCharacterPrefab = Resources.Load<GameObject>("controlledPlayer");
    }
}

