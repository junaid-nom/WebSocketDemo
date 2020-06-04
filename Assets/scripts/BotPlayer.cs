using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BotPlayer
{
    public List<Message> msgs;
    public string uid;
    // Somethinbg for my history of anims
    // something for 
    public const string BOTUIDPREFIX = "SERVERBOT:";
    public List<CharacterState> charState; // make sure new stuff is added at 0
}
public class CharacterState
{
    public Vector3 position;
    public string animation;
    public float normalizedTime;
}

public enum Condition
{
    LowHealthSelf, LowHealthEnemy, EnemyClose, EnemyAttacking, EnemyMissAttack, EnemyWillHit
}

public enum BehaviorAttribute
{
    HasRunAway, HasAttack, HasSpecificAttack, HasDodgeBait, HasWalkToward
}

public static class Bots
{
    public static bool botAlive(BotPlayer b, int activeConnections)
    {
        // If get bot number-> botnumber active connections, return true if not many connections and bot number low.
        return false;
    }

    public delegate UserInput BotBehavior(BotPlayer b);
    public delegate BotBehavior BotPersonality(BotPlayer b);
    // So can do something like a BotBehavior thats like dodge alot until you bait enemy. Then switch to some other strategy when low HP etc. and thats a BotPersonality

    public static BotBehavior RunAway = (bot) =>
    {
        UserInput ret = new UserInput();

        // Find NearBy Enemies.
        List<Message> cpMsgs = bot.msgs.FindAll(m => m.msgType == 1);
        List<CopyMovement> copyMovements = new List<CopyMovement>();
        cpMsgs.ForEach(cp => copyMovements.Add((CopyMovement)cp)); // cast
        // Get direction away from them
        List<CopyMovement> enemies = copyMovements.FindAll(cp => cp.objectInfo.uid != bot.uid);
        Vector2 runTo = new Vector2(0, 0);
        Vector2 myLoc = bot.charState.Count > 0 ? new Vector2(bot.charState[0].position.x, bot.charState[0].position.z) : new Vector2(0, 0);
        enemies.ForEach(e =>
        {
            Vector2 loc2d = new Vector2(e.localPosition.x, e.localPosition.z);
            loc2d = loc2d * -1;
            loc2d = loc2d * Vector2.Distance(loc2d, myLoc);
            runTo += loc2d;
        });

        //Find closest direction of the 8 a player can move to runTo
        Vector2 closestDir = new Vector2(0, 0);
        for (int x = -1; x <= 1;  x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 xy = new Vector2(x, y);
                if (Vector2.Distance(xy, runTo) < Vector2.Distance(closestDir, runTo))
                {
                    closestDir = xy;
                }
            }
        }
        ret.x = closestDir.x;
        ret.y = closestDir.y;
        ret.buttonsDown = new List<bool>() { false, false, false, false};
        if (EnemiesCouldAttackRange(bot))
        {
            ret.buttonsDown[3] = true;//dodge away
        }

        return ret;
    };

    public static bool EnemiesCouldAttackRange(BotPlayer b)
    {
        // TODO figure this out
        // probably need "weapon info" somewhere. like EffectiveRange. (high for spear, low for 1h sword)
        return false;
    }

    public static BotBehavior DoNothingBot(BotPlayer b)
    {
        Dictionary<Condition, bool> hasConditions;
        Dictionary<BehaviorAttribute, bool> hasBAttributes;
        Tuple<Dictionary<Condition, bool>, BotBehavior> behavior;


        // Tuple< Dictionary<Condition, bool>, BotBehavior > is a BotBehavior that should be done when all the conditions in the dictionary are met. aka ConditionalBehavior
        // A Priority list of ConditionalBehaviors is an AI
        List< Tuple< Dictionary<Condition, bool>, BotBehavior > > AI;

        // Then we have a list of AIChoices. These are moved around priorty wise based on deciding functions. 
        // Such as, if Lower Health than Enemy nearby run away.  
        List<Tuple<Dictionary<BehaviorAttribute, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> AIChoices;

        return (bot) => new UserInput();
    }

    public static BotBehavior IfWinningAggroIfLosingCaustious(BotPlayer b)
    {

        return (bot) => new UserInput();
    }

    // Need to keep track of state of bot...
    public static CharacterState getMyCharacterState(BotPlayer b, List<Message> currentGameState)
    {
        return null;
    }


    
    // TODO: Make a bunch of bool returning functions
    public static bool safeFromAttack(BotPlayer b)
    {
        return true;
    }
}