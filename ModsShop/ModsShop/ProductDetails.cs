using System;
using UnityEngine;

namespace ModsShop
{

    public class ItemDetails
    {
        public string ModName;
        public string ModID;
        public string ItemID;
        public string ItemName;
        public float ItemPrice;
        public bool MultiplePurchases = true;
        public Action<Checkout> Checkout = null;
        public GameObject ItemPrefab;
        public bool bought = false;
        public ProductOnShelf product;
        public SpawnMethod SpawnMethod;

        public ItemDetails(string modID, string modName, string itemID, string itemName, float itemPrice, bool multiplePurchases, Action<Checkout> checkout, GameObject itemPrefab, SpawnMethod spawnMethod)
        {
            ModID = modID;
            ModName = modName;
            ItemID = itemID;
            ItemName = itemName;
            ItemPrice = itemPrice;
            MultiplePurchases = multiplePurchases;
            Checkout = checkout;
            ItemPrefab = itemPrefab;
            SpawnMethod = spawnMethod;
        }

    }
    
    public class Checkout
    {
        public GameObject gameObject = null;
        public string itemID = string.Empty;
        public Vector3 spawnLocation = Vector3.zero;

        public Checkout(GameObject go, string id, Vector3 spawn)
        {
            gameObject = go;
            itemID = id;
            spawnLocation = spawn;
        }
    }

    //Old
    [Obsolete("Old Shop Catalog is no longer supported", true)]
    public class ProductDetails
    {
        public Sprite productIcon = null;
        public string productName;
        public string productCategory;
        public float productPrice;
        public bool multiplePurchases = true;
    }
    [Obsolete("Old Shop Catalog is no longer supported", true)]
    public class PurchaseInfo
    {
        public GameObject gameObject = null;
        public int qty = 0;
    }
}
