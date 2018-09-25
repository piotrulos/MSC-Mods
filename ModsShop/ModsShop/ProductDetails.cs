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
    public class PurchaseInfo
    {
        public GameObject gameObject = null;
        public int qty = 0;
    }
}
