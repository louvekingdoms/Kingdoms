using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RULE { STARTING_KINGDOMS, MAX_STARTING_KINGDOM_STRENGTH, STARTING_POPULATION_PER_REGION};

public interface IRuleValue
{
    float GetFloat();
    int GetInt();
    bool GetBool();
}

#region IRules

public class RuleFloat : IRuleValue
{
    float value = 0f;
    public RuleFloat(float _value)
    {
        value = _value;
    }
    public bool GetBool(){ throw new System.NotImplementedException(); }
    public float GetFloat(){ return value; }
    public int GetInt() { throw new System.NotImplementedException(); }
}

public class RuleInt : IRuleValue
{
    int value = 0;
    public RuleInt(int _value)
    {
        value = _value;
    }
    public bool GetBool() { throw new System.NotImplementedException(); }
    public float GetFloat() { throw new System.NotImplementedException(); }
    public int GetInt() { return value; }
}

public class RuleBool : IRuleValue
{
    bool value = false;
    public RuleBool(bool _value)
    {
        value = _value;
    }
    public bool GetBool() { return value; }
    public float GetFloat() { throw new System.NotImplementedException(); }
    public int GetInt() { throw new System.NotImplementedException(); }
}

#endregion

public class Ruleset : Dictionary<RULE, IRuleValue> {
    public Ruleset()
    {
        this.Add(RULE.STARTING_KINGDOMS, new RuleInt(1));
        this.Add(RULE.MAX_STARTING_KINGDOM_STRENGTH, new RuleInt(5));
        this.Add(RULE.STARTING_POPULATION_PER_REGION, new RuleInt(200));
    }

}

public class Rules
{
    public static Ruleset set;

    public static void LoadRuleset(Ruleset _set)
    {
        set = _set;
    }
}