#if !Mini
using MSCLoader;
using System.Collections.Generic;
using UnityEngine;

namespace ModsShop;
public class DebugCmd : ConsoleCommand
{
    public override string Name => "shopDebug";
    public override string Help => "Command Description";
    public override bool ShowInHelp => false;

    public override void Run(string[] args)
    {
        if (ModLoader.CurrentScene == CurrentScene.Game)
        {
            switch (args[0])
            {
                case "tp":
                    GameObject.Find("PLAYER").transform.position = new Vector3(-1511.606f, 4f, 1234.105f);
                    GameObject.Find("PLAYER").transform.eulerAngles = new Vector3(0f, 332f, 0f);
                    break;
                case "list":
                    bool nay = true;
                    if (args.Length > 1)
                    {
                        for (int i = 0; i < ModsShop.GetShopReference().items.Count; i++)
                        {
                            if (ModsShop.GetShopReference().items[i].ModID == args[1])
                            {
                                nay = false;
                                ModConsole.Print($"[<color=yellow>{ModsShop.GetShopReference().items[i].ItemID}</color>] <color=lime>{ModsShop.GetShopReference().items[i].ItemName}</color> (<color=aqua>{ModsShop.GetShopReference().items[i].SpawnMethod}</color>) - {ModsShop.GetShopReference().items[i].ItemPrice} MK");
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ModsShop.GetShopReference().items.Count; i++)
                        {
                            nay = false;
                            ModConsole.Print($"[<color=yellow>{ModsShop.GetShopReference().items[i].ItemID}</color>] <color=lime>{ModsShop.GetShopReference().items[i].ItemName}</color> (<color=aqua>{ModsShop.GetShopReference().items[i].SpawnMethod}</color>) - {ModsShop.GetShopReference().items[i].ItemPrice} MK");
                        }
                    }
                    if (nay) ModConsole.Print("Nothing...");
                    break;
                case "cart":

                    foreach (KeyValuePair<ItemDetails, int> cartItems in ModsShop.GetShopReference().shopRefs.shoppingCart)
                    {
                        ModConsole.Print($"{cartItems.Key.ItemName} <color=yellow>x{cartItems.Value}</color> ({cartItems.Key.ItemPrice*cartItems.Value} MK)");
                    }
                    break;
                case "switches":
                    if (ModLoader.CurrentGame == Game.MyWinterCar)
                    {
                        ModConsole.Print("Nope");
                        break;
                    }
                    for (int i = 0; i < ModsShop.GetShopReference().shopRefs.lightSwitches.Length; i++)
                    {
                        ModsShop.GetShopReference().shopRefs.lightSwitches[i].MakeInteractable(true);
                    }
                    break;
                default:
                    ModConsole.Error("Invalid syntax");
                    break;
            }
            if (args.Length < 1)
                ModConsole.Error("Invalid syntax");
        }
    }
}
#endif