using System;
using UnityEngine;

namespace ModsShop
{
    public class ProductDetails
    {
        public Sprite productIcon = null;
        public string productName;
        public string productCategory;
        public float productPrice;
        public bool multiplePurchases = true;
    }

    public class ItemDetails
    {
        public string ModName;
        public string ItemID;
        public string ItemName;
        public float ItemPrice;
        public bool MultiplePurchases = true;
        public Action<Checkout> Checkout = null;
        public GameObject ItemPrefab;

        public ItemDetails(string modName, string itemID, string itemName, float itemPrice, bool multiplePurchases, Action<Checkout> checkout, GameObject itemPrefab)
        {
            ModName = modName;
            ItemID = itemID;
            ItemName = itemName;
            ItemPrice = itemPrice;
            MultiplePurchases = multiplePurchases; 
            Checkout = checkout;
            ItemPrefab = itemPrefab;
        }

    }
    
    public class Checkout
    {
        public GameObject gameObject = null;
    }

    public class PurchaseInfo
    {
        public GameObject gameObject = null;
        public int qty = 0;
    }
}
