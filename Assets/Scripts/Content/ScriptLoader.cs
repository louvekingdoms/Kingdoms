using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ScriptLoader : ScriptLoaderBase
{
    public override object LoadFile(string file, Table globalContext)
    {
        return string.Format("print ([[A request to load '{0}' has been made]])", file);
    }

    public override bool ScriptFileExists(string name)
    {
        return true;
    }
}
