using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BotHelpers
{
    // Need to keep track of state of bot...
    public static CharacterState getMyCharacterState(BotState b, List<Message> currentGameState)
    {
        return null;
    }

    public static List<CopyMovement> getEnemies(BotState bot)
    {
        List<Message> cpMsgs = bot.msgs.FindAll(m => m.msgType == 1);
        List<CopyMovement> copyMovements = new List<CopyMovement>();
        cpMsgs.ForEach(cp => copyMovements.Add((CopyMovement)cp)); // cast
        // Get direction away from them
        List<CopyMovement> enemies = copyMovements.FindAll(cp => cp.objectInfo.uid != bot.uid);
        return enemies;
    }

    public static CopyMovement getClosest(List<CopyMovement> enemies, Vector3 loc)
    {
        float minDistance = 0;
        CopyMovement enemyClosest = null;
        enemies.ForEach(e =>
        {
            var distance = Vector3.Distance(enemyClosest.localPosition, loc);
            if (enemyClosest == null || distance < minDistance)
            {
                minDistance = distance;
                enemyClosest = e;
            }
        });

        return enemyClosest;
    }

    public static Vector2 getSafestDirection(Vector3 location, List<CopyMovement> enemies)
    {
        Vector2 runTo = new Vector2(0, 0);
        Vector2 myLoc = new Vector2(location.x, location.z);
        enemies.ForEach(e =>
        {
            Vector2 loc2d = new Vector2(e.localPosition.x, e.localPosition.z);
            loc2d = loc2d * -1;
            loc2d = loc2d * Vector2.Distance(loc2d, myLoc);
            runTo += loc2d;
        });
        return runTo;
    }

    public static Vector2 positionToInputDirections(Vector2 runTo)
    {
        Vector2 closestDir = new Vector2(0, 0);
        for (int x = -1; x <= 1; x++)
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
        return closestDir;
    }
    
    public static CopyMovement getSpecificEnemy(BotState bot, string uid)
    {
        //TODO: this could get a "bullet" or spawned thingy of a player that isnt the player itself in the future unless changed.
        var enemies = BotHelpers.getEnemies(bot);
        var lookingfor = enemies.FindAll(cp => cp.objectInfo.uid == uid);
        if (lookingfor.Count > 0)
        {
            return lookingfor[0];
        } else
        {
            return null;
        }
    }
    
}

public static class Conditions
{
    public static bool EnemiesCouldAttackRange(BotState bot)
    {
        // TODO: Eventually see what weapon they have equiped and use that ones info;
        float range = Constants.swordInfo.avgRange;
        CopyMovement closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.charState[0].myState.localPosition);
        return Vector3.Distance(bot.charState[0].myState.localPosition, closest.localPosition) <= range;
    }

    public static bool CanAttackRange(BotState bot)
    {
        // TODO: Eventually see what weapon I have equiped and use that ones info;
        float range = Constants.swordInfo.avgRange;
        CopyMovement closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.charState[0].myState.localPosition);
        return Vector3.Distance(bot.charState[0].myState.localPosition, closest.localPosition) <= range;
    }

    public static bool closestEnemyMissedInAttackRange(BotState bot)
    {
        CopyMovement closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.charState[0].myState.localPosition);
        if (Constants.attackAnimationInfo.nameToAnimation.ContainsKey(closest.anim_state))
        {
            AnimationClip attack = Constants.attackAnimationInfo.nameToAnimation[closest.anim_state];
            float leftOverTime = attack.length - closest.normalizedTime;
            float distance = Vector3.Distance(bot.charState[0].myState.localPosition, closest.localPosition);
            float runTime = distance / Constants.charMoveSpeed;
            if (leftOverTime - Constants.timeNeededToCounterAttack > runTime)
            {
                return true;
            }
        }
        return false;
    }

    public static bool selfAttacking(BotState bot)
    {
        return Constants.attackAnimationInfo.nameToAnimation.ContainsKey(bot.charState[0].myState.anim_state);
    }
}

public static class Behaviors
{
    public static BotBehavior RunAway = (bot) =>
    {
        UserInput ret = new UserInput();

        List<CopyMovement> enemies = BotHelpers.getEnemies(bot);

        Vector2 runTo = BotHelpers.getSafestDirection(bot.charState[0].myState.localPosition, enemies);

        //Find closest direction of the 8 a player can move to runTo
        Vector2 closestDir = BotHelpers.positionToInputDirections(runTo);

        ret.x = closestDir.x;
        ret.y = closestDir.y;

        ret.buttonsDown = new List<bool>() { false, false, false, false };
        if (Conditions.EnemiesCouldAttackRange(bot))
        {
            ret.buttonsDown[3] = true;//dodge away
        }

        return ret;
    };

    public static BotBehavior dodgeAway = (bot) =>
    {
        UserInput ret = new UserInput();

        List<CopyMovement> enemies = BotHelpers.getEnemies(bot);

        Vector2 runTo = BotHelpers.getSafestDirection(bot.charState[0].myState.localPosition, enemies);

        //Find closest direction of the 8 a player can move to runTo
        Vector2 closestDir = BotHelpers.positionToInputDirections(runTo);

        ret.x = closestDir.x;
        ret.y = closestDir.y;

        ret.buttonsDown = new List<bool>() { false, false, false, false };

        ret.buttonsDown[3] = true;//dodge away

        return ret;
    };

    public static BotBehavior standStill = (bot) =>
    {
        UserInput ret = new UserInput();
        ret.x = 0;
        ret.y = 0;
        ret.buttonsDown = new List<bool>() { false, false, false, false };

        return ret;
    };

    // attacks and chases CONTINOUSLY FOREVER, up to AI func to put a condition to stop this
    public static BotBehavior chaseTarget = (bot) =>
    {
        UserInput ret = new UserInput();

        CopyMovement target = BotHelpers.getSpecificEnemy(bot, bot.extraState.targetUID);
        if (target != null)
        {
            Vector2 dir = BotHelpers.positionToInputDirections(new Vector2(target.localPosition.x, target.localPosition.z));
            ret.x = dir.x;
            ret.y = dir.y;
            ret.buttonsDown = new List<bool>() { false, false, false, false };
        }
        else
        {
            //Do nothing
            ret.x = 0;
            ret.y = 0;
            ret.buttonsDown = new List<bool>() { false, false, false, false };
        }

        return ret;
    };

    // This is a function that returns a BotBehavior function
    public static BotBehavior AttackTarget(int buttonIndex) {
        return (bot) =>
        {
            UserInput ret = new UserInput();

            CopyMovement target = BotHelpers.getSpecificEnemy(bot, bot.extraState.targetUID);
            if (target != null)
            {
                Vector2 dir = BotHelpers.positionToInputDirections(new Vector2(target.localPosition.x, target.localPosition.z));
                ret.x = dir.x;
                ret.y = dir.y;

                ret.buttonsDown = new List<bool>() { false, false, false, false };
                ret.buttonsDown[buttonIndex] = true;
            }
            else
            {
                //Do nothing
                ret.x = 0;
                ret.y = 0;
                ret.buttonsDown = new List<bool>() { false, false, false, false };
            }

            return ret;
        };
    }

}
