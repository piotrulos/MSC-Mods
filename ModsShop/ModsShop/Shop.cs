#if !Mini
using MSCLoader;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsShop
{
    public enum SpawnMethod
    {
        Instantiate,
        SetActive,
        Custom
    }
    public class Shop : MonoBehaviour
    {
        public ShopRefs shopRefs;
        public List<ItemDetails> items;

        void Awake()
        {
            items = new List<ItemDetails>();
            
        }
#if !Mini
        public ItemDetails CreateShopItem(Mod mod, string itemID, string itemName, float itemPrice, bool multiplePurchases, Action<Checkout> purchashedAction) => CreateShopItem(mod, itemID, itemName, itemPrice, multiplePurchases, purchashedAction, null, SpawnMethod.Custom);
        public ItemDetails CreateShopItem(Mod mod, string itemID, string itemName, float itemPrice, bool multiplePurchases, Action<Checkout> purchashedAction, GameObject itemObject, SpawnMethod spawnMehod)
        {
            switch (spawnMehod)
            {
                case SpawnMethod.SetActive:
                    if(multiplePurchases) ModConsole.Error("[ModsShop] CreateShopItem() - SetActive cannot be used for items that can be purchased multiple times.");
                    break;
                case SpawnMethod.Instantiate:
                    break;
                case SpawnMethod.Custom:
                    break;

            }
            ItemDetails itemDetails = new ItemDetails(mod.ID, mod.Name, $"{itemID}", itemName, itemPrice, multiplePurchases, purchashedAction, itemObject, spawnMehod);
            items.Add(itemDetails);
            return itemDetails;
        }

        public void AddDisplayItem(ItemDetails itemDetails, GameObject displayObject, SpawnMethod displayObjectSpawnMethod, Vector3 rotation, int gap)
        {
            GameObject go = null;
            switch (displayObjectSpawnMethod)
            {
                case SpawnMethod.SetActive:
                    go = displayObject;
                    go.SetActive(true);
                    break;
                case SpawnMethod.Instantiate:
                    go = Instantiate(displayObject);
                    break;
                case SpawnMethod.Custom:
                    ModConsole.Error("Custom spawn method doesn't apply to display objects.");
                    break;

            }
            if (go == null)
            {
                ModConsole.Error("[ModsShop] AddDisplayItem() - displayObject is null, make sure you added displayObject and it has correct SpawnMethod");
                return;
            }
            go.AddComponent<ProductOnShelf>().itemDetails = itemDetails;
            go.transform.eulerAngles = rotation;
            shopRefs.autoShelves.SpawnItem(itemDetails.ModID, go, gap, 0);
        }

        public void AddCustomShelf(GameObject customShelfPrefab)
        {
            GameObject go = Instantiate(customShelfPrefab);
            shopRefs.customShelves.InsertSlelf(go);
        }

        public ItemDetails GetItemDetailsByID(string modID, string ItemID)
        {
            if(items != null)
            {
                ItemDetails item = items.Where(x => x.ItemID == ItemID && x.ModID == modID).FirstOrDefault();
                if(item != null)
                    return item;
            }
            return null;
        }
#endif
    }
}