using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BuildingType
{ 
   Source_Food_FruitTree,
   Source_Water_Well,
   Producer_Biomass_Composter,
   Producer_Ore_Mine,
   Producer_Bread_BreadFoundry,
   Omnicore
}

[Serializable]
public class CurrencyData
{
   public int Biomass;
   public int Ore;
   public int Bread;
}

[Serializable]
public sealed class BuildingMetadata : CurrencyData
{
   public GameObject Prefab;
   public BuildingType BuildingType;
}

/// <summary>
/// Manages the player's resources and currencies for purchasing upgrades and buildings
/// </summary>
public class EconomyManager : MonoBehaviour
{
   public CurrencyData Balance;

   [SerializeField]
   private List<BuildingMetadata> buildingMetadataList = new List<BuildingMetadata>();

   /// <summary>
   /// Checks if the cost of an object is less than the player's balance
   /// </summary>
   /// <param name="purchase">Cost data of the item</param>
   /// <returns>True if player can afford</returns>
   public bool CanPurchase(CurrencyData purchase)
   {
      return purchase.Biomass <= Balance.Biomass &&
         purchase.Ore <= Balance.Ore &&
         purchase.Bread <= Balance.Bread;
   }

   /// <summary>
   /// Attempts to purchase the supplied item
   /// </summary>
   /// <param name="purchase">Cost data of the item</param>
   /// <returns>True if the purchase is successful</returns>
   public bool TryPurchase(CurrencyData purchase)
   {
      if (purchase.Biomass <= Balance.Biomass &&
         purchase.Ore <= Balance.Ore &&
         purchase.Bread <= Balance.Bread)
      {
         Balance.Biomass -= purchase.Biomass;
         Balance.Ore -= purchase.Ore;
         Balance.Bread -= purchase.Bread;

         return true;
      }

      return false;
   }

   /// <summary>
   /// Gets the building data for the specified type
   /// </summary>
   /// <param name="buildingType">What kind of building to retrieve data for</param>
   /// <returns>Building metadata with game object and cost data</returns>
   /// <exception cref="ArgumentException">Thrown if the building does not exist</exception>
   public BuildingMetadata GetBuildingMetadata(BuildingType buildingType)
   {
      if (buildingMetadataList.Any(b => b.BuildingType == buildingType))
      {
         return buildingMetadataList.Find(b => b.BuildingType == buildingType);
      }

      throw new ArgumentException("No building of type " + buildingType.ToString() + "exists");
   }
}
