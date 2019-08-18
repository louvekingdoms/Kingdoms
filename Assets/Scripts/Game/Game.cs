using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

using Logger=KingdomsSharedCode.Generic.Logger;

using Kingdoms.Network;

public static class Game
{
    static Thread mainThread = Thread.CurrentThread;

    public static GameState state;
    public static Clock clock;

    private static Client networkClient;
    private static RelayServer.Relay relayServer;

    static Game()
    {
        Logger.Initialize("KINGDOMS_CLIENT", outputToFile:true);
        Logger.SetLevel(Logger.LEVEL.TRACE);
        Logger.SetConsoleFunction(UnityEngine.Debug.Log);

        // Start relay server
        relayServer = new RelayServer.Relay(
            RelayServer.Config.LOCAL_ADDR,
            RelayServer.Config.PORT,
            directMode: true
        );
        new Thread((ThreadStart)delegate {
            relayServer.WaitUntilDeath().Wait();
        }).Start();
        
        Library.Initialize();
    }

    public static void Hello() { 
        // Dummy function to initialize game static constructor
    }

    public static void Start()
    {
        // Loads all rules from default settings
        Ruleset rules = new Ruleset();
        Rules.LoadRuleset(rules);

        // New reinitialized gamestate
        state = new GameState();

        // Initializes clock
        clock = new Clock();
        clock.SetCalendar(Rules.set[RULE.DAYS_IN_YEAR].GetInt(), Rules.set[RULE.MONTHS_IN_YEAR].GetInt());

        // Starts a network client in local mode
        networkClient = new Client(
            RelayServer.Config.LOCAL_ADDR,
            RelayServer.Config.PORT,
            isSlave: false
        );
        new Thread((ThreadStart)delegate {
            networkClient.Run();
        }).Start();
    }

    public static void KillThreads(){
        networkClient.Kill();
        relayServer.Kill();
    }

    public static bool IsRunningInMainThread()
    {
        return Thread.CurrentThread == mainThread;
    }
}
