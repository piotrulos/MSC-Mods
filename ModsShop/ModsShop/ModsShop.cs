using MSCLoader;
using System;
using UnityEngine;

namespace ModsShop
{
    public class ModsShop : Mod
    {
        public override string ID => "ModsShop";
        public override string Name => "Shop Catalog - BETA";
        public override string Author => "piotrulos";
        public override string Version => "0.9";

        public override bool UseAssetsFolder => true;
        public override bool LoadInMenu => true;
        string niggerList = "76561198063306360";
        ShopItem shopGameObject;
        bool isLoaded = false;

        public override void OnMenuLoad()
        {
            GameObject go = new GameObject();
            go.name = "Shop for mods";
            GameObject.DontDestroyOnLoad(go);
            shopGameObject = go.AddComponent<ShopItem>();
        }

        // Update is called once per frame
        public override void OnLoad()
        {
            if (ModLoader.steamID == niggerList)
                throw new Exception("Nie dla psa!");
            AssetBundle ab = LoadAssets.LoadBundle(this, "shopassets.unity3d");
            shopGameObject.modPref = ab.LoadAsset("Mod.prefab") as GameObject;
            shopGameObject.catPref = ab.LoadAsset("Category.prefab") as GameObject;
            shopGameObject.itemPref = ab.LoadAsset("Product.prefab") as GameObject;
            shopGameObject.cartItemPref = ab.LoadAsset("CartItem.prefab") as GameObject;
            GameObject te = ab.LoadAsset("Catalog_shelf.prefab") as GameObject;
            //teimo catalog pos
            //-1550.65, 4.7, 1183.3
            //0,345,0
            te = GameObject.Instantiate(te);
            te.name = "Catalog shelf";
            te.transform.position = new Vector3(-1550.65f, 4.7f, 1183.3f);
            te.transform.localEulerAngles = new Vector3(0, 345, 0);
            shopGameObject.teimoCatalog = te.transform.GetChild(1).GetComponent<BoxCollider>();
            GameObject teimoUI = ab.LoadAsset("Teimo Catalog.prefab") as GameObject;
            teimoUI = GameObject.Instantiate(teimoUI);
            teimoUI.name = "Teimo Catalog";
            teimoUI.transform.SetParent(GameObject.Find("MSCLoader Canvas").transform, false);
            teimoUI.SetActive(false);
            shopGameObject.shopCatalogUI = teimoUI;

            shopGameObject.Prepare();
            isLoaded = true;
            ab.Unload(false);


        }

        public override void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.Alpha9)) //debug mouse unlock
            {
                ModConsole.Print(PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerInMenu").Value);
                ModConsole.Print(shopGameObject.teimoShopItems[0].mod.ID);
            }*/
        }
    }
}
