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
        public override string Name => "CDPlayer Enhanced";
        public override string Author => "Piotrulos";
        public override string Version => "1.4.5";
        public override string Description => "Makes adding CDs much easier, no renaming, no converting. (supports <color=orage>*.mp3, *.ogg, *.flac, *.wav, *.aiff</color>";

        private readonly string readme = $"This folder is used by CDPlayer Enhanced mod{System.Environment.NewLine}{System.Environment.NewLine}To create a new CD, create a new folder here, put your music or playlist file in that new folder.";

        public string path = Path.GetFullPath("CD");

        public static SettingsCheckBox bypassDis;
        public SettingsCheckBox debugInfo, RDSsim;
        public SettingsTextBox channel3url, channel4url;
        static List<GameObject> listOfCDs, listOfCases, listOfDisplayCases;

        GameObject rack10_d, cdCaseP_d, rack10, cdCaseP;
        public override void ModSetup()
        {
            SetupFunction(Setup.OnNewGame, CDPlayer_NewGame);
            SetupFunction(Setup.OnLoad, CDPlayer_OnLoad);
            SetupFunction(Setup.OnSave, CDPlayer_OnSave);
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
            Settings.AddText(this, "Disable distortion filter on satsuma amplifier speakers");
            bypassDis = Settings.AddCheckBox(this, "cdDisBypass", "Bypass distortion filters", false, FilterChange);
            Settings.AddText(this, "Respawn purchased stuff on kitchen table");
            Settings.AddButton(this, "resetcd", "Reset CDs", ResetPosition, new Color32(255, 66, 66, 255),  Color.white);
            Settings.AddHeader(this, "Internet radio settings", new Color32(0, 128, 0, 255));
            debugInfo = Settings.AddCheckBox(this, "debugInfo", "Show debug info", false);
            RDSsim = Settings.AddCheckBox(this, "RDSsim", "Simulate RDS", true);
            channel3url = Settings.AddTextBox(this, "ch3url", "Channel 3:", "http://185.33.21.112:11010", "Stream URL...");
            channel4url = Settings.AddTextBox(this, "ch4url", "Channel 4:", "http://185.33.21.112/90s_128", "Stream URL...");
        }

        void CDPlayer_OnSave()
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

            for (int i = 0; i < listOfCDs.Count; i++)
            {
                CDSaveDataList sdl = new CDSaveDataList
                {
                    pos = listOfCDs[i].transform.position,
                    rotX = listOfCDs[i].transform.rotation.eulerAngles.x,
                    rotY = listOfCDs[i].transform.rotation.eulerAngles.y,
                    rotZ = listOfCDs[i].transform.rotation.eulerAngles.z,
                    CDName = listOfCDs[i].GetComponent<CD>().CDName,
                    inCase = listOfCDs[i].GetComponent<CD>().inCase
                };
                if (listOfCDs[i].GetComponent<CD>().inPlayer)
                    sdl.inCase = true;
                sd.cds.Add(sdl);
            }
            for (int i = 0; i < listOfCases.Count; i++)
            {
                if (listOfCases[i].activeSelf)
                {
                    CaseSaveDataList sdl = new CaseSaveDataList
                    {
                        pos = listOfCases[i].transform.position,
                        rotX = listOfCases[i].transform.rotation.eulerAngles.x,
                        rotY = listOfCases[i].transform.rotation.eulerAngles.y,
                        rotZ = listOfCases[i].transform.rotation.eulerAngles.z,
                        CDName = listOfCases[i].GetComponent<CDCase>().CDName,
                        inRack = listOfCases[i].GetComponent<CDCase>().inRack,
                        inRackSlot = listOfCases[i].GetComponent<CDCase>().inRackSlot,
                        purchased = true
                    };

                    sd.cases.Add(sdl);
                }
            }
            SaveLoad.SerializeSaveFile(this, sd, "cdplayer.save");


        }

        //Called when mod is loading
        void CDPlayer_OnLoad()
        {
            AssetBundle ab = LoadAssets.LoadBundle(this, "cdplayer.unity3d");
            rack10 = GameObject.Instantiate(ab.LoadAsset<GameObject>("rack10.prefab"));
            rack10_d =ab.LoadAsset<GameObject>("rack10(display).prefab");
            cdCaseP = ab.LoadAsset<GameObject>("cd case.prefab");
            cdCaseP_d = ab.LoadAsset<GameObject>("cd case(display).prefab");
            ab.Unload(false);
            rack10.name = "CD Rack(rackz)";
            LoadAssets.MakeGameObjectPickable(rack10);
            rack10.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
            rack10.SetActive(false);

            if (!Directory.Exists(path)) //CD folder was renamed to CD1/2/3
                Directory.CreateDirectory(path);
            if (!File.Exists(Path.Combine(path, "CD Player Enhanced.txt")))
                File.WriteAllText(Path.Combine(path, "CD Player Enhanced.txt"), readme);
            string[] dirs = Directory.GetDirectories(path);
            listOfCDs = new List<GameObject>();
            listOfCases = new List<GameObject>();
            listOfDisplayCases = new List<GameObject>();
            for (int i = 0; i < dirs.Length; i++)
            {
                GameObject cdCase = GameObject.Instantiate(cdCaseP);
                GameObject cdCaseD = GameObject.Instantiate(cdCaseP_d);
                LoadAssets.MakeGameObjectPickable(cdCase);
                cdCase.name = "cd case(itemz)";
               // cd.GetComponent<CD>().CDName = new DirectoryInfo(dirs[i]).Name;
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
                    cdCase.transform.GetChild(3).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCase.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCaseD.transform.GetChild(1).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCaseD.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                }
                listOfCDs.Add(cd.gameObject);
                listOfCases.Add(cdCase);
                listOfDisplayCases.Add(cdCaseD);
                cdCase.SetActive(false);
            }
            Load();
            if (ModLoader.IsModPresent("ModsShop"))
            {
                ModsShop.Shop shop;
                //Shop for mods is installed
                shop = ModsShop.ModsShop.GetShopReference();
                if (!rack10.activeSelf)
                {
                    ModsShop.ItemDetails item = shop.CreateShopItem(this, "rack1", "Rack for 10 CDs", 50, false, BuyCDRack, rack10, ModsShop.SpawnMethod.SetActive);
                    shop.AddDisplayItem(item, rack10_d, ModsShop.SpawnMethod.Instantiate);
                }
                for (int i = 0; i < listOfCases.Count; i++)
                {
                    GameObject go = listOfCases[i];
                    if (!go.activeSelf)
                    {
                        ModsShop.ItemDetails item = shop.CreateShopItem(this, $"cd{i}", $"[CD] {go.GetComponent<CDCase>().CDName}", 100, false, BuyCDs, go, ModsShop.SpawnMethod.SetActive);
                        shop.AddDisplayItem(item, listOfDisplayCases[i], ModsShop.SpawnMethod.SetActive, new Vector3(0, -90, 0), 0);
                    }
                }
            }
            else
            {
                //if no shop installed.
                rack10.SetActive(true);
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
        public void BuyCDs(ModsShop.Checkout item)
        {
            item.gameObject.GetComponent<CDCase>().purchased = true;
        }
        public void BuyCDRack(ModsShop.Checkout item)
        {
            item.gameObject.GetComponent<CDRack>().purchased = true;
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
                if (rack10.activeSelf)
                {
                    rack10.transform.position = new Vector3(-10.2f, 0.17f, 6.47f);
                    rack10.transform.transform.localEulerAngles = Vector3.zero;
                }
                ModUI.ShowMessage("CDs should be now on kitchen table.", "Reset CD positions");
            }
        }

        void FindPlayer()
        {
            List<Transform> players = Resources.FindObjectsOfTypeAll<Transform>().Where(x => x.name == "cd player(Clone)").ToList();

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].parent != null)
                {
                    if (players[i].parent.name != "Boxes" && players[i].parent.name != "Products")
                    {
                        players[i].Find("Sled/cd_sled_pivot").gameObject.AddComponent<CarCDPlayer>().cdplayer = this;
                        ModConsole.Print("<color=green>Your CD Player is now enhanced! Enjoy.</color>");
                    }
                }
                else
                {
                    players[i].Find("Sled/cd_sled_pivot").gameObject.AddComponent<CarCDPlayer>().cdplayer = this;
                    ModConsole.Print("<color=green>Your CD Player is now enhanced! Enjoy.</color>");
                }
            }
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
                    rack10.transform.position = data.rackpos;
                    rack10.transform.eulerAngles = new Vector3(data.rackrotX, data.rackrotY, data.rackrotZ);
                    rack10.SetActive(true);
                    rack10.GetComponent<CDRack>().purchased = true;
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
                            listOfCases[i].transform.SetParent(rack10.transform.GetChild(cas.inRackSlot), false);
                            listOfCases[i].transform.localPosition = Vector3.zero;
                            listOfCases[i].transform.localEulerAngles = Vector3.zero;
                            listOfCases[i].GetComponent<CDCase>().inRack = true;
                            listOfCases[i].GetComponent<CDCase>().inRackSlot = cas.inRackSlot;
                            listOfCases[i].name = "cd case (" + (cas.inRackSlot + 1).ToString() + ")(itemy)";
                        }
                        listOfCases[i].GetComponent<CDCase>().purchased = true;
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
}
