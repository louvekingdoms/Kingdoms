using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Logger
{
    static type LogLevel = type.DEBUG;

    static StringBuilder builder = new StringBuilder();

    static public string locale = "fr-FR";
    static public int callerLength = 18;
    static public int methodLength = 12;
    static public int flushEverySecond = 1;
    static public bool flushToUnityConsole = false;
    static public bool useNetworkLogger = true;
    static public int networkLoggerPort = 4004;
    static LoggerNetClient netClient;

    public enum type {TRACE, DEBUG, INFO, WARNING, ERROR };

    static CoroutineSlave coroutineSlave;
    static Logger instance;

    static string fullPath;
    static CultureInfo culture;

    static bool isInitialized = false;

    public static void Initialize()
    {
        coroutineSlave = Camera.main.gameObject.AddComponent<CoroutineSlave>();
        culture = new CultureInfo(locale);

        string firstLine = "LOG START - " + DateTime.Now.ToString(culture.DateTimeFormat.LongTimePattern);

        Directory.CreateDirectory(Paths.LogPath());
        fullPath = Paths.LogFile();
        File.WriteAllText(fullPath, firstLine+"\n");

        if (useNetworkLogger)
        {
            netClient = new LoggerNetClient(networkLoggerPort);
            netClient.Send(">clear");
            netClient.Send("<< Logger connection started >>");
        }

        coroutineSlave.StartCoroutine(Flush());
        
        isInitialized = true;
    }


    public static void Trace(params string[] msgs) { LogMessage(type.TRACE, msgs); }
    public static void Debug(params string[] msgs) { LogMessage(type.DEBUG, msgs); }
    public static void Info(params string[] msgs) { LogMessage(type.INFO, msgs); }
    public static void Warn(params string[] msgs) { LogMessage(type.WARNING, msgs); }
    public static void Error(params string[] msgs) { LogMessage(type.ERROR, msgs); }
    public static void Throw(params string[] msgs) {
        LogMessage(type.ERROR, new string[1] { "================== FATAL ==================" });
        LogMessage(type.ERROR, msgs);
        UnityEngine.Debug.LogError(string.Join(" ", msgs).ToString());
        Crash();
    }

    private static void LogMessage(type type, params string[] msgs)
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (type < LogLevel) {
            return;
        }

        string caller = "";

        // Formatting the "X=>Y" file/method pattern
        if (Game.IsRunningInMainThread() && UnityEngine.Debug.isDebugBuild) {
            StackFrame sf = new StackFrame(2, true);
            string file = sf.GetFileName().Replace(Application.dataPath.Replace("/", "\\") + "\\Scripts\\", "");
            string method = sf.GetMethod().Name;
            try {
                caller =
                    file.Substring(0, Mathf.Min(file.Length, callerLength)).PadRight(callerLength)
                    + "=>"
                    + method.Substring(0, Mathf.Min(method.Length, methodLength)).PadRight(methodLength);
            }
            catch (NullReferenceException) {
                caller = "???";
            }
        }

        // Debug line formatting
        string line =
            DateTime.Now.ToString(culture.DateTimeFormat.LongTimePattern)
            + " ["
            + type.ToString()
            + "] "
            + "[" 
            + caller
            + "] - " 
            + string.Join(" ", msgs);
        builder.Append(line).AppendLine();
    }

    private static void Crash()
    {
        coroutineSlave.StartCoroutine(Flush(true));
    }

    private static IEnumerator Flush(bool exit=false)
    {
        // Writes to disk
        string line = builder.ToString();

        if (line.Length > 0)
        {
            File.AppendAllText(fullPath, line);
            yield return true;

            // Output to console
            if (flushToUnityConsole)
            {
                string[] lines = line.Split('\n');
                foreach (string consoleLine in lines)
                {
                    UnityEngine.Debug.Log(consoleLine);
                }
            }
            yield return true;

            // Net
            if (useNetworkLogger)
            {
                string[] lines = line.Split('\n');
                foreach (string consoleLine in lines)
                {
                    netClient.Send(consoleLine);
                    yield return true;
                }
            }
        }

        // Kills the app if exit is set to true - useful to write logs before quitting

        if (exit) {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
        
        // Resets the string builder
        builder = new StringBuilder();
        yield return new WaitForSeconds(flushEverySecond);
        coroutineSlave.StartCoroutine(Flush());
        yield return true;
    }
}