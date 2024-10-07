using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriceDisplay : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _biomassText;
    [SerializeField] private TMPro.TextMeshProUGUI _oreText;
    [SerializeField] private TMPro.TextMeshProUGUI _breadText;

    public void OnEnable()
    {
        ClearPrices();
    }

    public void SetPrices(int biomass, int ore, int bread)
    {
        _biomassText.text = biomass.ToString();
        _oreText.text = ore.ToString();
        _breadText.text = bread.ToString();
    }

    public void ClearPrices()
    {
        _biomassText.text = "";
        _oreText.text = "";
        _breadText.text = "";
    }
}
