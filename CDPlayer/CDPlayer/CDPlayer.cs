using MSCLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CDPlayer
{
    public class CDSaveData
    {
        public List<CDSaveDataList> cds = new List<CDSaveDataList>();
        public List<CaseSaveDataList> cases = new List<CaseSaveDataList>();
        public Vector3 rackpos;
        public float rackrotX, rackrotY, rackrotZ;
        public bool rackpurchased;
    }
    public class CDSaveDataList
    {
        public Vector3 pos;
        public float rotX, rotY, rotZ;
        public string CDName;
        public bool inCase;
        // public bool purchased = true;
    }
    public class CaseSaveDataList
    {
        public Vector3 pos;
        public float rotX, rotY, rotZ;
        public string CDName;
        public bool inRack;
        //public int inRackNr;
        public int inRackSlot;
        public bool purchased;
    }


    public class CDPlayer : Mod
    {

        public override string ID => "CDPlayer";
        public override string Name => "CDPlayer Enchanced";
        public override string Author => "Piotrulos";
        public override string Version => "1.3.1";

        //Set this to true if you will be load custom assets from Assets folder.
        //This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;

        public string path = Path.GetFullPath("CD");

        Settings resetCds = new Settings("resetcd", "Reset CDs", ResetPosition);
        public Settings debugInfo = new Settings("debugInfo", "Show debug info", false);
        public Settings RDSsim = new Settings("RDSsim", "Simulate RDS", true);
        public Settings channel3url = new Settings("ch3url", "Channel 3:", "http://185.33.21.112:11010");
        public Settings channel4url = new Settings("ch4url", "Channel 4:", string.Empty);
        List<GameObject> listOfCDs, listOfCases;


        public override void OnNewGame()
        {
            string savepath = Path.Combine(ModLoader.GetModConfigFolder(this), "cdplayer.save");
            if (File.Exists(savepath))
                File.Delete(savepath);
        }

        public override void ModSettings()
        {
            Settings.AddHeader(this, "CD Settings");
            Settings.AddButton(this, resetCds, "Respawn purchased stuff on kitchen table");
            Settings.AddHeader(this, "Radio settings");
            Settings.AddCheckBox(this, debugInfo);
            Settings.AddCheckBox(this, RDSsim);
            Settings.AddTextBox(this, channel3url, "Stream URL...");
            Settings.AddTextBox(this, channel4url, "Stream URL...");
        }

        public override void OnSave()
        {
            CDSaveData sd = new CDSaveData();

            if (rack10.activeSelf)
            {
                sd.rackpos = rack10.transform.position;
                sd.rackrotX = rack10.transform.rotation.eulerAngles.x;
                sd.rackrotY = rack10.transform.rotation.eulerAngles.y;
                sd.rackrotZ = rack10.transform.rotation.eulerAngles.z;
                sd.rackpurchased = true;
            }

            foreach (GameObject go in listOfCDs)
            {
                CDSaveDataList sdl = new CDSaveDataList
                {
                    pos = go.transform.position,
                    rotX = go.transform.rotation.eulerAngles.x,
                    rotY = go.transform.rotation.eulerAngles.y,
                    rotZ = go.transform.rotation.eulerAngles.z,
                    CDName = go.GetComponent<CD>().CDName,
                    inCase = go.GetComponent<CD>().inCase
                };
                if (go.GetComponent<CD>().inPlayer)
                    sdl.inCase = true;
                sd.cds.Add(sdl);
            }
            foreach (GameObject go in listOfCases)
            {
                if (go.activeSelf)
                {
                    CaseSaveDataList sdl = new CaseSaveDataList
                    {
                        pos = go.transform.position,
                        rotX = go.transform.rotation.eulerAngles.x,
                        rotY = go.transform.rotation.eulerAngles.y,
                        rotZ = go.transform.rotation.eulerAngles.z,
                        CDName = go.GetComponent<CDCase>().CDName,
                        inRack = go.GetComponent<CDCase>().inRack,
                        inRackSlot = go.GetComponent<CDCase>().inRackSlot,
                        purchased = true
                    };

                    sd.cases.Add(sdl);
                }
            }
            SaveLoad.SerializeSaveFile(this, sd, "cdplayer.save");


        }
        GameObject rack10, cdp, cdCaseP;
        Sprite cd_icon, rack_icon;
        //Called when mod is loading
        public override void OnLoad()
        {
            //fuck playmaker, use AssetBundle!
            AssetBundle ab = LoadAssets.LoadBundle(this, "cdplayer.unity3d");
            rack10 = GameObject.Instantiate(ab.LoadAsset<GameObject>("rack10.prefab"));
            cdp = ab.LoadAsset<GameObject>("cd.prefab");
            cdCaseP = ab.LoadAsset<GameObject>("cd case.prefab");
            cd_icon = ab.LoadAsset<Sprite>("cd.png");
            rack_icon = ab.LoadAsset<Sprite>("rackicon.png");
            ab.Unload(false);

            rack10.name = "CD Rack(itemy)";
            LoadAssets.MakeGameObjectPickable(rack10);
            rack10.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
            rack10.AddComponent<CDRack>();
            rack10.SetActive(false);


            string[] dirs = Directory.GetDirectories(path);
            int i = 0;
            listOfCDs = new List<GameObject>();
            listOfCases = new List<GameObject>();
            foreach (string dir in dirs)
            {
                //Quaternion e = Quaternion.Euler(new Vector3(270, 0, 0));
                GameObject cd = (GameObject)Object.Instantiate(cdp);
                cd.layer = 0;
                GameObject cdCase = GameObject.Instantiate(cdCaseP);
                LoadAssets.MakeGameObjectPickable(cdCase);
                cd.name = "cd(itemy)";
                cdCase.name = "cd case(itemy)";
                cd.AddComponent<CD>().CDName = new DirectoryInfo(dir).Name;
                cdCase.AddComponent<CDCase>().CDName = new DirectoryInfo(dir).Name;
                if (i == dirs.Length - 1)
                    cdCase.GetComponent<CDCase>().cdp = this;
                if (File.Exists(Path.Combine(dir, "folder.txt")))
                {
                    string[] txtDirs = File.ReadAllLines(Path.Combine(dir, "folder.txt"));
                    cd.GetComponent<CD>().CDPath = Path.GetFullPath(txtDirs[0]);
                }
                else
                    cd.GetComponent<CD>().CDPath = Path.GetFullPath(dir);
                cd.GetComponent<CD>().cdCase = cdCase.GetComponent<CDCase>();

                cd.GetComponent<CD>().InCase();
                cd.transform.SetParent(cdCase.transform.GetChild(2), false);

                //Load coverart.png if exists, else leave default.
                if (File.Exists(Path.Combine(dir, "coverart.png")))
                {
                    Texture2D t2d = new Texture2D(1, 1);
                    t2d.LoadImage(File.ReadAllBytes(Path.Combine(dir, "coverart.png")));
                    cd.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCase.transform.GetChild(3).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCase.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                }
                listOfCDs.Add(cd);
                listOfCases.Add(cdCase);
                cdCase.SetActive(false);
                i++;
            }
            Load();
            if (GameObject.Find("Shop for mods") != null)
            {
                ModsShop.ShopItem shop;
                //Shop for mods is installed
                shop = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();
                if (!rack10.activeSelf)
                {
                    ModsShop.ProductDetails cdRack = new ModsShop.ProductDetails
                    {
                        productName = "Rack for 10 CDs",
                        multiplePurchases = false,
                        productCategory = "Accesories",
                        productIcon = rack_icon,
                        productPrice = 50
                    };
                    shop.Add(this, cdRack, ModsShop.ShopType.Teimo, BuyCDRack, rack10);
                }
                foreach (GameObject go in listOfCases)
                {
                    if (!go.activeSelf)
                    {
                        ModsShop.ProductDetails cases = new ModsShop.ProductDetails
                        {
                            productName = go.GetComponent<CDCase>().CDName,
                            multiplePurchases = false,
                            productCategory = "CDs",
                            productIcon = cd_icon,
                            productPrice = 100
                        };
                        shop.Add(this, cases, ModsShop.ShopType.Teimo, BuyCDs, go);
                    }
                }
            }
            else
            {
                //if no shop catalog installed.
                rack10.SetActive(true);
                foreach (GameObject go in listOfCases)
                    go.SetActive(true);
            }
            FindPlayer();
            if (GameObject.Find("cd(item1)") != null)
                GameObject.Find("cd(item1)").SetActive(false);
            if (GameObject.Find("cd case(item1)") != null)
                GameObject.Find("cd case(item1)").SetActive(false);
            if (GameObject.Find("cd(item2)") != null)
                GameObject.Find("cd(item2)").SetActive(false);
            if (GameObject.Find("cd case(item2)") != null)
                GameObject.Find("cd case(item2)").SetActive(false);
            if (GameObject.Find("cd(item3)") != null)
                GameObject.Find("cd(item3)").SetActive(false);
            if (GameObject.Find("cd case(item3)") != null)
                GameObject.Find("cd case(item3)").SetActive(false);
        }
        public void BuyCDs(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.desk;
            item.gameObject.SetActive(true);
            item.gameObject.GetComponent<CDCase>().purchased = true;

        }
        public void BuyCDRack(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.desk;
            item.gameObject.SetActive(true);
            item.gameObject.GetComponent<CDRack>().purchased = true;
        }
        public static void ResetPosition()
        {
            if (Application.loadedLevelName == "MainMenu")
            {
                ModUI.ShowMessage("Please use this when you are in game!", "Reset CD positions");
            }
            else
            {
                Quaternion e = Quaternion.Euler(new Vector3(0, 0, 0));
                foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (go.activeSelf)
                    {
                        if (go.name == "cd(itemy)")
                        {
                            if (!go.GetComponent<CD>().inPlayer && !go.GetComponent<CD>().inCase)
                            {
                                go.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
                                go.transform.rotation = e;
                            }
                        }
                        else if (go.name == "cd case(itemy)")
                        {
                            go.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
                            go.transform.rotation = e;
                        }
                        else if (go.name == "CD Rack(itemy)")
                        {
                            go.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
                            go.transform.rotation = e;
                        }
                    }
                }
            }
        }

        void FindPlayer()
        {
            List<Transform> players = Resources.FindObjectsOfTypeAll<Transform>().Where(x => x.name == "cd player(Clone)").ToList();

            foreach (Transform p in players)
            {
                //find correct cd player object
                if (p.parent.name != "Boxes" && p.parent.name != "Products")
                {
                    p.gameObject.AddComponent<CarCDPlayer>().cdplayer = this;
                    ModConsole.Print("<color=green>Your CD Player is now enchanced! Enjoy.</color>");
                }
            }
        }

        public void Load()
        {
            string old_path = Path.Combine(ModLoader.GetModConfigFolder(this), "case.save");
            string old_path2 = Path.Combine(ModLoader.GetModConfigFolder(this), "cd.save");

            if (File.Exists(old_path))
                File.Delete(old_path);
            if (File.Exists(old_path2))
                File.Delete(old_path2);

            CDSaveData data = SaveLoad.DeserializeSaveFile<CDSaveData>(this, "cdplayer.save");

            if (data != null)
            {
                if (data.rackpurchased)
                {
                    rack10.transform.position = data.rackpos;
                    rack10.transform.rotation = Quaternion.Euler(data.rackrotX, data.rackrotY, data.rackrotZ);
                    rack10.SetActive(true);
                    rack10.GetComponent<CDRack>().purchased = true;
                }
                foreach (GameObject go in listOfCases)
                {
                    for (int i = 0; i < data.cases.Count; i++)
                    {
                        if (data.cases[i].CDName == go.GetComponent<CDCase>().CDName && data.cases[i].purchased)
                        {
                            if (!data.cases[i].inRack)
                            {
                                go.transform.position = data.cases[i].pos;
                                go.transform.rotation = Quaternion.Euler(data.cases[i].rotX, data.cases[i].rotY, data.cases[i].rotZ);
                            }
                            else
                            {
                                go.GetComponent<Rigidbody>().isKinematic = true;
                                go.GetComponent<Rigidbody>().detectCollisions = false;
                                go.transform.SetParent(rack10.transform.GetChild(data.cases[i].inRackSlot), false);
                                go.transform.localPosition = Vector3.zero;
                                go.transform.localEulerAngles = Vector3.zero;
                                go.GetComponent<CDCase>().inRack = true;
                                go.GetComponent<CDCase>().inRackSlot = data.cases[i].inRackSlot;
                                go.name = "cd case (" + (data.cases[i].inRackSlot + 1).ToString() + ")(itemy)";
                            }
                            go.GetComponent<CDCase>().purchased = true;
                            go.SetActive(true);
                        }
                    }

                }
                foreach (GameObject go in listOfCDs)
                {
                    for (int i = 0; i < data.cds.Count; i++)
                    {
                        if (data.cds[i].CDName == go.GetComponent<CD>().CDName)
                        {
                            if (!data.cds[i].inCase)
                            {
                                go.GetComponent<CD>().inCase = false;
                                LoadAssets.MakeGameObjectPickable(go);
                                go.transform.SetParent(null);
                                go.transform.position = data.cds[i].pos;
                                go.transform.rotation = Quaternion.Euler(data.cds[i].rotX, data.cds[i].rotY, data.cds[i].rotZ);
                            }
                        }
                    }
                }
            }
        }

        // Update is called once per frame
        public override void Update()
        {
           /* if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                OnSave();
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                Load();
            }*/
        }
    }
}
