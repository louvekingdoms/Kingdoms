using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class LuaTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Game.IsRunningInMainThread();
        Library.Initialize();
    }

}
