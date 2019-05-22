using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Varying Resource
public class Resource
{
    float value;
    public readonly ResourceDefinition definition;

    public Resource(ResourceDefinition _definition)
    {
        definition = _definition;
        value = definition.start;
    }

    public void Increase(Resources otherChars, float amount = 1)
    {
        var oldVal = value;
        value = Mathf.Clamp(value + amount, definition.min, definition.max);
    }

    public void Decrease(Resources otherChars, float amount = 1)
    {
        var oldVal = value;
        value = Mathf.Clamp(value - amount, definition.min, definition.max);
    }

    public void SetRaw(float val)
    {
        value = val;
    }

    public float GetValue()
    {
        return value;
    }
}

// "set-in-stone" definition, rules, chara behavior
public class ResourceDefinition
{
    public float min = 0;
    public float max = 99;
    public float start = 0;
    public bool noModifiers = false;
    public bool isMutable = true;
}

// static set of varying characteristics
public class Resources : Dictionary<string, Resource> { }

// static definitions set
public class ResourceDefinitions : Dictionary<string, ResourceDefinition> { }
