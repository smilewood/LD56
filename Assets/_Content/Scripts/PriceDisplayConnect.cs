using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PriceDisplayConnect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int Biomass;
    public int Ore;
    public int Bread;
    public PriceDisplay priceDisplay;

    public void OnPointerEnter(PointerEventData eventData)
    {
        priceDisplay.SetPrices(Biomass, Ore, Bread);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        priceDisplay.ClearPrices();
    }
}
