﻿using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModsShop
{
    public class CashRegister : MonoBehaviour
    {
        public ShoppingCartUI shoppingCartUI;
        public TextMesh display;
        public ShopDudeAnim anims;
        public Transform[] spawnPoint;
        internal Dictionary<ItemDetails, int> shoppingCart = new Dictionary<ItemDetails, int>();
        internal float totalPrice = 0f;
        internal bool uiOpen = false;
#if !Mini
        void Start()
        {
            display.text = Math.Round(totalPrice, 2).ToString("0.00");
        }
        void OnMouseExit()
        {
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
        }
        public byte AddToCart(ItemDetails item)
        {

            if (shoppingCart.ContainsKey(item))
            {
                if (item.MultiplePurchases)
                {
                    if (shoppingCart[item] < 10)
                    {
                        shoppingCart[item] += 1;
                        UpdateCart();
                    }
                    return 0;
                }
                else
                {
                    //not a multibuy item
                    return 1;
                }
            }
            else if (item.bought && !item.MultiplePurchases)
            {                
                //item already bought and not multibuy so can't buy it again using cheating.
                return 2;
            }

            shoppingCart.Add(item, 1);
            UpdateCart();
            return 0;
        }
        internal void UpdateCart(bool aud = true)
        {
            totalPrice = 0;
            foreach (KeyValuePair<ItemDetails, int> cartItems in shoppingCart)
            {
                totalPrice += cartItems.Key.ItemPrice * cartItems.Value;
            }
            display.text = Math.Round(totalPrice, 2).ToString("0.00");
            if (aud)
            {
                anims.CashRegisterAnim();
                MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "cash_register_1");
            }
        }
        internal void PlayCheckoutSound()
        {
            anims.CashRegisterAnim();
            MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "cash_register_2");
        }
        void ShowCart()
        {
            uiOpen = true;
            PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = true; //unlock mouse
            GameObject.Find("Systems").transform.GetChild(7).gameObject.SetActive(true); //can't clickthrough UI when menu is active.
            if (shoppingCart.Count == 0)
            {
                MsgBoxBtn btn = ModUI.CreateMessageBoxBtn("Got it", delegate {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = false; 
                    GameObject.Find("Systems").transform.GetChild(7).gameObject.SetActive(false);
                    StartCoroutine(DelayMenu());
                });
                ModUI.ShowCustomMessage($"Your shopping cart is empty{Environment.NewLine}To buy stuff just turn around and look at the store shelf to add stuff to your cart.", "Shopping cart", new MsgBoxBtn[] { btn });
                return;
            }
            shoppingCartUI.ui.SetActive(true);
            shoppingCartUI.PopulateCart();
        }
        IEnumerator DelayMenu()
        {
            yield return new WaitForSeconds(1f);
            uiOpen = false;
        }
        void Update()
        {
            if (ModsShop.mainCam == null) return;
            if (uiOpen) return;
            if (Physics.Raycast(ModsShop.mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1f))
            {
                if(hit.transform.gameObject == gameObject)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = $"Preview items";

                    if (Input.GetMouseButtonDown(0))
                    {
                        ShowCart();
                    }
                }
            }
        }
        #endif
    }
}