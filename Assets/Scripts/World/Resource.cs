using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = KingdomsSharedCode.Generic.Logger;

// Varying Resource
public class Resource
{
    float value;
    public readonly Definition definition;

    public Resource(Definition _definition)
    {
        definition = _definition;
        value = definition.start;
    }

    public void Increase(float amount = 1)
    {
        var val = value + amount;
        Set(val);
    }

    public void Decrease(float amount = 1)
    {
        var val = value - amount;
        Set(val);
    }

    public void SetRaw(float val)
    {
        value = val;
    }

    public void Set(float val)
    {
        var min = definition.min == int.MinValue ? val : definition.min;
        var max = definition.max == int.MaxValue ? val : definition.max;
        value = Mathf.Clamp(val, min, max);
    }

    public float GetValue()
    {
        return value;
    }

    // "set-in-stone" definition, rules, chara behavior
    public class Definition
    {
        public float min = 0;
        public float max = 99;
        public float start = 0;
        public bool noModifiers = false;
        public bool isMutable = true;
    }

    // static definitions set
    public class Definitions : Dictionary<string, Resource.Definition> { }
}

// static set of varying characteristics
public class Resources : Dictionary<string, Resource> {
    public float GetValue(string name)
    {
        try
        {
            return this[name].GetValue();
        }
        catch(KeyNotFoundException e)
        {
            throw new KeyNotFoundException("Resource " + name + " was not found among " + string.Join(", ", Keys)+"\n"+ e.ToString());
        }
    }
    public void SetRaw(string name, float value)
    {
        this[name].SetRaw(value);
    }
    public Resource.Definition GetDefinition(string name)
    {
        return this[name].definition;
    }
}
