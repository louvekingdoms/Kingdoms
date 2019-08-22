using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

using static GameLogger;

using KingdomsSharedCode.Generic;
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
        logger = new Logger("KINGDOMS", outputToFile:true);
        logger.SetLevel(Logger.LEVEL.TRACE);  
        logger.SetConsoleFunction(UnityEngine.Debug.Log);

        RelayServer.Relay.logger = new Logger("RELAY", outputToFile:true, outputToConsole:false);
        RelayServer.Relay.logger.SetLevel(Logger.LEVEL.TRACE);

        Client.logger = new Logger("NETWORK_CLIENT", outputToFile: true, outputToConsole: false);
        Client.logger.SetLevel(Logger.LEVEL.TRACE);

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

        // For debug purposes, we host a session immediatly
        networkClient.RequestNewSession();
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
