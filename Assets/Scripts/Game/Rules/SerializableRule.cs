using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class SerializableRule
{
    public enum SRULETYPE { INT, FLOAT, BOOL }

    public string name;
    public string value;
    public SRULETYPE type;

    public RuleInt ToRuleInt()
    {
        return new RuleInt(Convert.ToInt32(value));
    }

    public RuleBool ToRuleBool()
    {
        return new RuleBool(Convert.ToBoolean(value));
    }

    public RuleFloat ToRuleFloat()
    {
        return new RuleFloat(Convert.ToSingle(value));
    }

    public static SRULETYPE GetSType(IRuleValue iRule)
    {
        if (iRule.GetType() == typeof(RuleInt)) return SRULETYPE.INT;
        if (iRule.GetType() == typeof(RuleBool)) return SRULETYPE.BOOL;
        if (iRule.GetType() == typeof(RuleFloat)) return SRULETYPE.FLOAT;

        throw new Exception("SRULETYPE unknown: " + iRule);
    }

    public static Ruleset Serialize(List<SerializableRule> rules)
    {
        var finalRules = new Ruleset();
        foreach(var rule in rules)
        {
            IRuleValue finalRule;
            switch (rule.type)
            {
                default: throw new Exception("SRULETYPE unknown: " + rule);
                case SRULETYPE.INT: finalRule = rule.ToRuleInt(); break;
                case SRULETYPE.BOOL: finalRule = rule.ToRuleBool(); break;
                case SRULETYPE.FLOAT: finalRule = rule.ToRuleFloat(); break;
            }
            finalRules[(RULE)Enum.Parse(typeof(RULE), rule.name)] = finalRule;
        }
        return finalRules;
    }
}
