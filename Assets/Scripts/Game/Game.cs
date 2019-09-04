using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

using static GameLogger;

using KingdomsSharedCode.Generic;
using KingdomsGame.Networking;

public static class Game
{
    static Thread mainThread = Thread.CurrentThread;

    public static GameFlags flags;
    public static GameState state;
    public static Clock clock;
    public static Client networkClient;
    public static Chat chat;
    public static Players players;

    private static RelayServer.Relay relayServer;

    // TODO: Create proper mouse class
    public static bool isMouseBusy = false;

    static Game()
    {
        // Initialize loggers
        logger = new Logger("KINGDOMS", outputToFile:true);
        logger.SetLevel(Logger.LEVEL.DEBUG);  
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

        chat = new Chat();

        // Initialize local player and player list
        players = new Players();
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
        clock.Kill();
    }

    public static bool IsRunningInMainThread()
    {
        return Thread.CurrentThread == mainThread;
    }
}
