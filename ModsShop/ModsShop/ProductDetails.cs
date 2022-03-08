using System;
using UnityEngine;

namespace ModsShop
{
    public class ProductDetails
    {
        public Sprite productIcon = null;
        public string productName;
        public string productCategory;
       // public string productDescription;
        public float productPrice;
        public bool multiplePurchases = true;
        //public bool spawnInPackage = false;
       // public Vector3 spawnLocation; 
    }
    public class ItemDetails
    {
        public string ModName;
        public string ItemID;
        public string ItemName;
        public float ItemPrice;
        public bool MultiplePurchases = true;
        public Action<Checkout> Checkout = null;

        public ItemDetails(string modName, string itemID, string itemName, float itemPrice, bool multiplePurchases, Action<Checkout> checkout)
        {
            ModName = modName;
            ItemID = itemID;
            ItemName = itemName;
            ItemPrice = itemPrice;
            MultiplePurchases = multiplePurchases; 
            Checkout = checkout;
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
