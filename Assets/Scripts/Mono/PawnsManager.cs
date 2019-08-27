using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnsManager : MonoBehaviour
{
    Dictionary<Character, CharacterPawn> displayers = new Dictionary<Character, CharacterPawn>();

    private void Update()
    {
        var rulers = new List<Ruler>();
        foreach (var kingdom in Game.state.world.kingdoms)
        {
            rulers.Add(kingdom.ruler);
        }

        UpdateCharacterPawns(rulers.ToArray());
    }

    void UpdateCharacterPawns(Character[] characters)
    {
        foreach(var chara in characters)
        {
            if (chara.position == null) continue;

            CharacterPawn pawn;
            if (displayers.ContainsKey(chara))
            {
                pawn = displayers[chara];
            }
            else
            {
                var g = Instantiate(GameMaster.hd.characterPawn, transform);
                pawn = g.GetComponent<CharacterPawn>();
                pawn.SetCharacter(chara);
                displayers.Add(chara, pawn);
            }
        }
    }
}
