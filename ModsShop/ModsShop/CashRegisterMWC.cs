using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if !Mini
#endif
namespace ModsShop;

public class CashRegisterMWC : MonoBehaviour
{
    public ShoppingCartUI shoppingCartUI;
    public Transform[] spawnPoint;
    public Transform[] bagSpawnPoint;
    public GameObject bagPrefab;
    public Collider coll;
    public ShopRefs shopRefs;

    internal bool uiOpen = false;

#if !Mini
    private PlayMakerFSM pskRegister;
    private byte bagSpawnPointNo = 0;
    void Start()
    {
        pskRegister = GameObject.Find("PERAPORTTI").transform.Find("Building/Store/Cashier/StoreCashRegister/CashRegisterLogic").GetComponent<PlayMakerFSM>();
        pskRegister.FsmInject("Purchase", ModsShopPurchase);
    }

    public byte AddToCart(ItemDetails item)
    {
        if (shopRefs.shoppingCart.ContainsKey(item))
        {
            if (item.MultiplePurchases)
            {
                if (shopRefs.shoppingCart[item] < 10)
                {
                    shopRefs.shoppingCart[item] += 1;
                    UpdateCart(item, false);
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

        shopRefs.shoppingCart.Add(item, 1);
        UpdateCart(item, false);
        return 0;
    }
    private void ModsShopPurchase()
    {
        if (shopRefs.shoppingCart.Count == 0) 
            return;        
        StartCoroutine(SpawnStuff());
    }
    IEnumerator SpawnStuff()
    {
        int spawnP = 0;
        int baggableItems = 0;
        Dictionary<ItemDetails, int> cartItemsSpawn = new Dictionary<ItemDetails, int>(shopRefs.shoppingCart);
        yield return null;
        shopRefs.shoppingCart.Clear();

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
                     //   spawnedObj.GetComponent<Rigidbody>().isKinematic = false;
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
                            spawnedObj.transform.position = spawnPoint[spawnP].position;
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
                        spawnedObj.transform.position = spawnPoint[spawnP].position;
                        yield return new WaitForSeconds(.2f);
                    }
                    break;
            }
            if (!canBag)
                yield return new WaitForSeconds(.1f);
            spawnP++;
            if (spawnP == spawnPoint.Length) spawnP = 0;
            if(!cartItems.Key.MultiplePurchases && ModLoader.CurrentGame == Game.MyWinterCar) cartItems.Key.product.gameObject.SetActive(false);
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
            if (spawnP == spawnPoint.Length) spawnP = 0;
            if (!cartItems.Key.MultiplePurchases && ModLoader.CurrentGame == Game.MyWinterCar) cartItems.Key.product.gameObject.SetActive(false);

        }
    }

    void CheckoutCallback(GameObject spawned, ItemDetails item, int spawn)
    {
        if (item.Checkout == null) return;
        try
        {
            item.Checkout(new Checkout(spawned, item.ItemID, spawnPoint[spawn].position));
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
        GameObject bag = Instantiate(bagPrefab);
        bag.transform.SetParent(bagSpawnPoint[bagSpawnPointNo], false);
        bag.MakePickable();
        bagSpawnPointNo++;
        if (bagSpawnPointNo == bagSpawnPoint.Length) bagSpawnPointNo = 0;
        return bag;
    }
    internal void UpdateCart(ItemDetails item, bool reduce)
    {
        if (reduce)
            pskRegister.FsmVariables.GetFsmFloat("PriceTotal").Value -= item.ItemPrice;
        else
            pskRegister.FsmVariables.GetFsmFloat("PriceTotal").Value += item.ItemPrice;
        pskRegister.SendEvent("PURCHASE");

    }
    internal void RemoveItem(ItemDetails det)
    {
        if (shopRefs.shoppingCart.ContainsKey(det))
            shopRefs.shoppingCart.Remove(det);
        det.product.Cancel();

    }
    internal void ReduceFromCart(ItemDetails item)
    {
        if (shopRefs.shoppingCart.ContainsKey(item))
        {
            if (shopRefs.shoppingCart[item] > 1)
                shopRefs.shoppingCart[item] -= 1;
            else
                RemoveItem(item);

            UpdateCart(item, true);
        }
    }

#endif
}