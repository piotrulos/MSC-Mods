#if !Mini 
using MSCLoader; 
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsShop
{
    [Obsolete("Old Shop Catalog is no longer supported", true)]
    public enum ShopType
    {
        Teimo,
        Fleetari        
    }
    [Obsolete("Old Shop Catalog is no longer supported", true)]
    public static class TeimoSpawnLocation
    {
        public static Vector3 desk = new Vector3(-1551.11f, 5f, 1182.77f);
        public static Vector3 floor = new Vector3(-1551f, 5f, 1182f);
        public static Vector3 outsideRamp = new Vector3(-1553.83f, 5f, 1182.74f);

    }
    [Obsolete("Old Shop Catalog is no longer supported", true)]
    public static class FleetariSpawnLocation
    {
        public static Vector3 outside = new Vector3(1549.38f, 5f, 728.47f);
        public static Vector3 desk = new Vector3(1553.83f, 6f, 740.17f);
        public static Vector3 floor = new Vector3(1553.42f, 6f, 739.31f);
    }
    [Obsolete("Old Shop Catalog is no longer supported", true)]
    public class ShopItems
    {
#if !Mini
        public Mod mod;
#endif
        public ProductDetails details;
        public Vector3 spawnLocation;
        public GameObject gameObject;
        public Action<PurchaseInfo> action;
        public bool purchashed = false;
    }

    [Obsolete("ShopItem is no longer supported. Use Shop.CreateShopItem() instead", true)]
    public class ShopItem : MonoBehaviour
    {
        internal GameObject legacyDisplay;
        public List<ShopItems> teimoShopItems = new List<ShopItems>();
        public List<ShopItems> fleetariShopItems = new List<ShopItems>();
        public Dictionary<ShopItems, int> shoppingCart = new Dictionary<ShopItems, int>();
        public GameObject modPref, catPref, itemPref, cartItemPref;
        public Collider teimoCatalog;
        public Collider fleetariCatalog;
        public GameObject shopCatalogUI;

#if !Mini
        [Obsolete("ShopItem is no longer supported. Use Shop.CreateShopItem() instead", true)]
        public void Add(Mod mod, ProductDetails product, ShopType shopType, Action<PurchaseInfo> action, GameObject go)
        {
            Shop s = ModsShop.GetShopReference();
            ItemDetails lp = s.CreateShopItem(mod, $"{mod.ID}_{product.productName}", $"[LEGACY ITEM] {product.productName}", product.productPrice, product.multiplePurchases, delegate (Checkout c)
            {
                PurchaseInfo info = new PurchaseInfo
                {
                    gameObject = c.gameObject,
                    qty = 1
                };
                TeimoSpawnLocation.desk = c.spawnLocation;
                TeimoSpawnLocation.outsideRamp = c.spawnLocation;
                TeimoSpawnLocation.floor = c.spawnLocation;
                FleetariSpawnLocation.outside = c.spawnLocation;
                FleetariSpawnLocation.desk = c.spawnLocation;
                FleetariSpawnLocation.floor = c.spawnLocation;
                if (action != null) action(info);
            }, go, SpawnMethod.Custom);
            s.AddDisplayItem(lp, legacyDisplay, SpawnMethod.Instantiate);
        }
#endif
    }
}
