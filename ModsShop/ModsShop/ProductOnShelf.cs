using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsShop;

public class ProductOnShelf : MonoBehaviour
{
    [Header("Your mod ID (case sensitive)")]
    public string ModID;
    [Header("Your item ID (case sensitive)")]
    public string ItemID;
    [Header("Single purchase settings (more info on wiki)")]
    public bool IgnoreItemNotFound = false;

    [HideInInspector]
    public Shop shop;
    [HideInInspector]
    public ItemDetails itemDetails;
    private Collider coll;
#if !Mini
    FsmBool GUIbuy;
    FsmString GUIinteraction;
    MeshRenderer[] meshRenderers;
    void Awake()
    {
        shop = ModsShop.GetShopReference();
        GUIbuy = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy");
        GUIinteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
        coll = GetComponent<Collider>();
    }

    void Start()
    {
        if (itemDetails == null)
        {
            itemDetails = shop.GetItemDetailsByID(ModID, ItemID);
            if (itemDetails == null)
            {
                if (IgnoreItemNotFound)
                    ModConsole.Print($"Shop: Shop itemID <b>{ItemID}</b> not found in mod <b>{ModID}</b> [Ignored by mod setting]");
                else
                    ModConsole.Error($"Shop: Shop itemID <b>{ItemID}</b> not found in mod <b>{ModID}</b>");
                return;
            }
        }
        itemDetails.product = this;
        List<MeshRenderer> mr = new List<MeshRenderer>();
        if (gameObject.GetComponent<MeshRenderer>() != null)
        {
            mr.Add(gameObject.GetComponent<MeshRenderer>());
        }
        else
        {
            mr.AddRange(GetComponentsInChildren<MeshRenderer>());
        }
        meshRenderers = [.. mr];
    }

    void OnMouseExit()
    {
        if (itemDetails == null) return;
        GUIbuy.Value = false;
        GUIinteraction.Value = string.Empty;
    }
    internal void Cancel()
    {
        if (!itemDetails.MultiplePurchases)
        {
            gameObject.SetActive(true);
        }
    }
    int ItemsInCart(ItemDetails itemDetails)
    {
        if(shop.shopRefs.shoppingCart.TryGetValue(itemDetails, out int value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }
    void Update()
    {
        if (itemDetails == null) return;
        if (shop.shopRefs.isShopClosed) return;
        if (UnifiedRaycast.GetRaycastHit().transform == null) return;
        if (UnifiedRaycast.GetRaycastHit().transform.gameObject == gameObject)
        {
            GUIbuy.Value = true;
            if(ModLoader.CurrentGame == Game.MyWinterCar)
                GUIinteraction.Value = $"{itemDetails.ItemName} - {itemDetails.ItemPrice} MK{Environment.NewLine}{ItemsInCart(itemDetails)} in cart";
            if(ModLoader.CurrentGame == Game.MySummerCar)
                GUIinteraction.Value = $"{itemDetails.ItemName} - {itemDetails.ItemPrice} MK";

            if (Input.GetMouseButtonDown(0))
            {
                if(ModLoader.CurrentGame == Game.MyWinterCar)
                {
                    if (shop.shopRefs.cashRegisterMWC.AddToCart(itemDetails) == 0)
                    {
                        if (!itemDetails.MultiplePurchases)
                        {
                            for (int i = 0; i < meshRenderers.Length; i++)
                            {
                                meshRenderers[i].enabled = false;
                            }
                        }
                    }
                }
                if (ModLoader.CurrentGame == Game.MySummerCar)
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
            if (Input.GetMouseButtonDown(1))
            {
                if (ModLoader.CurrentGame == Game.MyWinterCar)
                {
                    shop.shopRefs.cashRegisterMWC.ReduceFromCart(itemDetails);
                    if (!itemDetails.MultiplePurchases)
                    {
                        for (int i = 0; i < meshRenderers.Length; i++)
                        {
                            meshRenderers[i].enabled = true;
                        }
                    }
                }
            }
        }
    }
#endif
}