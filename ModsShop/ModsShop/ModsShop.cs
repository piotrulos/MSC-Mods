﻿#if !Mini
using MSCLoader;
using System;
using System.IO;
using UnityEngine;

namespace ModsShop
{
    public class ModsShop : Mod
    {
        public override string ID => "ModsShop";
        public override string Name => "Mods Shop (shop for mods)";
        public override string Author => "piotrulos";
        public override string Version => "1.0.4";

        public override string Description => "Standalone shop that can be used to put stuff by mods. Shop is located near inspection building.";

        public GameObject modShop;
        public static Camera mainCam = null;
        private Shop mainShop;
#pragma warning disable CS0618 
        private ShopItem shopGameObject;
#pragma warning restore CS0618
        internal static ModsShop instance;
        AssetBundle assetBundle;
        internal SettingsCheckBox interiorShadows;
        bool noAss = false;
        public override void ModSetup()
        {
            instance = this; 
            SetupFunction(Setup.OnMenuLoad, InitializeShop);
            SetupFunction(Setup.PreLoad, BuildShop);
            SetupFunction(Setup.OnLoad, LegacyShopLoad);
        }

        void InitializeShop()
        {
            GameObject go = new GameObject("Shop for mods");
            GameObject.DontDestroyOnLoad(go);
#pragma warning disable CS0618
            shopGameObject = go.AddComponent<ShopItem>(); //Legacy shop
#pragma warning restore CS0618
            mainShop = go.AddComponent<Shop>(); //Standalone shop
            if (!File.Exists(Path.Combine(ModLoader.GetModAssetsFolder(this), "shopassets.unity3d")))
            {
                noAss = true;
                ModUI.ShowMessage($"Asset files for <color=orange>Mods Shop</color> not found.{Environment.NewLine} Make sure you unpacked ALL files from zip into mods folder.", "Mods Shop - Fatal error");
                return;
            }
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
            mainShop.shopRefs.SetShadows();
        }

        void LegacyShopLoad()
        {
            if (noAss) return;
            //old code for legacy shop
            shopGameObject.modPref = assetBundle.LoadAsset<GameObject>("Mod.prefab");
            shopGameObject.catPref = assetBundle.LoadAsset<GameObject>("Category.prefab");
            shopGameObject.itemPref = assetBundle.LoadAsset<GameObject>("Product.prefab");
            shopGameObject.cartItemPref = assetBundle.LoadAsset<GameObject>("CartItem.prefab");
            GameObject te = assetBundle.LoadAsset<GameObject>("Catalog_shelf.prefab");
            GameObject fl = assetBundle.LoadAsset<GameObject>("Catalog_shelf_F.prefab");
            mainCam = HutongGames.PlayMaker.FsmVariables.GlobalVariables.FindFsmGameObject("POV").Value.GetComponent<Camera>();
            //teimo catalog pos
            //-1550.65, 4.7, 1183.3
            //0,345,0
            te = GameObject.Instantiate(te);
            te.name = "Catalog shelf";
            te.transform.position = new Vector3(-1550.65f, 4.7f, 1183.3f);
            te.transform.localEulerAngles = new Vector3(0, 345, 0);
            shopGameObject.teimoCatalog = te.transform.GetChild(1).GetComponent<BoxCollider>();
            if(shopGameObject.teimoShopItems.Count == 0)
                te.SetActive(false);
            //fleetari catalog pos
            //1554.1, 5.54, 739.7
            //0,90,0
            fl = GameObject.Instantiate(fl);
            fl.name = "Catalog shelf (Fleetari)";
            fl.transform.position = new Vector3(1553.9f, 5.54f, 740.1f);
            fl.transform.localEulerAngles = new Vector3(0, 40, 0);
            shopGameObject.fleetariCatalog = fl.transform.GetChild(1).GetComponent<BoxCollider>();
            if (shopGameObject.fleetariShopItems.Count == 0)
                fl.SetActive(false);

            GameObject teimoUI = assetBundle.LoadAsset("Teimo Catalog.prefab") as GameObject;
            teimoUI = GameObject.Instantiate(teimoUI);
            teimoUI.name = "Teimo Catalog";
            teimoUI.transform.SetParent(ModUI.GetCanvas().transform, false);
            teimoUI.SetActive(false);
            shopGameObject.shopCatalogUI = teimoUI;
            shopGameObject.Prepare();
            assetBundle.Unload(false);
        }

        public override void ModSettings()
        {
            if (wl) return;
            ConsoleCommand.Add(new DebugCmd());
            interiorShadows = Settings.AddCheckBox(this, "interiorShadows", "Disable shadows from interior lights", false, ChangeShadows);
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
#region BS
        bool wl = false;
        public override void OnMenuLoad()
        {
            ModUI.ShowYesNoMessage($"ModsShop: <color=aqua>To use this version of mod you need to <color=orange>Update MSCLoader to version 1.2+</color></color>{System.Environment.NewLine}{System.Environment.NewLine}Open download page?", "Outdated MSCloader Version".ToUpper(), delegate
            {
                Application.OpenURL("https://www.nexusmods.com/mysummercar/mods/147?tab=files");
            });
            if (GameObject.Find("MSCLoader Canvas") != null)
            {
                GameObject mp = GameObject.Find("MSCLoader Canvas").transform.Find("ModPrompt(Clone)").gameObject;
                if (mp != null)
                    mp.transform.localScale = Vector3.one;
            }
            wl = true;
            return;
        }
        public override void OnLoad()
        {
            ModConsole.Error("ModsShop: Oudated MSCLoader version, please update modloader to version <b>1.2.5</b> or highier to use this mod.");
        }
#endregion
    }
}
#endif