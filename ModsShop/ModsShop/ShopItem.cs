using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModsShop
{
    public enum ShopType
    {
        Teimo,
        Fleetari        
    }
    public static class TeimoSpawnLocation
    {
        public static readonly Vector3 desk = new Vector3(-1551.11f, 5f, 1182.77f);
        public static readonly Vector3 floor = new Vector3(-1551f, 5f, 1182f);
        public static readonly Vector3 outsideRamp = new Vector3(-1553.83f, 5f, 1182.74f);

    }
    public class ShopItems
    {
        public Mod mod;
        public ProductDetails details;
        public Vector3 spawnLocation;
        public GameObject gameObject;
        public Action<PurchaseInfo> action;
        public bool purchashed = false;
    }
    public class ShopItem : MonoBehaviour
    {
        public List<ShopItems> teimoShopItems = new List<ShopItems>();
        public List<ShopItems> fleetariShopItems = new List<ShopItems>();
        public Dictionary<ShopItems, int> shoppingCart = new Dictionary<ShopItems, int>();
        public GameObject modPref, catPref, itemPref, cartItemPref;
        public Collider teimoCatalog;
        public GameObject shopCatalogUI;
        GameObject leftListView;
        GameObject cartListView;
        Text selectSth;
        float totalPrice = 0;
        AudioSource fo;
        public void RemoveChildren(Transform parent) //clear 
        {
            foreach (Transform child in parent)
                Destroy(child.gameObject);
        }

        public void Prepare()
        {
            shopCatalogUI.transform.GetChild(1).GetChild(0).GetComponent<Button>().onClick.AddListener(() => OpenCatalog());
            leftListView = shopCatalogUI.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject;
            selectSth = shopCatalogUI.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
            cartListView = shopCatalogUI.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(0).gameObject;
            shopCatalogUI.transform.GetChild(0).GetChild(1).GetChild(2).GetChild(2).GetComponent<Button>().onClick.AddListener(() => FinishOrder());
            fo = GameObject.Find("STORE/StoreCashRegister").AddComponent<AudioSource>();
            fo.clip = GameObject.Find("MasterAudio/Store/cash_register_2").GetComponent<AudioSource>().clip;

        }

        public void Add(Mod mod, ProductDetails product, ShopType shopType, GameObject productObject)
        {
            throw new NotImplementedException("Not implemented - yet");
        }
        public void Add(Mod mod, ProductDetails product, ShopType shopType, Action<PurchaseInfo> action, GameObject go)
        {
            ShopItems item = new ShopItems
            {
                mod = mod,
                details = product,
                action = action,
                gameObject = go
            };
            if (shopType == ShopType.Teimo)
                teimoShopItems.Add(item);
            else
                fleetariShopItems.Add(item);
        }

        public void ShowCatalog()
        {
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = true; //unlock mouse
            GameObject.Find("Systems").transform.GetChild(7).gameObject.SetActive(true); //can't clickthrough UI when menu is active.
            shopCatalogUI.transform.GetChild(0).gameObject.SetActive(false);
            shopCatalogUI.transform.GetChild(1).gameObject.SetActive(true);
            shopCatalogUI.SetActive(true);
        }

        public void HideCatalog()
        {
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = false; //unlock mouse
            GameObject.Find("Systems").transform.GetChild(7).gameObject.SetActive(false); //can't clickthrough UI when menu is active.
            shopCatalogUI.SetActive(false);
        }

        public void OpenCatalog()
        {
            shopCatalogUI.transform.GetChild(0).gameObject.SetActive(true);
            shopCatalogUI.transform.GetChild(1).gameObject.SetActive(false);
            shopCatalogUI.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = string.Format("Items in cart: {0}", shoppingCart.Count);
            shopCatalogUI.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<Text>().text = string.Format("Money: {0} MK", Math.Round(PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value,1));
            ListOfMods();
        }
        public void AddToCart(ShopItems item)
        {
            if (shoppingCart.ContainsKey(item))
                if (item.details.multiplePurchases)
                    shoppingCart[item] += 1;
                else
                    ModUI.ShowMessage("This item can be bought only once", "Information");
            else if (item.purchashed)
                ModUI.ShowMessage("You already bought this item", "Information");
            else
                shoppingCart.Add(item, 1);
            UpdateCart();
        }
        void UpdateCart()
        {
            RemoveChildren(cartListView.transform);
            shopCatalogUI.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = string.Format("Items in cart: {0}", shoppingCart.Count);
            totalPrice = 0;
            foreach (var cartItems in shoppingCart)
            {
                GameObject cartItem = Instantiate(cartItemPref);
                cartItem.transform.GetChild(0).GetComponent<Text>().text = cartItems.Value + "x " + cartItems.Key.details.productName;
                cartItem.transform.GetChild(1).GetComponent<Text>().text = cartItems.Key.details.productPrice * cartItems.Value + " MK"; //multiply price test
                cartItem.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { shoppingCart.Remove(cartItems.Key); UpdateCart(); });

                cartItem.transform.SetParent(cartListView.transform, false);
                totalPrice += cartItems.Key.details.productPrice * cartItems.Value;
            }
            shopCatalogUI.transform.GetChild(0).GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = string.Format("Total price: {0} MK", Math.Round(totalPrice, 2));

        }

        private void FinishOrder()
        {
            if (shoppingCart.Count == 0)
            {
                ModUI.ShowMessage("Your shopping cart is empty", "Shopping cart");

            }
            else if (totalPrice > Math.Round(PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value, 1))
            {
                ModUI.ShowMessage("You are too poor for this order!", "Poor man");
            }
            else 
            {
                foreach (var cartItems in shoppingCart)
                {
                    PurchaseInfo pi = new PurchaseInfo
                    {
                        gameObject = cartItems.Key.gameObject,
                        qty = cartItems.Value
                    };
                    cartItems.Key.action(pi);
                    cartItems.Key.purchashed = true;
                }
                shoppingCart.Clear();
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value -= totalPrice;
                UpdateCart();
                HideCatalog();
                fo.Play();
            }

        }

        public void ListOfMods()
        {
            RemoveChildren(leftListView.transform);
            selectSth.text = "Select Mod:";
            Text numOfproductsText = null;
            int num = 1;
            Mod lastmod =null;
            foreach(Mod mods in teimoShopItems.Select(s => s.mod))
            {
                if (lastmod == mods)
                {
                    num++;
                    numOfproductsText.text = string.Format("Available products: {0}", num);
                }
                else
                {
                    num = 1;
                    GameObject mod = Instantiate(modPref);
                    mod.GetComponent<Button>().onClick.AddListener(() => ListOfCats(mods));
                    mod.transform.GetChild(0).GetComponent<Text>().text = mods.Name;
                    numOfproductsText = mod.transform.GetChild(1).GetComponent<Text>();
                    numOfproductsText.text = "Available products: 1";
                    lastmod = mods;
                    mod.transform.SetParent(leftListView.transform, false);
                }
            }
        }
        public void ListOfCats(Mod mod)
        {
            RemoveChildren(leftListView.transform);
            selectSth.text = "Select Product:";

            foreach (ShopItems shopItem in teimoShopItems.Select(s => s).Where(x => x.mod == mod))
            {
                GameObject item = Instantiate(itemPref);
                item.transform.GetChild(0).GetComponent<Image>().sprite = shopItem.details.productIcon;
                item.transform.GetChild(1).GetComponent<Text>().text = shopItem.details.productName;
                item.transform.GetChild(3).GetComponent<Text>().text = string.Format("{0} MK",shopItem.details.productPrice);
                item.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => AddToCart(shopItem));

                item.transform.SetParent(leftListView.transform, false);
            }
            //ModConsole.Print("Sas " + mod.Name);
        }
        private void Update()
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, 1f);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == teimoCatalog)
                    {
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Open shop catalog";
                        if (Input.GetMouseButtonDown(0))
                        {
                            ShowCatalog();
                        }
                        break;
                    }
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    HideCatalog();
                }
            }
        }

    }
}
