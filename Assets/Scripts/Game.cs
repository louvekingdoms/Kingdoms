using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public static class Game
{
    static Thread mainThread = Thread.CurrentThread;

    public static bool IsRunningInMainThread()
    {
        return Thread.CurrentThread == mainThread;
    }
}
