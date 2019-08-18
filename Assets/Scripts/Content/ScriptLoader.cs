using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Logger=KingdomsSharedCode.Generic.Logger;

class ScriptLoader : ScriptLoaderBase
{
    string basePath;

    public ScriptLoader(string basePath)
    {
        ModulePaths = new string[] { "?.lua" };
        this.basePath = basePath;
    }

    public override object LoadFile(string file, Table globalContext)
    {
        Logger.Info("<SCRIPTLOADER> Module '{0}' was imported)".Format(file));
        return Disk.ReadAllText(Path.Combine(basePath, file));
    }

    public override bool ScriptFileExists(string name)
    {
        return true;
    }
}
