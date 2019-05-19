using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class RulerCreationWizard : MonoBehaviour
{
    public GameObject row;
    public Transform parentRow;
    public Gradient valueGradient;
    public TextMeshProUGUI rulerPrefix;
    public TMP_InputField rulerFirstName;
    public TMP_InputField rulerFamilyName;
    public TMP_Dropdown raceDropdown;
    public TextMeshProUGUI stockDisplayer;
    public TextMeshProUGUI ageDisplayer;
    public Button increaseAgeButton;
    public Button decreaseAgeButton;

    int stock;
    Ruler ruler;
    Dictionary<string, RulerCreationWizardCharaRow> rows = new Dictionary<string, RulerCreationWizardCharaRow>();

    // Start is called before the first frame update
    void Start()
    {
        ruler = Ruler.CreateRuler();
        stock = ruler.race.rulerCreationRules.stock;
        CreateCharacteristics();
        CreateRaces();
        increaseAgeButton.onClick.AddListener(delegate { IncreaseAge(); });
        decreaseAgeButton.onClick.AddListener(delegate { DecreaseAge(); });
    }
    
    void CreateRaces()
    {
        raceDropdown.ClearOptions();
        var newOptions = new List<TMP_Dropdown.OptionData>();
        foreach (var id in Library.races.Keys) {
            var data = new TMP_Dropdown.OptionData(Library.races[id].name);
            newOptions.Add(data);
        }
        raceDropdown.AddOptions(newOptions);

        raceDropdown.onValueChanged.AddListener((x) => {
            var raceName = raceDropdown.options[x].text;
            foreach(var id in Library.races.Keys)
            {
                var libRace = Library.races[id];
                if (libRace.name == raceName)
                {
                    var newRuler = Ruler.CreateRuler();
                    newRuler.name = ruler.name;
                    ruler = newRuler;
                    UpdateDisplay();
                    return;
                }
            }
        });
    }

    void CreateCharacteristics()
    {
        // Update name
        rulerFirstName.onValueChanged.AddListener((x) => {
            ruler.name.firstName = x;
            UpdateNames();
        });
        rulerFamilyName.onValueChanged.AddListener((x) => {
            ruler.name.lastName = x;
            UpdateNames();
        });

        // Display each characteristic
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
                    if (stock <= 0 || charaLine.characteristic.GetValue() >= charaLine.characteristic.definition.max) return;
                    charaLine.characteristic.Increase(ruler.characteristics);
                    UpdateCharacteristics();
                    stock--;
                    UpdateStockDisplay();
                });
                charaLine.decreaseButton.onClick.AddListener(delegate
                {
                    if (charaLine.characteristic.GetValue() <= charaLine.characteristic.definition.min) return;
                    charaLine.characteristic.Decrease(ruler.characteristics);
                    UpdateCharacteristics();
                    stock++;
                    UpdateStockDisplay();
                });
            }
        
            rows[charaName] = charaLine;

        }
        row.SetActive(false);
        UpdateDisplay();
    }

    int GetMaxStock()
    {
        int maxStock = ruler.race.rulerCreationRules.stock
            + Mathf.FloorToInt((ruler.age - ruler.race.rulerCreationRules.majority) * ruler.race.rulerCreationRules.lifespanToStockRatio);

        return maxStock;
    }

    void UpdateStockDisplay()
    {
        stockDisplayer.text = string.Format("{0} creation points left", stock.ToString());
        var color = valueGradient.Evaluate(1f-stock/(float)ruler.race.rulerCreationRules.stock);
        stockDisplayer.color = color;
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
        UpdateStockDisplay();
        UpdatePersonalDisplay();
        UpdateNames();
    }

    void UpdateNames()
    {
        rulerFirstName.text = ruler.name.firstName;
        rulerFamilyName.text = ruler.name.lastName;
        rulerPrefix.text = ruler.name.GetPreTitle();
    }

    void UpdatePersonalDisplay()
    {
        ageDisplayer.text = ruler.age.ToString();
    }

    void IncreaseAge()
    {
        if (ruler.age >= ruler.race.rulerCreationRules.maxStartingAge) return;
        int previousMaxStock = GetMaxStock();
        ruler.age++;
        int maxStockNow = GetMaxStock();
        if (previousMaxStock != maxStockNow)
        {
            stock += maxStockNow - previousMaxStock;
            return;
        }
        UpdatePersonalDisplay();
        UpdateStockDisplay();
    }

    void DecreaseAge()
    {
        if (ruler.age <= ruler.race.rulerCreationRules.majority) return;
        int previousMaxStock = GetMaxStock();
        ruler.age--;
        int maxStockNow = GetMaxStock();
        if (previousMaxStock != maxStockNow)
        {
            var diff = maxStockNow - previousMaxStock;
            if (stock+ diff < 0)
            {
                ruler.age++;
                return;
            }
            stock += diff;
        }
        UpdatePersonalDisplay();
        UpdateStockDisplay();
    }
    
}
