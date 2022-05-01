#if !Mini
using MSCLoader;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsShop
{
    public enum ObjectType
    {
        Instantiated,
        Prefab
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
        public ItemDetails CreateShopItem(Mod mod, string itemID, string itemName, float itemPrice, bool multiplePurchases, Action<Checkout> purchashedAction, GameObject itemObject, ObjectType objectType)
        {
            ItemDetails itemDetails = new ItemDetails(mod.Name, $"{mod.ID}_{itemID}", itemName, itemPrice, multiplePurchases, purchashedAction, itemObject, objectType);
            items.Add(itemDetails);
            return itemDetails;
        }

        public void AddDisplayItem(ItemDetails itemDetails, GameObject displayObject, ObjectType displayObjectType, Vector3 rotation, int gap)
        {
            GameObject go = null;
            switch (displayObjectType)
            {
                case ObjectType.Instantiated:
                    go = displayObject;
                    go.SetActive(true);
                    break;
                case ObjectType.Prefab:
                    go = Instantiate(displayObject);
                    break;
            }
            if(go == null)
            {
                ModConsole.Error("[ModsShop] AddDisplayItem() - displayObject is null");
                return;
            }
            go.AddComponent<ProductOnShelf>().ItemDetails = itemDetails;
            go.transform.eulerAngles = rotation;
            shopRefs.autoShelves.SpawnItem(itemDetails.ModName, go, gap, 0);
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
#endif
    }
}