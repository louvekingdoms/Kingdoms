using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Logger=KingdomsSharedCode.Generic.Logger;
using System;

public static class Game
{
    static Thread mainThread = Thread.CurrentThread;

    public static GameState state;
    public static Clock clock;

    static Game()
    {
        Logger.Initialize("KINGDOMS_CLIENT", outputToFile:true);
        Logger.SetLevel(Logger.LEVEL.TRACE);
        Logger.SetConsoleFunction(UnityEngine.Debug.Log);
    }

    public static void New()
    {
        state = new GameState();
    }

    public static bool IsRunningInMainThread()
    {
        return Thread.CurrentThread == mainThread;
    }
}
