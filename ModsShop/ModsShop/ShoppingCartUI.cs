using MSCLoader;
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

        void Start()
        {
            transform.parent.parent = null;
        }
        public void PurchaseBtn()
        {

        }

        public void CancelBtn()
        {
            gameObject.SetActive(false);
        }
    }
}