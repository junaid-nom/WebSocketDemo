using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using BehaviorList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>;
using AIPriorityList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>> >>;

public delegate UserInput BotBehavior(BotState bot);
// Includes conditions that are determined by AIMemory like chasingOpponent
// Note also includes "Composite Conditions" which are a combination of OR/AND of other conditions
public delegate bool Condition(BotState bot);
public delegate bool BehaviorListAttribute(BotBehavior behavior); // PROBABLY not going to be used much because the programmer will already know ATTACK behavior attacks. Only needed for some insane advanced AI that wants to "create itself" and like "modify all my attacking behaviors to be priotized, or even change the attack it uses"
public delegate Tuple<AIPriorityList, AIMemory> BotRun(AIPriorityList ai, BotState bot);


// Bot state that an AI may use to store stuff like what was my previous HP, oh my current is less, must mean I got his this frame
// Should not be modified by BotBehaviors, only by the higher level BotAIs
public class AIMemory
{
    public float lastRecordedHP = 0;
    public string targetUID = null;
    public bool chasingTarget = false;
    // ...
}

public class BotState
{
    public List<Message> msgs = new List<Message>();
    public string uid;
    // Somethinbg for my history of anims
    // something for 
    public const string BOTUIDPREFIX = "SERVERBOT:";
    public List<CharacterState> charState = new List<CharacterState>(); // make sure new stuff is added at 0
    public AIMemory extraState = new AIMemory(); // is null when first ran

    public BotState(string uid)
    {
        charState.Add(new CharacterState(new CopyMovement(null, new Vector3(0,0,0), new Quaternion(), Constants.canMoveState, 0, false, 0))); // add one so dont have to do length > 0 all the time
    }
}
public class CharacterState
{
    public CopyMovement myState;

    public CharacterState(CopyMovement state)
    {
        myState = state;
    }
}

public static class Bots
{
    public static bool botAlive(BotState b, int activeConnections)
    {

        // If get bot number-> botnumber active connections, return true if not many connections and bot number low.
        return false;
    }

    //public delegate BotBehavior BotPersonality(BotPlayer b); Dont need because List<Tuple<Dictionary<BehaviorAttribute, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> is what actually represents an AI (position 0 on this list is the Behavior, and it can be mutated easily by moving things around in the list
    // So can do something like a BotBehavior thats like dodge alot until you bait enemy. Then switch to some other strategy when low HP etc. and thats a BotPersonality


    // put init as extraState is null
    public static Tuple<AIPriorityList, AIMemory> AttackAndChaseOrRunawayBot(AIPriorityList ai, BotState bot)
    {
        if (bot.extraState == null)
        {
            // initialize bot
            BehaviorList behaviorsHighHealth = new BehaviorList();

            //TODO:
            // First BehaviorList is: if high hp > 50%
            // 1. AttackTarget Behavior, if: CanAttackRange is true, and chasing memory
            // 2. ChaseTarget Behavior, if: chasing memory
            // 3. Dodge, if: enemy in attack range.
            // 4. Do nothing
            // AI should set chasing in memory to true, if enemy ever misses an attack (cause dodged)
            // AI should set chasing to false again when we do an attack.

            // Second BehaviorList is similar except, default behavior is to run away not do nothing.
            // AI should use 2nd behavior list if lower hp than nearest enemy

            // so basically dont need to use BehaviorAttributes really...
        }


        return null;
    }
}