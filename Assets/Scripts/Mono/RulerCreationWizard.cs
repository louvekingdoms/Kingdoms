using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class RulerCreationWizard : MonoBehaviour
{
    public GameObject row;
    public Transform parentRow;
    public Gradient valueGradient;
    public TextMeshProUGUI rulerName;

    Ruler ruler;
    Dictionary<string, RulerCreationWizardCharaRow> rows = new Dictionary<string, RulerCreationWizardCharaRow>();

    // Start is called before the first frame update
    void Start()
    {
        ruler = new Ruler();

        CreateCharacteristics();

    }
    

    void CreateCharacteristics()
    {
        foreach (var charaName in ruler.characteristics.Keys)
        {
            var r = Instantiate(row, parentRow);
            var charaLine = r.GetComponent<RulerCreationWizardCharaRow>();
            charaLine.characteristic = ruler.characteristics[charaName];

            if (charaLine.characteristic.definition.rules.isFrozen)
            {
                Destroy(charaLine.increaseButton.gameObject);
                Destroy(charaLine.decreaseButton.gameObject);
            }
            else
            {
                charaLine.increaseButton.onClick.AddListener(delegate
                {
                    charaLine.characteristic.Increase(ruler.characteristics);
                    UpdateCharacteristics();
                });
                charaLine.decreaseButton.onClick.AddListener(delegate
                {
                    charaLine.characteristic.Decrease(ruler.characteristics);
                    UpdateCharacteristics();
                });
            }
        
            rows[charaName] = charaLine;

        }
        Destroy(row);
        UpdateDisplay();
    }

    void UpdateCharacteristics()
    {
        foreach (var charaName in ruler.characteristics.Keys)
        {
            var characteristic = ruler.characteristics[charaName];
            var charaLine = rows[charaName];

            charaLine.nameText.text = charaName;
            charaLine.valueText.text = ruler.characteristics[charaName].GetClampedValue().ToString();

            // Colors
            var period = (float)(characteristic.GetClampedValue() - characteristic.definition.min) / (characteristic.definition.max/2 - characteristic.definition.min);
            if (characteristic.definition.rules.isBad)
            {
                period = 1f - period;
            }
            var color = valueGradient.Evaluate(period);

            charaLine.valueText.color = color;
        }
    }

    void UpdateDisplay()
    {
        UpdateCharacteristics();
        rulerName.text = ruler.name;
    }
    
}
