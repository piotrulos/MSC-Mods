using MSCLoader;
using UnityEngine;

namespace ModsShop
{
    public class ProductOnShelf : MonoBehaviour
    {
        [Header("Your mod ID (case sensitive)")]
        public string ModID;
        [Header("Your item ID (case sensitive)")]
        public string ItemID;

        [HideInInspector]
        public Shop shop;
        [HideInInspector]
        public ItemDetails itemDetails;
#if !Mini
        void Awake()
        {
            shop = ModsShop.GetShopReference();
        }

        void Start()
        {
            if (itemDetails == null)
            {
                itemDetails = shop.GetItemDetailsByID(ModID, ItemID);
                if (itemDetails == null)
                {
                    ModConsole.Error($"Shop: Shop itemID <b>{ItemID}</b> not found in mod <b>{ModID}</b>");
                    return;
                }
            }
            itemDetails.product = this;
        }

        void OnMouseExit()
        {
            if (itemDetails == null) return;
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
        }
        internal void Cancel()
        {
            if (!itemDetails.MultiplePurchases)
            {
                gameObject.SetActive(true);
            }
        }
        void Update()
        {
            if (itemDetails == null) return;
            if (shop.shopRefs.hitted)
            {
                if (shop.shopRefs.hit.transform.gameObject == gameObject)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = $"{itemDetails.ItemName} - {itemDetails.ItemPrice} MK";

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (shop.shopRefs.cashRegister.AddToCart(itemDetails) == 0)
                        {
                            if (!itemDetails.MultiplePurchases)
                            {
                                gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
#endif
    }

}