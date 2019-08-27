using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPawn : MonoBehaviour
{
    public Character character;

    public UnityEngine.UI.Image image;

    RectTransform rect;
    MapDisplayer mapDisplayer;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        mapDisplayer = GameMaster.instance.mapDisplayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (character.position != null)
        {
            rect.anchoredPosition = mapDisplayer.GetRegionPawnPosition(character.position);
        }
    }

    public void SetCharacter(Character chara)
    {
        character = chara;
        image.sprite = character.pawn.GetSprite();
    }
}
