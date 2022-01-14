#if !Mini
using MSCLoader;
using System;
using UnityEngine;

namespace ModsShop
{
    public class ModsShop : Mod
    {
        public override string ID => "ModsShop";
        public override string Name => "Shop for mods (test)";
        public override string Author => "piotrulos";
        public override string Version => "0.9.4";

        ShopItem shopGameObject;
        bool experimentalShop = true;
        public GameObject modShop;

        public override void ModSetup()
        {
            SetupFunction(Setup.OnMenuLoad, Shop_OnMenuLoad);
            SetupFunction(Setup.OnLoad, Shop_OnLoad);
        }

        public void Shop_OnMenuLoad()
        {
            GameObject go = new GameObject("Shop for mods");
            GameObject.DontDestroyOnLoad(go);
            shopGameObject = go.AddComponent<ShopItem>();
        }

        // Update is called once per frame
        public void Shop_OnLoad()
        {
            AssetBundle ab = LoadAssets.LoadBundle(this, "shopassets.unity3d");
            shopGameObject.modPref = ab.LoadAsset<GameObject>("Mod.prefab");
            shopGameObject.catPref = ab.LoadAsset<GameObject>("Category.prefab");
            shopGameObject.itemPref = ab.LoadAsset<GameObject>("Product.prefab");
            shopGameObject.cartItemPref = ab.LoadAsset<GameObject>("CartItem.prefab");
            GameObject te = ab.LoadAsset<GameObject>("Catalog_shelf.prefab");
            GameObject fl = ab.LoadAsset<GameObject>("Catalog_shelf_F.prefab");
            if (experimentalShop)
            {
                GameObject door = ab.LoadAsset<GameObject>("door.prefab");
                GameObject shelves = ab.LoadAsset<GameObject>("Kenneths stuff.prefab");
                GameObject shop_banner = ab.LoadAsset<GameObject>("shop_banner.prefab");
             //   GameObject lamps = ab.LoadAsset<GameObject>("Lamps.prefab");

                //disable 2 random shelves
                GameObject.Find("INSPECTION").transform.Find("LOD/garage_shelf_bars 1").gameObject.AddComponent<MopShelfHack>();
                GameObject.Find("INSPECTION").transform.Find("LOD/garage_shelf_bars 2").gameObject.AddComponent<MopShelfHack>();
                //disable weird reflections on window
                GameObject.Find("INSPECTION").transform.GetChild(1).GetChild(GameObject.Find("INSPECTION").transform.GetChild(1).childCount - 1).GetComponent<MeshRenderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                //Create new GameObject that will be parent for shop stuff
                modShop = new GameObject("Mod Shop");
                modShop.transform.SetParent(GameObject.Find("INSPECTION").transform, false);
                modShop.transform.localPosition = new Vector3(-4.5f, -32f, 0.2f);
                modShop.transform.localEulerAngles = new Vector3(90, 0, 0);
                door = GameObject.Instantiate(door);
                GameObject.Find("INSPECTION").transform.GetChild(1).GetChild(GameObject.Find("INSPECTION").transform.GetChild(1).childCount - 1).GetComponent<MeshRenderer>().material = door.transform.GetChild(1).GetComponent<MeshRenderer>().material;
                door.name = "Door";
                door.transform.SetParent(modShop.transform, false);
                door.transform.localPosition = new Vector3(0.17f, -0.02f, 3.17f);
                shelves = GameObject.Instantiate(shelves);
                shelves.transform.SetParent(modShop.transform, false);
                // shelves.transform.localPosition = new Vector3(0.5f, 1.1f, 0.3f);
                GameObject.Find("INSPECTION").transform.GetChild(1).Find("Floor").transform.SetParent(shelves.transform.GetChild(1), false);
                //shelves.transform.GetChild(0).GetChild(0).localEulerAngles = Vector3.zero;
                shelves.transform.GetChild(0).Find("Floor").localEulerAngles = new Vector3(0, 0, 352);
                shelves.transform.GetChild(0).Find("Floor").localPosition = new Vector3(0.5f, 0, 0);
                //GameObject lr = GameObject.Find("YARD/Building/LIVINGROOM/");
                /* GameObject rallyAd = GameObject.Instantiate(GameObject.Find("YARD/Building/LIVINGROOM/").transform.GetChild(0).GetChild(11).GetChild(2).gameObject);
                 rallyAd.transform.SetParent(shelves.transform.GetChild(0), false);*/
          /*      lamps = GameObject.Instantiate(lamps);
                lamps.transform.SetParent(modShop.transform, false);
                lamps.AddComponent<LampsOnOff>();
                foreach (Transform c in lamps.transform)
                {
                    c.gameObject.AddComponent<Lamp>().bulbs[0] = c.GetChild(0).gameObject;
                    c.GetComponent<Lamp>().bulbs[1] = c.GetChild(1).gameObject;
                    c.GetComponent<Lamp>().light = c.GetChild(2).GetComponent<Light>();
                    lamps.GetComponent<LampsOnOff>().lamps.Add(c.GetComponent<Lamp>());
                }*/
                shop_banner = GameObject.Instantiate(shop_banner);
                shop_banner.transform.SetParent(modShop.transform, false);
                shop_banner.transform.localPosition = new Vector3(0.8f, 3.1f, 4.38f);
                shop_banner.transform.localEulerAngles = new Vector3(0, 270, 0);
            }

            //old code


            //teimo catalog pos
            //-1550.65, 4.7, 1183.3
            //0,345,0
            te = GameObject.Instantiate(te);
            te.name = "Catalog shelf";
            te.transform.position = new Vector3(-1550.65f, 4.7f, 1183.3f);
            te.transform.localEulerAngles = new Vector3(0, 345, 0);
            shopGameObject.teimoCatalog = te.transform.GetChild(1).GetComponent<BoxCollider>();
            //fleetari catalog pos
            //1554.1, 5.54, 739.7
            //0,90,0
            fl = GameObject.Instantiate(fl);
            fl.name = "Catalog shelf (Fleetari)";
            fl.transform.position = new Vector3(1553.9f, 5.54f, 740.1f);
            fl.transform.localEulerAngles = new Vector3(0, 40, 0);
            shopGameObject.fleetariCatalog = fl.transform.GetChild(1).GetComponent<BoxCollider>();

            GameObject teimoUI = ab.LoadAsset("Teimo Catalog.prefab") as GameObject;
            teimoUI = GameObject.Instantiate(teimoUI);
            teimoUI.name = "Teimo Catalog";
            teimoUI.transform.SetParent(GameObject.Find("MSCLoader Canvas").transform, false);
            teimoUI.SetActive(false);
            shopGameObject.shopCatalogUI = teimoUI;
            shopGameObject.Prepare();
            ab.Unload(false);


        }

        public override void ModSettings()
        {
            if (wl) return;
            ConsoleCommand.Add(new DebugCmd());
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
            ModConsole.Error("ModsShop: Oudated MSCLoader version, please update modloader to version <b>1.2</b> or highier to use this mod.");
        }
#endregion
    }
}
#endif