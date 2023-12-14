using MSCLoader;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static HutongGames.PlayMaker.Actions.ConvertCase;

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

#if !Mini
    public class CDPlayer : Mod
    {

        public override string ID => "CDPlayer";
        public override string Name => "CDPlayer Enhanced";
        public override string Author => "Piotrulos";
        public override string Version => "1.5.2";
        public override string Description => "Makes adding CDs much easier, no renaming, no converting. (supports <color=orage>*.mp3, *.ogg, *.flac, *.wav, *.aiff</color>";

        private readonly string readme = $"This folder is used by CDPlayer Enhanced mod{System.Environment.NewLine}{System.Environment.NewLine}To create a new CD, create a new folder here, put your music or playlist file in that new folder.";

        public string path = Path.GetFullPath("CD");

        public static SettingsCheckBox bypassDis;
        public SettingsCheckBox debugInfo, RDSsim;
        public SettingsTextBox channel3url, channel4url;
        static List<GameObject> listOfCDs, listOfCases, listOfDisplayCases, listOfRacks;

        GameObject rack10_d, cdCaseP_d, cdCaseP, rack10P;
        public override void ModSetup()
        {
            SetupFunction(Setup.OnNewGame, CDPlayer_NewGame);
            SetupFunction(Setup.OnMenuLoad, CDPlayer_OnMenuLoad);
            SetupFunction(Setup.OnLoad, CDPlayer_OnLoad);
            SetupFunction(Setup.OnSave, CDPlayer_OnSave);
           // SetupFunction(Setup.Update, test);
        }
        void CDPlayer_OnMenuLoad()
        {
            GameObject r = GameObject.Find("Radio");
            r.transform.Find("CD").GetComponent<PlayMakerFSM>().enabled = false;
            GameObject text = GameObject.Find("Interface/Songs/LoadingCD");
            if (ModLoader.IsModPresent("Toiveradio_Enhanced"))
            {
                GameObject.Find("Interface/Songs/Button").GetComponent<BoxCollider>().enabled = false;
                text.GetComponent<PlayMakerFSM>().enabled = false;
                text.GetComponent<TextMesh>().text = "FULLY MODDED (NO IMPORT NEEDED)";
            }
            else
            {
                text.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Text").Value = "CD IS MODDED (NO IMPORT NEEDED)";
            }    
        }
        void test()
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                ModConsole.Warning("s");
                CDPlayer_OnSave();
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                ModConsole.Warning("l");
                LoadUnifiedSave();
            }
        }
        void CDPlayer_NewGame()
        {
            string savepath = Path.Combine(ModLoader.GetModSettingsFolder(this), "cdplayer.save");
            if (File.Exists(savepath))
                File.Delete(savepath);
        }
        public static void FilterChange()
        {
            if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/CDPlayer") != null)
                GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/CDPlayer").GetComponent<AudioSource>().bypassEffects = bypassDis.GetValue();
        }
        public override void ModSettings()
        {
            Settings.AddHeader(this, "CD player settings", new Color32(0, 128, 0, 255));
            Settings.AddButton(this, "Open CD folder", delegate { Process.Start(Path.GetFullPath("CD")); }, Color.black, Color.white);
            Settings.AddText(this, "Disable distortion filter on satsuma amplifier speakers");
            bypassDis = Settings.AddCheckBox(this, "cdDisBypass", "Bypass distortion filter on aftermarket speakers", false, FilterChange);
            Settings.AddHeader(this, "Reset Settings");
            Settings.AddText(this, "Respawn purchased stuff on kitchen table");
            Settings.AddButton(this, "resetcd", "Reset CDs", ResetPosition, Color.black, Color.white);
            Settings.AddHeader(this, "Internet radio settings", new Color32(0, 128, 0, 255));
            debugInfo = Settings.AddCheckBox(this, "debugInfo", "Show debug info", false);
            RDSsim = Settings.AddCheckBox(this, "RDSsim", "Simulate RDS", true);
            Settings.AddText(this, "REMINDER! Only links starting with http:// will work. <b>https:// is not supported.</b>");
            channel3url = Settings.AddTextBox(this, "ch3url", "Channel 3:", "http://185.33.21.112:11010", "Stream URL...");
            channel4url = Settings.AddTextBox(this, "ch4url", "Channel 4:", "http://185.33.21.112/90s_128", "Stream URL...");
        }
        void LoadUnifiedSave()
        {
            CDPSaveData save = SaveLoad.DeserializeClass<CDPSaveData>(this, "SaveData", true);
            if (save == null) return;
            for (int i = 0; i < save.goSaveList.Count; i++)
            {
                CDPSaveList cdsavelist = save.goSaveList[i];
                GameObject go = null;
                switch (cdsavelist.goType)
                {
                    case 0:
                        go = listOfCDs.Where(x => x.GetComponent<CD>().CDName == cdsavelist.CDName).FirstOrDefault();
                        break;
                    case 1:
                        go = listOfCases.Where(x => x.GetComponent<CDCase>().CDName == cdsavelist.CDName).FirstOrDefault();
                        break;
                    case 2:
                        go = GameObject.Instantiate(rack10P);                        
                        go.GetComponent<CDRack>().rackNr = cdsavelist.rackID;
                        go.name = "CD Rack(rackz)";
                        listOfRacks.Add(go);
                        break;
                }
                if (go != null)
                {
                    if (cdsavelist.rackID != -1 && cdsavelist.RackSlot != 255)
                    {
                        go.GetComponent<Rigidbody>().isKinematic = true;
                        go.GetComponent<Rigidbody>().detectCollisions = false;
                        go.transform.SetParent(listOfRacks.Where(x=>x.GetComponent<CDRack>().rackNr == cdsavelist.rackID).FirstOrDefault().transform.GetChild(cdsavelist.RackSlot), false);
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localEulerAngles = Vector3.zero;
                        go.GetComponent<CDCase>().inRack = true;
                        go.GetComponent<CDCase>().inRackSlot = cdsavelist.RackSlot;
                        go.GetComponent<CDCase>().inRackNr = cdsavelist.rackID;
                        go.name = "cd case (" + (cdsavelist.RackSlot + 1).ToString() + ")(itemz)";
                    }
                    else
                    {
                        go.transform.position = new Vector3(cdsavelist.posX, cdsavelist.posY, cdsavelist.posZ);
                        go.transform.eulerAngles = new Vector3(cdsavelist.rotX, cdsavelist.rotY, cdsavelist.rotZ);
                        go.transform.parent = null;
                    }
                    go.MakePickable();
                    go.SetActive(true);
                }
            }
        }
        void CDPlayer_OnSave()
        {
            CDPSaveData save = new CDPSaveData();
            for (int i = 0; i < listOfRacks.Count; i++)
            {
                CDPSaveList csl = new CDPSaveList()
                {
                    goType = 2,
                    rackID = listOfRacks[i].GetComponent<CDRack>().rackNr,
                    posX = listOfRacks[i].transform.position.x,
                    posY = listOfRacks[i].transform.position.y,
                    posZ = listOfRacks[i].transform.position.z,
                    rotX = listOfRacks[i].transform.localEulerAngles.x,
                    rotY = listOfRacks[i].transform.localEulerAngles.y,
                    rotZ = listOfRacks[i].transform.localEulerAngles.z,

                };
                save.goSaveList.Add(csl);

            }
            for (int i = 0; i < listOfCases.Count; i++)
            {
                if (listOfCases[i].activeSelf)
                {
                    if (listOfCases[i].GetComponent<CDCase>().inRack)
                    {
                        CDPSaveList csl = new CDPSaveList()
                        {
                            goType = 1,
                            CDName = listOfCases[i].GetComponent<CDCase>().CDName,
                            rackID = listOfCases[i].GetComponent<CDCase>().inRackNr,
                            RackSlot = (byte)listOfCases[i].GetComponent<CDCase>().inRackSlot
                        };
                        save.goSaveList.Add(csl);
                    }
                    else
                    {
                        CDPSaveList csl = new CDPSaveList()
                        {
                            goType = 1,
                            CDName = listOfCases[i].GetComponent<CDCase>().CDName,
                            posX = listOfCases[i].transform.position.x,
                            posY = listOfCases[i].transform.position.y,
                            posZ = listOfCases[i].transform.position.z,
                            rotX = listOfCases[i].transform.localEulerAngles.x,
                            rotY = listOfCases[i].transform.localEulerAngles.y,
                            rotZ = listOfCases[i].transform.localEulerAngles.z,

                        };
                        save.goSaveList.Add(csl);

                    }
                }
            }
            for (int i = 0; i < listOfCDs.Count; i++)
            {
                if (listOfCDs[i].transform.parent == null)
                {
                    CDPSaveList csl = new CDPSaveList()
                    {
                        goType = 0,
                        CDName = listOfCDs[i].GetComponent<CD>().CDName,
                        posX = listOfCDs[i].transform.position.x,
                        posY = listOfCDs[i].transform.position.y,
                        posZ = listOfCDs[i].transform.position.z,
                        rotX = listOfCDs[i].transform.localEulerAngles.x,
                        rotY = listOfCDs[i].transform.localEulerAngles.y,
                        rotZ = listOfCDs[i].transform.localEulerAngles.z,

                    };
                    save.goSaveList.Add(csl);

                }
            }
            SaveLoad.SerializeClass(this, save, "SaveData", true);
        }

        //Called when mod is loading
        void CDPlayer_OnLoad()
        {
            AssetBundle ab = LoadAssets.LoadBundle(this, "cdplayer.unity3d");
            rack10P = ab.LoadAsset<GameObject>("rack10.prefab");
            rack10_d =ab.LoadAsset<GameObject>("rack10(display).prefab");
            cdCaseP = ab.LoadAsset<GameObject>("cd case.prefab");
            cdCaseP_d = ab.LoadAsset<GameObject>("cd case(display).prefab");
            ab.Unload(false);

            if (!Directory.Exists(path)) //CD folder was renamed to CD1/2/3
                Directory.CreateDirectory(path);
            if (!File.Exists(Path.Combine(path, "CD Player Enhanced.txt")))
                File.WriteAllText(Path.Combine(path, "CD Player Enhanced.txt"), readme);
            string[] dirs = Directory.GetDirectories(path);
            listOfCDs = new List<GameObject>();
            listOfCases = new List<GameObject>();
            listOfDisplayCases = new List<GameObject>();
            listOfRacks = new List<GameObject>();
            for (int i = 0; i < dirs.Length; i++)
            {
                GameObject cdCase = GameObject.Instantiate(cdCaseP);
                GameObject cdCaseD = GameObject.Instantiate(cdCaseP_d);
                LoadAssets.MakeGameObjectPickable(cdCase);
                cdCase.name = "cd case(itemz)";
                cdCase.GetComponent<CDCase>().CDName = new DirectoryInfo(dirs[i]).Name;
                CD cd = cdCase.GetComponent<CDCase>().cdt.transform.GetChild(0).GetComponent<CD>();
                cd.CDName = new DirectoryInfo(dirs[i]).Name;

                string[] pls = Directory.GetFiles(dirs[i], "*.*").Where(file => file.ToLower().EndsWith(".m3u", System.StringComparison.OrdinalIgnoreCase) ||
                                                                            file.ToLower().EndsWith(".m3u8", System.StringComparison.OrdinalIgnoreCase) ||
                                                                            file.ToLower().EndsWith(".pls", System.StringComparison.OrdinalIgnoreCase)).ToArray();
                if (pls.Length > 0)
                {
                    cd.isPlaylist = true;
                    cd.CDPath = Path.GetFullPath(pls[0]);
                }
                else
                {
                    if (File.Exists(Path.Combine(dirs[i], "folder.txt")))
                    {
                        string[] txtDirs = File.ReadAllLines(Path.Combine(dirs[i], "folder.txt"));
                        cd.CDPath = Path.GetFullPath(txtDirs[0]);
                    }
                    else
                        cd.CDPath = Path.GetFullPath(dirs[i]);
                }
                cd.cdCase = cdCase.GetComponent<CDCase>();
                cd.InCase();

                //Load coverart.png if exists, else leave default.
                if (File.Exists(Path.Combine(dirs[i], "coverart.png")))
                {
                    Texture2D t2d = new Texture2D(1, 1);
                    t2d.LoadImage(File.ReadAllBytes(Path.Combine(dirs[i], "coverart.png")));
                    cd.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCase.GetComponent<CDCase>().ChangeLabels(t2d);
                    cdCaseD.transform.GetChild(1).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCaseD.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                }
                listOfCDs.Add(cd.gameObject);
                listOfCases.Add(cdCase);
                listOfDisplayCases.Add(cdCaseD);
                cdCase.SetActive(false);
            }
            if (SaveLoad.ValueExists(this, "SaveData"))
            {
                LoadUnifiedSave();
            }
            else
            {
                Load();
            }
            if (ModLoader.IsModPresent("ModsShop"))
            {
                //if shop is installed
                SetupShop();
            }
            else
            {
                //if no shop installed.
                for (int i = 0; i < listOfCases.Count; i++)
                {
                    listOfCases[i].SetActive(true);
                }
            }
            FindPlayer();
            //disable OG cds
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
        void SetupShop()
        {
            ModsShop.Shop shop;
            shop = ModsShop.ModsShop.GetShopReference();
            ModsShop.ItemDetails rackitem = shop.CreateShopItem(this, "rack1", "Rack for 10 CDs", 50, true, BuyCDRack, rack10P, ModsShop.SpawnMethod.Instantiate);
            shop.AddDisplayItem(rackitem, rack10_d, ModsShop.SpawnMethod.Instantiate);

            for (int i = 0; i < listOfCases.Count; i++)
            {
                GameObject go = listOfCases[i];
                if (!go.activeSelf)
                {
                    ModsShop.ItemDetails item = shop.CreateShopItem(this, $"cd{i}", $"[CD] {go.GetComponent<CDCase>().CDName}", 100, false, BuyCDs, go, ModsShop.SpawnMethod.SetActive);
                    if(i==0)
                        shop.AddDisplayItem(item, listOfDisplayCases[i], ModsShop.SpawnMethod.SetActive, new Vector3(0, -90, 0), 2);
                    else
                        shop.AddDisplayItem(item, listOfDisplayCases[i], ModsShop.SpawnMethod.SetActive, new Vector3(0, -90, 0), 0);

                }
            }
        }
        void BuyCDs(ModsShop.Checkout item)
        {
            //item.gameObject.GetComponent<CDCase>().purchased = true;
        }
        void BuyCDRack(ModsShop.Checkout item)
        {
            item.gameObject.GetComponent<CDRack>().rackNr = listOfRacks.Count;
            item.gameObject.MakePickable();
            item.gameObject.name = "CD Rack(rackz)";
            listOfRacks.Add(item.gameObject);

        }
        public void ResetPosition()
        {
            if (Application.loadedLevelName == "MainMenu")
            {
                ModUI.ShowMessage("Please use this when you are in game!", "Reset CD positions");
            }
            else
            {
                for (int i = 0; i < listOfCases.Count; i++)
                {
                    if (!listOfCases[i].activeSelf)
                        continue;
                    listOfCases[i].transform.position = new Vector3(-10f, 0.16f, 6.28f);
                    listOfCases[i].transform.localEulerAngles = Vector3.zero;
                }
                for (int i = 0; i < listOfCDs.Count; i++)
                {
                    if (!listOfCDs[i].activeSelf)
                        continue;
                    if (!listOfCDs[i].GetComponent<CD>().inPlayer && !listOfCDs[i].GetComponent<CD>().inCase)
                    {
                        listOfCDs[i].transform.position = new Vector3(-10.1f, 0.16f, 6.28f);
                        listOfCDs[i].transform.localEulerAngles = Vector3.zero;
                    }
                }
                for (int i = 0; i < listOfRacks.Count; i++)
                {
                    listOfRacks[i].transform.position = new Vector3(-10.2f, 0.17f, 6.47f);
                    listOfRacks[i].transform.transform.localEulerAngles = Vector3.zero;
                }
                ModUI.ShowMessage("CDs should be now on kitchen table.", "Reset CD positions");
            }
        }

        void FindPlayer()
        {
            PlayMakerFSM cdp = GameObject.Find("Database/DatabaseOrders/CD_player").GetComponent<PlayMakerFSM>();
           // cdp.FsmVariables.FindFsmBool("Purchased");
            cdp.FsmVariables.FindFsmGameObject("ThisPart").Value.transform.Find("Sled/cd_sled_pivot").gameObject.AddComponent<CarCDPlayer>().cdplayer = this;
            ModConsole.Print("<color=green>Your CD Player is now enhanced! Enjoy.</color>");
        }

        public void Load()
        {
            string old_path = Path.Combine(ModLoader.GetModSettingsFolder(this), "case.save");
            string old_path2 = Path.Combine(ModLoader.GetModSettingsFolder(this), "cd.save");

            if (File.Exists(old_path))
                File.Delete(old_path);
            if (File.Exists(old_path2))
                File.Delete(old_path2);

            CDSaveData data = SaveLoad.DeserializeSaveFile<CDSaveData>(this, "cdplayer.save");

            if (data != null)
            {
                if (data.rackpurchased)
                {
                    GameObject rack10 = GameObject.Instantiate(rack10P);
                    rack10.transform.position = data.rackpos;
                    rack10.name = "CD Rack(rackz)";
                    rack10.transform.eulerAngles = new Vector3(data.rackrotX, data.rackrotY, data.rackrotZ);
                    rack10.SetActive(true);
                    rack10.MakePickable();
                    listOfRacks.Add(rack10);
                }
                for (int i = 0; i < listOfCases.Count; i++)
                {
                    CaseSaveDataList cas = data.cases.Where(x => x.CDName == listOfCases[i].GetComponent<CDCase>().CDName && x.purchased).FirstOrDefault();
                    if (cas != null)
                    {
                        if (!cas.inRack)
                        {
                            listOfCases[i].transform.position = cas.pos;
                            listOfCases[i].transform.eulerAngles = new Vector3(cas.rotX, cas.rotY, cas.rotZ);
                        }
                        else
                        {
                            listOfCases[i].GetComponent<Rigidbody>().isKinematic = true;
                            listOfCases[i].GetComponent<Rigidbody>().detectCollisions = false;
                            listOfCases[i].transform.SetParent(listOfRacks[0].transform.GetChild(cas.inRackSlot), false);
                            listOfCases[i].transform.localPosition = Vector3.zero;
                            listOfCases[i].transform.localEulerAngles = Vector3.zero;
                            listOfCases[i].GetComponent<CDCase>().inRack = true;
                            listOfCases[i].GetComponent<CDCase>().inRackSlot = cas.inRackSlot;
                            listOfCases[i].name = "cd case (" + (cas.inRackSlot + 1).ToString() + ")(itemy)";
                        }
                      //  listOfCases[i].GetComponent<CDCase>().purchased = true;
                        listOfCases[i].SetActive(true);
                    }
                }
                for (int i = 0; i < listOfCDs.Count; i++)
                {
                    CDSaveDataList cds = data.cds.Where(x => x.CDName == listOfCDs[i].GetComponent<CD>().CDName).FirstOrDefault();
                    if (cds != null)
                    {
                        if (cds.CDName == listOfCDs[i].GetComponent<CD>().CDName)
                        {
                            if (!cds.inCase)
                            {
                                listOfCDs[i].GetComponent<CD>().inCase = false;
                                LoadAssets.MakeGameObjectPickable(listOfCDs[i]);
                                listOfCDs[i].transform.SetParent(null);
                                listOfCDs[i].transform.position = cds.pos;
                                listOfCDs[i].transform.eulerAngles = new Vector3(cds.rotX, cds.rotY, cds.rotZ);
                            }
                        }
                    }
                }
            }
        }
    }
#endif

}
