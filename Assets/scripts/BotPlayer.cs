using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using BehaviorList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>;
using ConditionalBehavior = System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>;
using Conditionals = System.Collections.Generic.Dictionary<Condition, bool>;
using ConditionalBehaviorList = System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>>>;
using AIPriorityList = System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, System.Collections.Generic.List<System.Tuple<System.Collections.Generic.Dictionary<Condition, bool>, BotBehavior>> >>;

public delegate UserInput BotBehavior(BotState bot);
// Includes conditions that are determined by AIMemory like chasingOpponent
// Note also includes "Composite Conditions" which are a combination of OR/AND of other conditions
public delegate bool Condition(BotState bot);
public delegate bool BehaviorListAttribute(BotBehavior behavior); // PROBABLY not going to be used much because the programmer will already know ATTACK behavior attacks. Only needed for some insane advanced AI that wants to "create itself" and like "modify all my attacking behaviors to be priotized, or even change the attack it uses"
public delegate Tuple<AIPriorityList, AIMemory> BotAI(AIPriorityList ai, BotState bot);


// Clone objects
public static class ClonerExtensions
{
    public static TObject Clone<TObject>(this TObject toClone)
    {
        var formatter = BinarySerializer.Formatter;
        
        using (var memoryStream = new System.IO.MemoryStream())
        {
            formatter.Serialize(memoryStream, toClone);

            memoryStream.Position = 0;

            return (TObject)formatter.Deserialize(memoryStream);
        }
    }
}

public class Bot
{
    public BotAI ai;
    public BotState state;
    public AIPriorityList aiList;

    public Bot(BotAI ai, BotState state, AIPriorityList aiList)
    {
        this.ai = ai;
        this.state = state;
        this.aiList = aiList;
    }

    public void reset()
    {
        state = new BotState(state.botNumber);
        aiList = null;
    }

    public override string ToString()
    {
        string ret = "";
        ret += "uid:" + state.uid + "\n";
        ret += "ai:" + ((aiList != null) ? Bots.printAIPriorityList(aiList, state) : "null") + "\n";
        ret += "state: " + state.ToString() +"\n";
        return ret;
    }
}

// Bot state that an AI may use to store stuff like what was my previous HP, oh my current is less, must mean I got his this frame
// Should not be modified by BotBehaviors, only by the higher level BotAIs
[Serializable] // for cloning
public class AIMemory
{
    public float lastRecordedHP = 0;
    public string targetUID = null;
    public bool chasingTarget = false;
    
    public AIMemory()
    {

    }

    public AIMemory(float lastRecordedHP, string targetUID, bool chasingTarget)
    {
        this.lastRecordedHP = lastRecordedHP;
        this.targetUID = targetUID;
        this.chasingTarget = chasingTarget;
    }

    public override string ToString()
    {
        string ret = "";
        ret += " lasthp: " + lastRecordedHP;
        ret += " target: " + targetUID;
        ret += " chasingTarget: " + chasingTarget;
        return ret;
    }
    // ...
}

public class BotState
{
    public List<Message> msgs = new List<Message>();
    public readonly string uid;
    public readonly int botNumber;
    // Somethinbg for my history of anims
    // something for 
    public const string BOTUIDPREFIX = "SERVERBOT:";
    public List<CharacterState> charState = new List<CharacterState>(); // TODO :make sure new stuff is added at 0
    public AIMemory extraState = null; // is null when first ran

    public BotState(int botNumber)
    {
        uid = BOTUIDPREFIX + botNumber;
        this.botNumber = botNumber;
        charState.Add(new CharacterState(new CopyMovement(null, new Vector3(0,0,0), new Quaternion(), Constants.canMoveState, 0, false, 0, WeaponType.sword))); // add one so dont have to do length > 0 all the time
    }

