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
        public ItemDetails ItemDetails;

        void Awake()
        {
#if !Mini
            shop = ModsShop.GetShopReference();
#endif
        }

        void Start()
        {
            if (ItemDetails == null)
            {
                ItemDetails = shop.GetItemDetailsByID($"{ModID}_{ItemID}");
                if (ItemDetails == null)
                {
                    ModConsole.Error($"Shop: Shop itemID <b>{ItemID}</b> not found in mod <b>{ModID}</b>");
                    return;
                }
            }
            ItemDetails.product = this;
        }

        void OnMouseExit()
        {
            if (ItemDetails == null) return;

#if !Mini
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
#endif

        }
        void Cancel()
        {
            if (!ItemDetails.MultiplePurchases)
            {
                gameObject.SetActive(true);
            }
        }
        void Update()
        {
            if (ItemDetails == null) return;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1f))
            {
                if (hit.transform.gameObject == gameObject)
                {
#if !Mini
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = $"{ItemID} - {ItemDetails.ItemPrice} MK";
#endif
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (shop.shopRefs.cashRegister.AddToCart(ItemDetails) == 0) 
                        {
                            if (!ItemDetails.MultiplePurchases)
                            {
                                gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }

}