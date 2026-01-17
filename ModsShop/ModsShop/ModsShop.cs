#if !Mini
using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.IO;
using UnityEngine;

namespace ModsShop;
public class ModsShop : Mod
{
    public override string ID => "ModsShop";
    public override string Name => "Mods Shop (shop for mods)";
    public override string Author => "piotrulos";
    public override string Version => "1.2";
    public override string Description => description;
    public override Game SupportedGames => Game.MySummerCar_And_MyWinterCar;

    public GameObject modShop;
    internal SettingsCheckBox interiorShadows;
    internal FsmBool shadowsHouse;
    internal static ModsShop instance;
    private bool noAss = false;
    private AssetBundle assetBundle;
    private Shop mainShop;
    private string description;
    [Obsolete]
    private ShopItem shopGameObject;
    public override void ModSetup()
    {
        instance = this;
        if(ModLoader.CurrentGame == Game.MyWinterCar)
            description = "Standalone shop that can be used to put stuff by mods. Shop is integrated in PSK shop. <color=orange>It's empty if no mods that uses it are installed.</color>";
        if (ModLoader.CurrentGame == Game.MySummerCar)
            description = "Standalone shop that can be used to put stuff by mods. Shop is located near inspection building. <color=orange>It's empty if no mods that uses it are installed.</color>";
        SetupFunction(Setup.OnMenuLoad, InitializeShop);
        if (ModLoader.CurrentGame == Game.MySummerCar)
            SetupFunction(Setup.PreLoad, BuildShop);
        if (ModLoader.CurrentGame == Game.MyWinterCar)
            SetupFunction(Setup.PreLoad, BuildShop_MWC);
        if (ModLoader.CurrentGame == Game.MySummerCar)
            SetupFunction(Setup.OnLoad, LegacyShopLoad);


        SetupFunction(Setup.PostLoad, PostLoad);

        if (ModLoader.CurrentGame == Game.MySummerCar)
            SetupFunction(Setup.ModSettings, Mod_Settings);
        if (ModLoader.CurrentGame == Game.MyWinterCar)
            SetupFunction(Setup.ModSettings, Mod_Settings_MWC);
    }
    void PostLoad()
    {
        if (noAss) return;
        mainShop.shopRefs.autoShelves.UpdateLastSticker();
        ModsShop.GetShopReference().shopRefs.finished = true;
    }
    void BuildShop_MWC()
    {
        if (noAss) return;
        assetBundle = LoadAssets.LoadBundle(this, "shopassets.unity3d");
        GameObject shelves = assetBundle.LoadAsset<GameObject>("Main Shop Area (psk).prefab");

        GameObject psk = GameObject.Find("PERAPORTTI");
        modShop = new GameObject("Mod Shop");
        modShop.transform.SetParent(psk.transform.Find("Building/LOD/Store"), false);
        modShop.transform.localPosition = new Vector3(14f, 16f, 1.2f);
        modShop.transform.localEulerAngles = new Vector3(90, 0, 0);
        shelves = GameObject.Instantiate(shelves);
        shelves.transform.SetParent(modShop.transform, false);
        mainShop.shopRefs = shelves.GetComponent<ShopRefs>();
        assetBundle.Unload(false);
    }
    void Mod_Settings_MWC()
    {
        ConsoleCommand.Add(new DebugCmd());
    }
    void InitializeShop()
    {
        if (!File.Exists(Path.Combine(ModLoader.GetModAssetsFolder(this), "shopassets.unity3d")))
        {
            noAss = true;
            ModUI.ShowMessage($"Asset files for <color=orange>Mods Shop</color> not found.{Environment.NewLine} Make sure you unpacked ALL files from zip into mods folder.", "Mods Shop - Fatal error");
            return;
        }
        GameObject go = new GameObject("Shop for mods");
        GameObject.DontDestroyOnLoad(go);
#pragma warning disable CS0612 // Type or member is obsolete
        if (ModLoader.CurrentGame == Game.MySummerCar)
            LegacyShop(go);
#pragma warning restore CS0612 // Type or member is obsolete
        mainShop = go.AddComponent<Shop>(); //Standalone shop

    }
    [Obsolete]
    void LegacyShop(GameObject go)
    {
        shopGameObject = go.AddComponent<ShopItem>();
    }

    void BuildShop()
    {
        if (noAss) return;
        assetBundle = LoadAssets.LoadBundle(this, "shopassets.unity3d");

        //Load standalone shop from bundle
        GameObject shelves = assetBundle.LoadAsset<GameObject>("Main Shop Area.prefab");
        GameObject inspection = GameObject.Find("INSPECTION");
        //disable 2 random shelves and add anti-MOP enabler because MOP is so shit.
        inspection.transform.Find("LOD/garage_shelf_bars 1").gameObject.AddComponent<MopShelfHack>();
        inspection.transform.Find("LOD/garage_shelf_bars 2").gameObject.AddComponent<MopShelfHack>();
        //disable weird reflections on window
        inspection.transform.GetChild(1).GetChild(inspection.transform.GetChild(1).childCount - 1).GetComponent<MeshRenderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        //Create new GameObject that will be parent for shop stuff
        modShop = new GameObject("Mod Shop");
        modShop.transform.SetParent(inspection.transform.Find("LOD"), false);
        modShop.transform.localPosition = new Vector3(-4.5f, -32f, 0.2f);
        modShop.transform.localEulerAngles = new Vector3(90, 0, 0);
        shelves = GameObject.Instantiate(shelves);
        shelves.transform.SetParent(modShop.transform, false);
        mainShop.shopRefs = shelves.GetComponent<ShopRefs>();
        inspection.transform.GetChild(1).Find("Floor").transform.SetParent(shelves.transform.Find("Info Board"), false);
        shelves.transform.Find("Info Board/Floor").localEulerAngles = new Vector3(0, 0, 352);
        shelves.transform.Find("Info Board/Floor").localPosition = new Vector3(0.5f, 0, 0);
        inspection.transform.Find("LOD/inspection_windows").GetComponent<Renderer>().material.SetFloat("_Metallic", 0f);
    }

    void LegacyShopLoad()
    {
        if (noAss) return;
        //old code for legacy shop
#pragma warning disable CS0612 // Type or member is obsolete
        shopGameObject.legacyDisplay = assetBundle.LoadAsset<GameObject>("LegacyDisplayItem.prefab");
#pragma warning restore CS0612 // Type or member is obsolete
        assetBundle.Unload(false);
        shadowsHouse = GameObject.Find("Systems/Options").GetPlayMaker("GFX").FsmVariables.FindFsmBool("ShadowsHouse");
        mainShop.shopRefs.SetShadows();
    }

    private void Mod_Settings()
    {
        ConsoleCommand.Add(new DebugCmd());
        Settings.AddText($"By default it uses {"House Shadows"} game setting to enable/disable shops interior shadows.{Environment.NewLine}Below option will force disable shadows in shops interior. (despite your game settings)");
        interiorShadows = Settings.AddCheckBox("MS_interiorShadows", "Disable shadows from interior lights", false, ChangeShadows);
    }
    void ChangeShadows()
    {
        if (mainShop.shopRefs == null) return;
        mainShop.shopRefs.SetShadows();
    }

    /// <summary>
    /// Get valid reference to ModsShop
    /// </summary>
    /// <returns>Shop</returns>
    public static Shop GetShopReference()
    {
        if (instance.mainShop == null) return null;
        return instance.mainShop;
    }
}
#endif