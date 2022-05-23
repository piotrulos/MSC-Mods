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

        public void RemoveChildren(Transform parent) 
        {
            foreach (Transform child in parent)
                Destroy(child.gameObject);
        }
        void Start()
        {
            transform.parent.parent = null;
        }
        public void PurchaseBtn()
        {
#if !Mini

#endif 
        }

        public void CancelBtn()
        {
            gameObject.SetActive(false);
        }
#if !Mini
        internal void PopulateCart()
        {
            UpdateCart();
        }

        internal void MoreLess(ItemDetails item, bool more)
        {
            if (cashRegister.shoppingCart.ContainsKey(item))
            {
                if (more)
                {
                    if (item.MultiplePurchases)
                    {
                        if(cashRegister.shoppingCart[item] < 10)
                        cashRegister.shoppingCart[item] += 1;
                    }
                }
                else
                {
                    if(cashRegister.shoppingCart[item] > 1)
                        cashRegister.shoppingCart[item] -= 1;
                }
                UpdateCart();
            }
        }

        private void UpdateCart()
        {
            RemoveChildren(listView.transform);
            foreach (KeyValuePair<ItemDetails, int> cartItems in cashRegister.shoppingCart)
            {
                GameObject itm = Instantiate(cartItem);
                itm.GetComponent<ShoppingCartUIItem>().SetupElement(this, cartItems);
                itm.transform.SetParent(listView.transform, false);
            }
            cashRegister.UpdateCart();
            priceText.text = $"Total: {cashRegister.totalPrice} MK";

        }

        internal void RemoveItem(ItemDetails det)
        {
            if(cashRegister.shoppingCart.ContainsKey(det))
               cashRegister.shoppingCart.Remove(det);
            det.product.Cancel();
            UpdateCart();
        }
#endif
    }
}