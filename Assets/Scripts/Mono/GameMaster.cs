using CatchCo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameMaster : MonoBehaviour
{
    public static GameMaster instance;

    public static HardData hd;
    public static HardData hardData { get { return hd; } }

    [Header("RULES")]
    public List<SerializableRule> sRules;

    [Header("HD")]
    public GameObject hardDataPrefab;

    [Header("FLAGS")]
    public GameFlags flags;

    [Header("SCENE ELEMENTS")]
    public MapDisplayer mapDisplayer;
    public PawnsManager pawnsManager;
    
    void Awake()
    {
        if (FindObjectsOfType<HardData>().Length > 1)
        {
            DestroyImmediate(this);
        }

        DontDestroyOnLoad(this.gameObject);

        Game.Start(SerializableRule.Serialize(sRules));

        if (hardDataPrefab == null)
        {
            throw new System.Exception("No hard data loaded for game master");
        }

        instance = this;
        hd = Instantiate(hardDataPrefab).GetComponent<HardData>();
    }

    private void Update()
    {
        Game.flags = flags;
    }

    [ExposeMethodInEditor]
    public void LoadDefaultRules()
    {
        var set = new Ruleset();
        sRules.Clear();
        foreach (var rule in set.Keys)
        {
            sRules.Add(
                new SerializableRule()
                {
                    name = rule.ToString(),
                    type = SerializableRule.GetSType(set[rule]),
                    value = set[rule].ToString()
                }
            );
        }
    }
}
