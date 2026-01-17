using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModsShop;

public class ShoppingCartUI : MonoBehaviour
{
    public CashRegister cashRegister;
    public GameObject cartItem;
    public GameObject listView;
    public Text priceText;
    public GameObject ui;

    private byte bagSpawnPoint = 0;
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
        if (cashRegister.shopRefs.shoppingCart.Count == 0)
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
        StartCoroutine(DelayMenu());

#endif
    }


    public void CancelBtn()
    {
        PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value = false; //unlock mouse
        GameObject.Find("Systems").transform.GetChild(7).gameObject.SetActive(false); //can't clickthrough UI when menu is active.
        ui.SetActive(false);
        StartCoroutine(DelayMenu());
    }
    IEnumerator DelayMenu()
    {
        yield return new WaitForSeconds(1f);
        cashRegister.uiOpen = false;
    }
#if !Mini

    IEnumerator SpawnStuff()
    {
        int spawnP = 0;
        int baggableItems = 0;
        Dictionary<ItemDetails, int> cartItemsSpawn = new Dictionary<ItemDetails, int>(cashRegister.shopRefs.shoppingCart);
        yield return null;
        cashRegister.shopRefs.shoppingCart.Clear();
        PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value -= cashRegister.totalPrice;
        cashRegister.UpdateCart(false);

        //Don't put custom spawn items in the bag
        baggableItems = cartItemsSpawn.Where(x => x.Key.SpawnMethod != SpawnMethod.Custom && !x.Key.ExcludeFromShoppingBag).Select(x => x.Value).Sum();
        int baggedItems = 0;
        GameObject bag = null;

        //Start Spawning       
        foreach (KeyValuePair<ItemDetails, int> cartItems in cartItemsSpawn.Where(x => x.Key.SpawnMethod != SpawnMethod.Custom))
        {
            GameObject spawnedObj = cartItems.Key.ItemPrefab;
            bool canBag = baggableItems > 2 && !cartItems.Key.ExcludeFromShoppingBag;
            if (baggedItems >= 25) baggedItems = 0;
            switch (cartItems.Key.SpawnMethod)
            {
                case SpawnMethod.Instantiate:
                    for (int i = 0; i < cartItems.Value; i++)
                    {
                        spawnedObj = Instantiate(cartItems.Key.ItemPrefab);
                        spawnedObj.GetComponent<Rigidbody>().isKinematic = false;
                        if (canBag)
                        {
                            if (baggedItems >= 25) baggedItems = 0;
                            CheckoutCallback(spawnedObj, cartItems.Key, spawnP);
                            if (bag == null || baggedItems == 0)
                            {
                                bag = SpawnBag();
                            }
                            spawnedObj.transform.SetParent(bag.transform, false);
                            spawnedObj.SetActive(false);
                            baggedItems++;
                        }
                        else
                        {
                            CheckoutCallback(spawnedObj, cartItems.Key, spawnP);
                            spawnedObj.transform.position = cashRegister.spawnPoint[spawnP].position;
                            yield return new WaitForSeconds(.2f);
                        }
                    }
                    break;
                case SpawnMethod.SetActive:
                    spawnedObj.GetComponent<Rigidbody>().isKinematic = false;
                    if (canBag)
                    {
                        CheckoutCallback(spawnedObj, cartItems.Key, spawnP);
                        if (bag == null || baggedItems == 0)
                        {
                            bag = SpawnBag();
                        }
                        spawnedObj.transform.SetParent(bag.transform, false);
                        spawnedObj.SetActive(false);
                        baggedItems++;
                    }
                    else
                    {
                        spawnedObj.SetActive(true);
                        CheckoutCallback(spawnedObj, cartItems.Key, spawnP);
                        spawnedObj.transform.position = cashRegister.spawnPoint[spawnP].position;
                        yield return new WaitForSeconds(.2f);
                    }
                    break;
            }
            if (!canBag)
                yield return new WaitForSeconds(.1f);
            spawnP++;
            if (spawnP == cashRegister.spawnPoint.Length) spawnP = 0;
        }
        //Spawn Custom last
        foreach (KeyValuePair<ItemDetails, int> cartItems in cartItemsSpawn.Where(x => x.Key.SpawnMethod == SpawnMethod.Custom))
        {
            for (int i = 0; i < cartItems.Value; i++)
            {
                CheckoutCallback(cartItems.Key.ItemPrefab, cartItems.Key, spawnP);
                yield return new WaitForSeconds(.2f);
            }
            spawnP++;
            if (spawnP == cashRegister.spawnPoint.Length) spawnP = 0;
        }
    }

    void CheckoutCallback(GameObject spawned, ItemDetails item, int spawn)
    {
        if (item.Checkout == null) return;
        try
        {
            item.Checkout(new Checkout(spawned, item.ItemID, cashRegister.spawnPoint[spawn].position));
            item.bought = true;
        }
        catch (Exception e)
        {
            ModConsole.Error($"Fatal error: in mod {item.ModID} item's {item.ItemID} checkout action.");
            ModConsole.Error($"{e.Message}");
            Console.WriteLine(e);
        }
    }
    private GameObject SpawnBag()
    {
        GameObject bag = Instantiate(cashRegister.bagPrefab);
        bag.transform.SetParent(cashRegister.bagSpawnPoint[bagSpawnPoint], false);
        bag.MakePickable();
        bagSpawnPoint++;
        if (bagSpawnPoint == cashRegister.bagSpawnPoint.Length) bagSpawnPoint = 0;
        return bag;
    }
    internal void PopulateCart()
    {
        UpdateCart(false);
    }

    internal void MoreLess(ItemDetails item, bool more)
    {
        if (cashRegister.shopRefs.shoppingCart.ContainsKey(item))
        {
            if (more)
            {
                if (item.MultiplePurchases)
                {
                    if (cashRegister.shopRefs.shoppingCart[item] < 10)
                        cashRegister.shopRefs.shoppingCart[item] += 1;
                    else return;
                }
            }
            else
            {
                if (cashRegister.shopRefs.shoppingCart[item] > 1)
                    cashRegister.shopRefs.shoppingCart[item] -= 1;
                else return;
            }
            UpdateCart(true);
        }
    }

    private void UpdateCart(bool aud)
    {
        RemoveChildren(listView.transform);
        foreach (KeyValuePair<ItemDetails, int> cartItems in cashRegister.shopRefs.shoppingCart)
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
        if (cashRegister.shopRefs.shoppingCart.ContainsKey(det))
            cashRegister.shopRefs.shoppingCart.Remove(det);
        det.product.Cancel();
        UpdateCart(true);
    }
#endif
}