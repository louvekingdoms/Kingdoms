using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class RulerCreationWizard : MonoBehaviour
{
    public GameObject row;

    Ruler ruler;


    // Start is called before the first frame update
    void Start()
    {
        ruler = new Ruler();

        DisplayCharacteristics();



    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DisplayCharacteristics()
    {
        foreach(var cha in ruler.characteristics.Keys)
        {
            var r = Instantiate(row, transform);
            var txts = GetTexts(r);

            txts[0].text = cha;
            txts[1].text = ruler.characteristics[cha].value.ToString();
        }
    }

    public TextMeshProUGUI[] GetTexts(GameObject row)
    {
        return row.GetComponentsInChildren<TextMeshProUGUI>().ToArray();
    }
}
