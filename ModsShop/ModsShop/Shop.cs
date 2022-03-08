#if !Mini
using MSCLoader;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsShop
{
    public class Shop : MonoBehaviour
    {
        public ShopRefs shopRefs;
        public List<ItemDetails> items;

        void Awake()
        {
            items = new List<ItemDetails>();
        }
#if !Mini
        public ItemDetails CreateShopItem(Mod mod, string itemID, string itemName, float itemPrice, bool multiplePurchases, Action<Checkout> purchashedAction, GameObject itemPrefab)
        {
            ItemDetails itemDetails = new ItemDetails(mod.Name, $"{mod.ID}_{itemID}", itemName, itemPrice, multiplePurchases, purchashedAction, itemPrefab);
            items.Add(itemDetails);
            return itemDetails;
        }
#endif
        public void AddDisplayItem(ItemDetails shopItem, GameObject displayItem)
        {
            GameObject go = Instantiate(displayItem);
            go.AddComponent<ProductOnShelf>().ItemDetails = shopItem;
            shopRefs.autoShelves.SpawnItem(go, 1, 0);
        }

        public void AddCustomShelf(GameObject customShelfPrefab)
        {
            GameObject go = Instantiate(customShelfPrefab);
            shopRefs.customShelves.InsertSlelf(go);
        }

        public ItemDetails GetItemDetailsByID(string ItemID)
        {
            if(items != null)
            {
                ItemDetails item = items.Where(x => x.ItemID == ItemID).FirstOrDefault();
                if(item != null)
                    return item;
            }
            return null;
        }

    }
}