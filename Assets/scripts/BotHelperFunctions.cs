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
        List<Message> cpMsgs = bot.msgs.FindAll(m => m.GetType() == typeof(CopyMovement)); // m.msgType == 1
        List<CopyMovement> copyMovements = new List<CopyMovement>();
        cpMsgs.ForEach(cp => copyMovements.Add((CopyMovement)cp)); // cast
        // Get direction away from them
        List<CopyMovement> enemies = copyMovements.FindAll(cp => cp.objectInfo.uid != bot.uid && cp.anim_state != Constants.deathState);
        return enemies;
    }

    public static CopyMovement getClosest(List<CopyMovement> enemies, Vector3 loc)
    {
        float minDistance = 0;
        CopyMovement enemyClosest = null;
        enemies.ForEach(e =>
        {
            var distance = Vector3.Distance(e.localPosition, loc);
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

    public static Vector2 positionToInputDirectionsFrom0_0(Vector2 runTo)
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

    public static Vector2 positionToInputDirections(Vector2 start, Vector2 runTo)
    {
        Vector2 closestDir = new Vector2(0, 0);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 dir = new Vector2(x, y);
                if (Vector2.Distance(dir + start, runTo) < Vector2.Distance(closestDir + start, runTo))
                {
                    closestDir = dir;
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
        CopyMovement closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition);
        if (closest == null)
        {
            return false;
        }
        return Vector3.Distance(bot.getCharacterState(0).myState.localPosition, closest.localPosition) <= range;
    }

    public static bool CanAttackRange(BotState bot)
    {
        // TODO: Eventually see what weapon I have equiped and use that ones info;
        float range = Constants.swordInfo.avgRange;
        CopyMovement closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition);
        if (closest == null)
        {
            return false;
        }
        return Vector3.Distance(bot.getCharacterState(0).myState.localPosition, closest.localPosition) <= range;
    }

    public static bool CanAttackRangeTarget(BotState bot)
    {
        // TODO: Eventually see what weapon I have equiped and use that ones info;
        float range = Constants.swordInfo.avgRange;
        if (bot.extraState.targetUID == null)
        {
            return false;
        }
        CopyMovement target = BotHelpers.getEnemies(bot).Find(cp => cp.objectInfo.uid == bot.extraState.targetUID);
        if (target == null)
        {
            return false;
        }
        return Vector3.Distance(bot.getCharacterState(0).myState.localPosition, target.localPosition) <= range;
    }

    public static bool closestEnemyMissedInAttackRange(BotState bot)
    {
        CopyMovement closest = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.getCharacterState(0).myState.localPosition);
        if (closest !=null && closest.anim_state!=null && Constants.attackAnimationInfo.nameToAnimation.ContainsKey(closest.anim_state))
        {
            AnimationClip attack = Constants.attackAnimationInfo.nameToAnimation[closest.anim_state];
            float leftOverTime = attack.length - closest.normalizedTime;
            float distance = Vector3.Distance(bot.getCharacterState(0).myState.localPosition, closest.localPosition);
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
        if (bot.getCharacterState(0).myState.anim_state == null)
        {
            return false;
        }
        
        return Constants.attackAnimationInfo.nameToAnimation.ContainsKey(bot.getCharacterState(0).myState.anim_state);
    }

    public static bool memoryIsChasing(BotState bot)
    {
        return bot.extraState.chasingTarget;
    }

    public static Condition selfHealthGreaterThan(float health)
    {
        return (bot) =>
        {
            return bot.getCharacterState(0).myState.health >= health;
        };
    }

    public static Condition nearByEnemyHealthLessThan(float health, float range)
    {
        return (bot) =>
        {
            var enemiesClose = BotHelpers.getEnemies(bot).FindAll(cp => Vector3.Distance(cp.localPosition, bot.getCharacterState(0).myState.localPosition) <= range);
            var enemiesLow = enemiesClose.FindAll(cp => cp.health <= health);
            return enemiesLow.Count > 0;
        };
    }
}

public static class Behaviors
{
    public static List<bool> defaultButtons()
    {
        return new List<bool>() { false, false, false, false, false };
    }
    public static BotBehavior RunAway = (bot) =>
    {
        UserInput ret = new UserInput();

        List<CopyMovement> enemies = BotHelpers.getEnemies(bot);

        Vector2 runTo = BotHelpers.getSafestDirection(bot.getCharacterState(0).myState.localPosition, enemies);

        //Find closest direction of the 8 a player can move to runTo
        Vector2 closestDir = BotHelpers.positionToInputDirectionsFrom0_0(runTo);

        ret.x = closestDir.x;
        ret.y = closestDir.y;

        ret.buttonsDown = defaultButtons();
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

        Vector2 runTo = BotHelpers.getSafestDirection(bot.getCharacterState(0).myState.localPosition, enemies);

        //Find closest direction of the 8 a player can move to runTo
        Vector2 closestDir = BotHelpers.positionToInputDirectionsFrom0_0(runTo);

        ret.x = closestDir.x;
        ret.y = closestDir.y;

        ret.buttonsDown = defaultButtons();

        ret.buttonsDown[3] = true;//dodge away

        return ret;
    };

    public static BotBehavior standStill = (bot) =>
    {
        UserInput ret = new UserInput();
        ret.x = 0;
        ret.y = 0;
        ret.buttonsDown = defaultButtons();

        return ret;
    };

    public static BotBehavior pickUpItem = (bot) =>
    {
        UserInput ret = new UserInput();
        ret.x = 0;
        ret.y = 0;
        ret.buttonsDown = defaultButtons();
        ret.buttonsDown[4] = true;
        return ret;
    };

    // attacks and chases CONTINOUSLY FOREVER, up to AI func to put a condition to stop this
    public static BotBehavior chaseTarget = (bot) =>
    {
        UserInput ret = new UserInput();

        CopyMovement target = BotHelpers.getSpecificEnemy(bot, bot.extraState.targetUID);
        if (target != null)
        {
            Vector2 dir = BotHelpers.positionToInputDirections(new Vector2(bot.getCharacterState(0).myState.localPosition.x, bot.getCharacterState(0).myState.localPosition.z), new Vector2(target.localPosition.x, target.localPosition.z));
            ret.x = dir.x;
            ret.y = dir.y;
            if (Constants.inspectorDebugging)
            {
                Server.inspectorDebugger.addPair(new StringPair(bot.uid + "chase", "x:" + ret.x + "y" + ret.y));
            }
            ret.buttonsDown = defaultButtons();
        }
        else
        {
            //Do nothing
            ret.x = 0;
            ret.y = 0;
            ret.buttonsDown = defaultButtons();
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
                Vector2 dir = BotHelpers.positionToInputDirections(new Vector2(bot.getCharacterState(0).myState.localPosition.x, bot.getCharacterState(0).myState.localPosition.z), new Vector2(target.localPosition.x, target.localPosition.z));
                ret.x = dir.x;
                ret.y = dir.y;
                if (Constants.inspectorDebugging)
                {
                    Server.inspectorDebugger.addPair(new StringPair(bot.uid + "attack", "x:" + ret.x + "y" + ret.y));
                }

                ret.buttonsDown = defaultButtons();
                ret.buttonsDown[buttonIndex] = true;
                ret.target = target.localPosition;
            }
            else
            {
                //Do nothing
                ret.x = 0;
                ret.y = 0;
                ret.buttonsDown = defaultButtons();
            }

            return ret;
        };
    }

}
