using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeInfoPopup : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _biomass;
    [SerializeField] private TMPro.TextMeshProUGUI _ore;
    [SerializeField] private TMPro.TextMeshProUGUI _bread;

    private void Update()
    {
        _biomass.text = EconomyManager.Instance.Balance.Biomass.ToString();
        _ore.text = EconomyManager.Instance.Balance.Ore.ToString();
        _bread.text = EconomyManager.Instance.Balance.Bread.ToString();
    }

}
