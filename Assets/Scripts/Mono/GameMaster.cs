using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster instance;

    public static HardData hd;
    public static HardData hardData { get { return hd; } }

    [Header("HD")]
    public GameObject hardDataPrefab;

    [Header("SCENE ELEMENTS")]
    public MapDisplayer mapDisplayer;
    public PawnsManager pawnsManager;

    void Awake()
    {
        if (hardDataPrefab == null)
        {
            throw new System.Exception("No hard data loaded for game master");
        }

        instance = this;
        hd = Instantiate(hardDataPrefab).GetComponent<HardData>();
    }

    private void OnLevelWasLoaded(int level)
    {
        if (FindObjectsOfType<HardData>().Length > 1)
        {
            DestroyImmediate(this);
        }
    }
}
