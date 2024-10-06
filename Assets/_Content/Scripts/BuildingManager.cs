using UnityEngine;

public class BuildingManager : MonoBehaviour
{
   public Collider Collider;
   public string Name;
   public string Description;
   public BuildingType BuildingType;
   public int Capacity;
   public float OperationRate;

   public bool IsCapacityUpgraded = false;
   public int CapacityUpgradeAmount = 50;
   public CurrencyData CapacityUpgradeCost;

   public bool IsOperationRateUpgraded = false;
   public int OperationRateUpgradeAmount = 1;
   public CurrencyData OperationRateUpgradeCost;

   private EconomyManager EconomyManager;

   void Start()
   {
        EconomyManager = EconomyManager.Instance;
   }

   /// <summary>
   /// See if the capacity can be upgraded
   /// </summary>
   public bool CanUpgradeCapacity()
   {
      return !IsCapacityUpgraded && EconomyManager.CanPurchase(CapacityUpgradeCost);
   }

   /// <summary>
   /// Attempt to purchase the capacity upgrade
   /// </summary>
   /// <returns>True if the capacity was upgraded</returns>
   public bool TryUpgradeCapacity()
   {
      if(!IsCapacityUpgraded && EconomyManager.TryPurchase(CapacityUpgradeCost))
      {
         Capacity += CapacityUpgradeAmount;
         IsCapacityUpgraded = true;
         return true;
      }

      return false;
   }

   /// <summary>
   /// See if the operation rate can be upgraded
   /// </summary>
   public bool CanUpgradeOperationRate()
   {
      return !IsOperationRateUpgraded && EconomyManager.CanPurchase(OperationRateUpgradeCost);
   }

   /// <summary>
   /// Attempt to purchase the operation rate upgrade
   /// </summary>
   /// <returns>True if the operation rate was upgraded</returns>
   public bool TryUpgradeOperationRate()
   {
      if(!IsOperationRateUpgraded && EconomyManager.TryPurchase(OperationRateUpgradeCost))
      {
         OperationRate += OperationRateUpgradeAmount;
         IsOperationRateUpgraded = true;
         return true;
      }

      return false;
   }

   void Update()
   {
      // eat pie
   }
}
