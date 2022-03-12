using MSCLoader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsShop
{
    public class CashRegister : MonoBehaviour
    {
        public TextMesh display;
        internal Dictionary<ItemDetails, int> shoppingCart = new Dictionary<ItemDetails, int>();
        private float totalPrice = 0f;

        void Start()
        {
            display.text = Math.Round(totalPrice, 2).ToString("0.00");
        }
        void OnMouseExit()
        {
#if !Mini
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
#endif
        }
        public byte AddToCart(ItemDetails item)
        {
            if (shoppingCart.ContainsKey(item))
            {
                if (item.MultiplePurchases)
                {
                    shoppingCart[item] += 1;
                    UpdateCart();
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else if (item.bought)
            {
                return 2;
            }

            shoppingCart.Add(item, 1);
            UpdateCart();
            return 0;
        }
        void UpdateCart()
        {
            totalPrice = 0;
            foreach (KeyValuePair<ItemDetails, int> cartItems in shoppingCart)
            {
                totalPrice += cartItems.Key.ItemPrice * cartItems.Value;
            }
            display.text = Math.Round(totalPrice, 2).ToString("0.00");
        }

        void ShowCart()
        {
            ModConsole.Warning(shoppingCart.Count.ToString());
        }

        void Update()
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1f))
            {
                if(hit.transform.gameObject == gameObject)
                {
#if !Mini
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = $"Preview items";
#endif
                    if (Input.GetMouseButtonDown(0))
                    {
                        ShowCart();
                    }
                }
            }
        }
    }
}