    public override string ToString()
    {
        string ret = "";
        ret += " uid:" + uid;
        ret += " msgs: " + msgs.Count;
        ret += " charstates: " + msgs.Count;
        ret += " extra state: " + extraState.ToString();
        return ret;
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
    public static string printAIPriorityList(AIPriorityList ai, BotState bot)
    {
        string ret = "ai:";
        // If its the bots first frame alive, just have it stand still.
        if (bot.msgs.Count <= 0)
        {
            return "no msgs ret nothing ";
        }

        //: return null if BotBehavior is null, Otherwise get BotBehavior by going through ai
        BehaviorList blist = null;
        if (ai != null)
        {
            foreach (var condBList in ai)
            {
                ret += "conditional Blist :\n";
                foreach (var conditional in condBList.Item1)
                {
                    ret += "c:" + conditional.Key.Method.Name;
                    ret += " " + (conditional.Key(bot) == conditional.Value);
                }
                if (checkConditionals(condBList.Item1, bot) && blist == null)
                {
                    blist = condBList.Item2;
                    ret += "\n PICKED THIS \n";
                }
            }
        }

        BotBehavior b = null;
       
        if (blist != null)
        {
            foreach (var condBehav in blist)
            {
                ret += "conditional behave :\n";
                foreach (var conditional in condBehav.Item1)
                {
                    ret += "c:" + conditional.Key.Method.Name;
                    ret += " " + (conditional.Key(bot) == conditional.Value);
                }
                if (checkConditionals(condBehav.Item1, bot))
                {
                    b = condBehav.Item2;
                    ret += "\n PICKED THIS \n";
                }
            }
        }

        //if (b != null)
        //{
        //    return b(bot);
        //}
        return ret;
    }

    public static bool botAlive(BotState b, int activeConnections, int maxBots)
    {
        // If get bot number-> botnumber active connections, return true if not many connections and bot number low.
        // Lower the botNumber more likely your alive

        return b.botNumber < maxBots - activeConnections;
    }

    public static bool checkConditionals(Conditionals c, BotState bot)
    {
        bool allCheck = true;
        foreach (var conditional in c)
        {
            if (conditional.Key(bot) != conditional.Value)
            {
                allCheck = false;
            }
        }
        return allCheck;
    }

    public static UserInput getBotAction(AIPriorityList ai, BotState bot)
    {
        // If its the bots first frame alive, just have it stand still.
        if (bot.msgs.Count <= 0)
        {
            return Behaviors.standStill(bot); 
        }

        //: return null if BotBehavior is null, Otherwise get BotBehavior by going through ai
        BehaviorList blist = null;
        if (ai != null)
        {
            foreach (var condBList in ai)
            {
                if (checkConditionals(condBList.Item1, bot))
                {
                    blist = condBList.Item2;
                    break;
                }
            }
        }

        BotBehavior b = null;
        if (blist != null)
        {
            foreach(var condBehav in blist)
            {
                if (checkConditionals(condBehav.Item1, bot))
                {
                    b = condBehav.Item2;
                    break;
                }
            }
        }

        if (b != null)
        {
            var ret = b(bot);
            return ret;
        }
        return null; //TODO NOTE: If a bot returns null long enough it will die from the Server thinking the "player" hasnt sent a message in a long time (dced). // maybe this is fine though, respawn bot in a diff random loc?
    }

    // put init as extraState is null
    public static Tuple<AIPriorityList, AIMemory> AttackAndChaseOrRunawayBot(AIPriorityList ai, BotState bot)
    {
        if (bot.extraState == null)
        {
            // initialize bot
            
            AIPriorityList retAI = new AIPriorityList();
            //TODO:
            // First BehaviorList is: if high hp > 50%
            // 1. AttackTarget Behavior, if: CanAttackRange is true, and chasing memory
            // 2. ChaseTarget Behavior, if: chasing memory
            // 3. Dodge, if: enemy in attack range.
            // 4. Do nothing
            // AI should set chasing in memory to true, if enemy ever misses an attack (cause dodged)
            // AI should set chasing to false again when we do an attack.
            
            BehaviorList highHealth = new BehaviorList();

            Dictionary<Condition, bool> attackIfChasingConditions = new Dictionary<Condition, bool>();
            attackIfChasingConditions.Add(Conditions.CanAttackRange, true);
            attackIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            ConditionalBehavior attackIfChasing = new Tuple<Dictionary<Condition, bool>, BotBehavior>(attackIfChasingConditions, Behaviors.AttackTarget(0));

            Dictionary<Condition, bool> runAtIfChasingConditions = new Dictionary<Condition, bool>();
            runAtIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            ConditionalBehavior runAtIfChasing = new Tuple<Dictionary<Condition, bool>, BotBehavior>(runAtIfChasingConditions, Behaviors.chaseTarget);

            Dictionary<Condition, bool> dodgeConditions = new Dictionary<Condition, bool>();
            dodgeConditions.Add(Conditions.EnemiesCouldAttackRange, true);
            ConditionalBehavior dodge = new ConditionalBehavior(dodgeConditions, Behaviors.dodgeAway);

            Conditionals nothingCond = new Conditionals();
            ConditionalBehavior nothing = new ConditionalBehavior(nothingCond, Behaviors.standStill);

            highHealth.Add(attackIfChasing);
            highHealth.Add(runAtIfChasing);
            highHealth.Add(dodge);
            highHealth.Add(nothing);

            Conditionals highHealthCondition = new Conditionals();
            highHealthCondition.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), true);

            retAI.Add(new ConditionalBehaviorList(highHealthCondition, highHealth));

            // Second BehaviorList is similar except, default behavior is to run away not do nothing.
            // AI should use 2nd behavior list if lower hp than nearest enemy
            BehaviorList lowHealth = new BehaviorList();

            Conditionals runAwayCond = new Conditionals();
            ConditionalBehavior runAway = new ConditionalBehavior(runAwayCond, Behaviors.RunAway);

            lowHealth.Add(attackIfChasing);
            lowHealth.Add(runAtIfChasing);
            lowHealth.Add(dodge);
            lowHealth.Add(runAway);

            Conditionals lowHealthConds = new Conditionals();
            lowHealthConds.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), false);

            retAI.Add(new ConditionalBehaviorList(lowHealthConds, lowHealth));
            // so basically dont need to use BehaviorAttributes really...
            // Just set chasing to when enemy misses, or enemy has low HP.
            var ret = Tuple.Create(retAI, new AIMemory());
            return ret;
        }
        else
        {
            AIMemory retMem = bot.extraState.Clone<AIMemory>();
            
            if (Conditions.closestEnemyMissedInAttackRange(bot) && !Conditions.selfAttacking(bot))
            {
                retMem.targetUID = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.charState[0].myState.localPosition).objectInfo.uid;
                retMem.chasingTarget = true;
            }
            else if (Conditions.nearByEnemyHealthLessThan(20, 40)(bot))
            {
                var enemiesClose = BotHelpers.getEnemies(bot).FindAll(cp => Vector3.Distance(cp.localPosition, bot.charState[0].myState.localPosition) <= 40);
                var enemiesLow = enemiesClose.FindAll(cp => cp.health <= 20);
                retMem.targetUID = BotHelpers.getClosest(enemiesLow, bot.charState[0].myState.localPosition).objectInfo.uid;
                retMem.chasingTarget = true;
            }

            if (retMem.chasingTarget && Conditions.selfAttacking(bot))
            {
                // retMem.targetUID = null; // Not sure if good? Maybe need to keep target but not chase?
                retMem.chasingTarget = false;
            }

            return Tuple.Create(ai, retMem);
        }
        
    }



    // put init as extraState is null
    public static Tuple<AIPriorityList, AIMemory> AggroLowHealth(AIPriorityList ai, BotState bot)
    {
        if (bot.extraState == null)
        {
            // initialize bot

            AIPriorityList retAI = new AIPriorityList();
            //:
            // First BehaviorList is: if high hp > 50%
            // 1. AttackTarget Behavior, if: CanAttackRange is true, and chasing memory
            // 2. ChaseTarget Behavior, if: chasing memory
            // 3. Dodge, if: enemy in attack range.
            // 4. Do nothing
            

            BehaviorList highHealth = new BehaviorList();

            Dictionary<Condition, bool> attackIfChasingConditions = new Dictionary<Condition, bool>();
            attackIfChasingConditions.Add(Conditions.CanAttackRange, true);
            attackIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            ConditionalBehavior attackIfChasing = new Tuple<Dictionary<Condition, bool>, BotBehavior>(attackIfChasingConditions, Behaviors.AttackTarget(0));

            Dictionary<Condition, bool> runAtIfChasingConditions = new Dictionary<Condition, bool>();
            runAtIfChasingConditions.Add(Conditions.memoryIsChasing, true);
            ConditionalBehavior runAtIfChasing = new Tuple<Dictionary<Condition, bool>, BotBehavior>(runAtIfChasingConditions, Behaviors.chaseTarget);

            Dictionary<Condition, bool> dodgeConditions = new Dictionary<Condition, bool>();
            dodgeConditions.Add(Conditions.EnemiesCouldAttackRange, true);
            ConditionalBehavior dodge = new ConditionalBehavior(dodgeConditions, Behaviors.dodgeAway);

            Conditionals nothingCond = new Conditionals();
            ConditionalBehavior nothing = new ConditionalBehavior(nothingCond, Behaviors.standStill);

            highHealth.Add(attackIfChasing);
            highHealth.Add(runAtIfChasing);
            highHealth.Add(dodge);
            highHealth.Add(nothing);

            Conditionals highHealthCondition = new Conditionals();
            //highHealthCondition.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), true);

            retAI.Add(new ConditionalBehaviorList(highHealthCondition, highHealth));

            // Second BehaviorList is similar except, default behavior is to run away not do nothing.
            // AI should use 2nd behavior list if lower hp than nearest enemy
            //BehaviorList lowHealth = new BehaviorList();

            //Conditionals runAwayCond = new Conditionals();
            //ConditionalBehavior runAway = new ConditionalBehavior(runAwayCond, Behaviors.RunAway);

            //lowHealth.Add(attackIfChasing);
            //lowHealth.Add(runAtIfChasing);
            //lowHealth.Add(dodge);
            //lowHealth.Add(runAway);

            //Conditionals lowHealthConds = new Conditionals();
            //lowHealthConds.Add(Conditions.selfHealthGreaterThan(Constants.startHP / 2), false);

            //retAI.Add(new ConditionalBehaviorList(lowHealthConds, lowHealth));
            //// so basically dont need to use BehaviorAttributes really...
            //// Just set chasing to when enemy misses, or enemy has low HP.

            var ret = Tuple.Create(retAI, new AIMemory());
            return ret;
        }
        else
        {
            AIMemory retMem = bot.extraState.Clone<AIMemory>();

            if (!retMem.chasingTarget && !Conditions.selfAttacking(bot))
            {
                var enemies = BotHelpers.getEnemies(bot);
                if (enemies.Count > 0)
                {
                    enemies.Sort((e1, e2) => {
                        if (e1.health == e2.health) return 0;
                        if (e1.health < e2.health) return -1;
                        if (e1.health > e2.health) return 1;
                        return 0;
                    });

                    Debug.Assert(enemies.Count <= 1 || enemies[0].health <= enemies[enemies.Count - 1].health);
                    if (enemies.Count == 1)
                    {
                        retMem.targetUID = enemies[0].objectInfo.uid;
                    }
                    else if (enemies[0].health == enemies[enemies.Count - 1].health)
                    {
                        retMem.targetUID = BotHelpers.getClosest(BotHelpers.getEnemies(bot), bot.charState[0].myState.localPosition).objectInfo.uid;
                    }
                    else
                    {
                        var lowEnemies = enemies.FindAll(e => e.health <= enemies[0].health);
                        retMem.targetUID = BotHelpers.getClosest(lowEnemies, bot.charState[0].myState.localPosition).objectInfo.uid;
                    }
                    retMem.chasingTarget = true;
                }
            }

            if (retMem.chasingTarget && Conditions.selfAttacking(bot))
            {
                // retMem.targetUID = null; // Not sure if good? Maybe need to keep target but not chase?
                retMem.chasingTarget = false;
            }

            return Tuple.Create(ai, retMem);
        }

    }
}