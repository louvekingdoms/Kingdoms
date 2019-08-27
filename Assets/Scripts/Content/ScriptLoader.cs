using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static GameLogger;

class ScriptLoader : ScriptLoaderBase
{
    string basePath;

    public ScriptLoader(string baseDirPath)
    {
        IgnoreLuaPathGlobal = true;
        ModulePaths = new string[] {
            Path.Combine(Paths.CommonLuaPath(), "?.lua"),
            Path.Combine(baseDirPath, "?.lua")
        };

        this.basePath = baseDirPath;
    }

    public override object LoadFile(string file, Table globalContext)
    {
        logger.Debug("<SCRIPTLOADER> Requesting import of '{0}')".Format(file));
        return Disk.ReadAllText(Path.Combine(basePath, file));
    }

    public override bool ScriptFileExists(string name)
    { // TODO: Sandbox
        return File.Exists(name);
    }
}
