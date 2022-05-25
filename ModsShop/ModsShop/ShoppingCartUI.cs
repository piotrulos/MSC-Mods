using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModsShop
{
    public class ShoppingCartUI : MonoBehaviour
    {
        public CashRegister cashRegister;
        public GameObject cartItem;
        public GameObject listView;
        public Text priceText;
        public GameObject ui;

        public void RemoveChildren(Transform parent) 
        {
            foreach (Transform child in parent)
                Destroy(child.gameObject);
        }
  
        void Start()
        {
            transform.SetParent(null, false);
        }
        public void PurchaseBtn()
        {
#if !Mini 
            if (cashRegister.shoppingCart.Count == 0)
            {
                ModUI.ShowMessage("Your shopping cart is empty", "Shopping cart");
                return;
            }
            if (cashRegister.totalPrice > Math.Round(PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value, 1))
            {
                ModUI.ShowMessage("You are too poor for this order!", "Poor man");
                return;
            }
            StartCoroutine(SpawnStuff());
            cashRegister.PlayCheckoutSound();
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = false; //unlock mouse
            GameObject.Find("Systems").transform.GetChild(7).gameObject.SetActive(false); //can't clickthrough UI when menu is active.
            ui.SetActive(false);
#endif
        }

        public void CancelBtn()
        {            
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = false; //unlock mouse
            GameObject.Find("Systems").transform.GetChild(7).gameObject.SetActive(false); //can't clickthrough UI when menu is active.
            ui.SetActive(false);
        }
#if !Mini
        IEnumerator SpawnStuff()
        {
            foreach (KeyValuePair<ItemDetails, int> cartItems in cashRegister.shoppingCart)
            {
                GameObject spawnedObj = null;
                switch (cartItems.Key.SpawnMethod)
                {
                    case SpawnMethod.Instantiate:
                        if (cartItems.Value > 1)
                        {
                            for (int i = 0; i < cartItems.Value; i++)
                            {
                                spawnedObj = Instantiate(cartItems.Key.ItemPrefab);
                                CheckoutCallback(spawnedObj, cartItems.Key);
                                spawnedObj.transform.position = cashRegister.spawnPoint.position;
                                yield return new WaitForSeconds(.2f);
                            }
                        }
                        else
                        {
                            spawnedObj = Instantiate(cartItems.Key.ItemPrefab);
                            CheckoutCallback(spawnedObj, cartItems.Key);
                        }
                        break;
                    case SpawnMethod.SetActive:
                        spawnedObj = cartItems.Key.ItemPrefab.gameObject;
                        spawnedObj.SetActive(true);
                        CheckoutCallback(spawnedObj, cartItems.Key);
                        spawnedObj.transform.position = cashRegister.spawnPoint.position;
                        break;
                    case SpawnMethod.Custom:
                        spawnedObj = null;
                        CheckoutCallback(spawnedObj, cartItems.Key);
                        break;
                }
                spawnedObj.transform.position = cashRegister.spawnPoint.position;
                yield return new WaitForSeconds(.2f);
            }
            cashRegister.shoppingCart.Clear();
            PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value -= cashRegister.totalPrice;
            cashRegister.UpdateCart(false);
        }

        void CheckoutCallback(GameObject spawned, ItemDetails item)
        {
            try
            {
                item.Checkout(new Checkout(spawned, item.ItemID, cashRegister.spawnPoint.position));
                item.bought = true;
            }
            catch (Exception e)
            {
                ModConsole.Error($"Fatal error: in mod {item.ModID} item's {item.ItemID} checkout action.");
                ModConsole.Error($"{e.Message}");
                Console.WriteLine(e);
            }
        }
        internal void PopulateCart()
        {
            UpdateCart(false);
        }

        internal void MoreLess(ItemDetails item, bool more)
        {
            if (cashRegister.shoppingCart.ContainsKey(item))
            {
                if (more)
                {
                    if (item.MultiplePurchases)
                    {
                        if (cashRegister.shoppingCart[item] < 10)
                            cashRegister.shoppingCart[item] += 1;
                        else return;
                    }
                }
                else
                {
                    if (cashRegister.shoppingCart[item] > 1)
                        cashRegister.shoppingCart[item] -= 1;
                    else return;
                }
                UpdateCart(true);
            }
        }

        private void UpdateCart(bool aud)
        {
            RemoveChildren(listView.transform);
            foreach (KeyValuePair<ItemDetails, int> cartItems in cashRegister.shoppingCart)
            {
                GameObject itm = Instantiate(cartItem);
                itm.GetComponent<ShoppingCartUIItem>().SetupElement(this, cartItems);
                itm.transform.SetParent(listView.transform, false);
            }
            cashRegister.UpdateCart(aud);
            priceText.text = $"Total: {cashRegister.totalPrice} MK";

        }

        internal void RemoveItem(ItemDetails det)
        {
            if(cashRegister.shoppingCart.ContainsKey(det))
               cashRegister.shoppingCart.Remove(det);
            det.product.Cancel();
            UpdateCart(true);
        }
#endif
    }
}