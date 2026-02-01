using MSCLoader;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CDPlayer;

#if !Mini
public class CDPlayer : Mod
{

    public override string ID => "CDPlayer";
    public override string Name => "CDPlayer Enhanced";
    public override string Author => "piotrulos";
    public override string Version => "1.6.4";
    public override string Description => "Makes adding CDs much easier, no renaming, no converting. (supports <color=orage>*.mp3, *.ogg, *.flac, *.wav, *.aiff</color>";
    public override Game SupportedGames => Game.MySummerCar_And_MyWinterCar;

    private readonly string readme = $"This folder is used by CDPlayer Enhanced mod{System.Environment.NewLine}{System.Environment.NewLine}To create a new CD, create a new folder here, put your music or playlist file in that new folder.";

    public string path = Path.GetFullPath("CD");

    public static SettingsCheckBox bypassDis;
    public SettingsCheckBox debugInfo, RDSsim;
    public SettingsTextBox channel3url, channel4url;
    static List<GameObject> listOfCDs, listOfCases, listOfDisplayCases, listOfRacks;

    // GameObject rack10_d, cdCaseP_d, cdCaseP, rack10P;
    AssetRefs assets;
    bool cdloadedinplayer = false;

    public override void ModSetup()
    {
        SetupFunction(Setup.OnMenuLoad, CDPlayer_OnMenuLoad);
        SetupFunction(Setup.OnLoad, CDPlayer_OnLoad);
        SetupFunction(Setup.OnSave, CDPlayer_OnSave);
        SetupFunction(Setup.ModSettings, CDPlayer_Settings);
        SetupFunction(Setup.Update, test);

        //MWC
        //"SORBET(190-200psi)/Functions/Radio/SoundSpeaker/CDAudioSourceSorbett"
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/Sled/cd_sled_pivot"
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/DiscTriggerPlayer5" string HandChild
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/ButtonsCD/Eject"
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/ButtonsCD/TrackChannelSwitch"
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/ButtonsCD/RadioCDSwitch"
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/ButtonsCD/CDplrVolume"

        //"HOMENEW/Functions/FunctionsDisable/Stereos/Player/Sled/cd_sled_pivot/stereos_sled"
    }
    void CDPlayer_OnMenuLoad()
    {
        GameObject r = GameObject.Find("Radio");
        GameObject text = GameObject.Find("Interface/Songs/LoadingCD");
        if (ModLoader.IsModPresent("Toiveradio_Enhanced"))
        {
            GameObject.Find("Interface/Songs/Button").GetComponent<BoxCollider>().enabled = false;
            text.GetComponent<PlayMakerFSM>().enabled = false;
            text.GetComponent<TextMesh>().text = "FULLY MODDED (NO IMPORT NEEDED)";
        }
        else
        {
            r.transform.Find("CD").GetComponent<PlayMakerFSM>().enabled = false;
            text.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Text").Value = "CD IS MODDED (NO IMPORT NEEDED)";
        }
        if (!Directory.Exists(path)) //Ceate CD directory
            Directory.CreateDirectory(path);
        if (!File.Exists(Path.Combine(path, "CD Player Enhanced.txt")))
            File.WriteAllText(Path.Combine(path, "CD Player Enhanced.txt"), readme);
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

    public static void FilterChange()
    {
        if (ModLoader.CurrentGame == Game.MyWinterCar)
        {
            if (GameObject.Find("CORRIS").transform.Find("Functions/Radio/SoundSpeakerBass/CDAudioSourceCorris") != null)
                GameObject.Find("CORRIS").transform.Find("Functions/Radio/SoundSpeakerBass/CDAudioSourceCorris").GetComponent<AudioSource>().bypassEffects = bypassDis.GetValue();
            return;
        }

        if (GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Electricity/SpeakerBass/CDPlayer") != null)
            GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Electricity/SpeakerBass/CDPlayer").GetComponent<AudioSource>().bypassEffects = bypassDis.GetValue();
    }
    private void CDPlayer_Settings()
    {
        Settings.AddHeader("CD player settings", new Color32(0, 128, 0, 255));
        Settings.AddButton("Open CD folder", delegate { Process.Start(Path.GetFullPath("CD")); }, Color.black, Color.white, SettingsButton.ButtonIcon.Folder);
        if (ModLoader.CurrentGame == Game.MyWinterCar)
            Settings.AddText("Disable distortion filter on corris subwoofer speakers");
        if (ModLoader.CurrentGame == Game.MySummerCar)
            Settings.AddText("Disable distortion filter on satsuma subwoofer speakers");
        bypassDis = Settings.AddCheckBox("cdDisBypass", "Bypass distortion filter on aftermarket speakers", false, FilterChange);
        Settings.AddHeader("Reset Settings");
        Settings.AddText("Respawn purchased stuff on kitchen table");
        Settings.AddButton("Reset CDs", ResetPosition, Color.black, Color.white);
        Settings.AddHeader("Internet radio settings", new Color32(0, 128, 0, 255));
        debugInfo = Settings.AddCheckBox("debugInfo", "Show debug info", false);
        RDSsim = Settings.AddCheckBox("RDSsim", "Simulate RDS", true);
        Settings.AddText("REMINDER! Only links starting with http:// will work. <b>https:// is not supported.</b>");
        channel3url = Settings.AddTextBox("ch3url", "Channel 3:", "http://185.33.21.112:11010", "Stream URL...");
        channel4url = Settings.AddTextBox("ch4url", "Channel 4:", "http://185.33.21.112/90s_128", "Stream URL...");
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
                    go = GameObject.Instantiate(assets.rack10);
                    go.GetComponent<CDRack>().rackNr = cdsavelist.rackID;
                    go.name = "CD Rack(rackz)";
                    listOfRacks.Add(go);
                    break;
            }
            if (go != null)
            {
                if (cdsavelist.inPlayer && cdsavelist.goType == 0 && !cdloadedinplayer)
                {
                    if (ModLoader.CurrentGame == Game.MyWinterCar) continue;
                    Transform cdpl = GameObject.Find("Database/DatabaseOrders/CD_player").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmGameObject("ThisPart").Value.transform.Find("Sled/cd_sled_pivot");
                    go.GetComponent<CD>().inPlayer = true;
                    go.GetComponent<Rigidbody>().detectCollisions = false;
                    go.transform.SetParent(cdpl, false);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    cdloadedinplayer = true;
                    continue;
                }
                if (cdsavelist.rackID != -1 && cdsavelist.RackSlot != 255)
                {
                    GameObject rack = listOfRacks.Where(x => x.GetComponent<CDRack>().rackNr == cdsavelist.rackID).FirstOrDefault();
                    if (rack != null)
                        go.GetComponent<CDCase>().LoadInRack(rack, cdsavelist.RackSlot, cdsavelist.rackID);

                }
                else
                {
                    go.transform.position = new Vector3(cdsavelist.posX, cdsavelist.posY, cdsavelist.posZ);
                    go.transform.eulerAngles = new Vector3(cdsavelist.rotX, cdsavelist.rotY, cdsavelist.rotZ);
                    go.transform.parent = null;
                    go.GetComponent<Rigidbody>().isKinematic = false;
                    go.GetComponent<Rigidbody>().detectCollisions = true;
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
            CDPSaveList csl = new CDPSaveList(2, listOfRacks[i].GetComponent<CDRack>().rackNr, listOfRacks[i].transform.position, listOfRacks[i].transform.localEulerAngles);
            save.goSaveList.Add(csl);
        }
        for (int i = 0; i < listOfCases.Count; i++)
        {
            if (!listOfCases[i].activeSelf) continue;

            if (listOfCases[i].GetComponent<CDCase>().inRack)
            {
                CDPSaveList csl = new CDPSaveList(1, listOfCases[i].GetComponent<CDCase>().CDName, listOfCases[i].GetComponent<CDCase>().inRackNr, listOfCases[i].GetComponent<CDCase>().inRackSlot);
                save.goSaveList.Add(csl);
            }
            else
            {
                CDPSaveList csl = new CDPSaveList(1, listOfCases[i].GetComponent<CDCase>().CDName, listOfCases[i].transform.position, listOfCases[i].transform.localEulerAngles);
                save.goSaveList.Add(csl);
            }

        }
        for (int i = 0; i < listOfCDs.Count; i++)
        {
            if (listOfCDs[i].transform.parent == null)
            {
                CDPSaveList csl = new CDPSaveList(0, listOfCDs[i].GetComponent<CD>().CDName, listOfCDs[i].transform.position, listOfCDs[i].transform.localEulerAngles);
                save.goSaveList.Add(csl);
            }
            else if (listOfCDs[i].GetComponent<CD>().inPlayer)
            {
                CDPSaveList csl = new CDPSaveList(0, listOfCDs[i].GetComponent<CD>().CDName, Vector3.zero, Vector3.zero);
                csl.inPlayer = true;
                save.goSaveList.Add(csl);
            }
        }
        SaveLoad.SerializeClass(this, save, "SaveData", true);
        string old_path = Path.Combine(ModLoader.GetModSettingsFolder(this), "cdplayer.save");
        if (File.Exists(old_path))
            File.Delete(old_path);
    }

    //Called when mod is loading
    void CDPlayer_OnLoad()
    {
        AssetBundle ab = LoadAssets.LoadBundle(this, "cdplayer.unity3d");
        assets = ab.LoadAsset<GameObject>("cdprefabs.prefab").GetComponent<AssetRefs>();
        ab.Unload(false);

        LabelGenerator labelGenerator = GameObject.Instantiate(assets.labelGenerator).GetComponent<LabelGenerator>();
        labelGenerator.transform.position = new Vector3(0f, -10f, 0f);
        string[] dirs = Directory.GetDirectories(path);
        listOfCDs = new List<GameObject>();
        listOfCases = new List<GameObject>();
        listOfDisplayCases = new List<GameObject>();
        listOfRacks = new List<GameObject>();

        for (int i = 0; i < dirs.Length; i++)
        {
            GameObject cdCaseObj = GameObject.Instantiate(assets.CDCase);
            GameObject cdCaseDObj = GameObject.Instantiate(assets.cdCaseD);
            LoadAssets.MakeGameObjectPickable(cdCaseObj);
            cdCaseObj.name = "cd case(itemz)";
            CDCase cdCase = cdCaseObj.GetComponent<CDCase>();
            cdCase.CDName = new DirectoryInfo(dirs[i]).Name;
            CD cd = cdCase.cd;
            cd.CDName = new DirectoryInfo(dirs[i]).Name;

            string[] pls = [.. Directory.GetFiles(dirs[i], "*.*").Where(file => file.ToLower().EndsWith(".m3u", System.StringComparison.OrdinalIgnoreCase) ||
                                                                        file.ToLower().EndsWith(".m3u8", System.StringComparison.OrdinalIgnoreCase) ||
                                                                        file.ToLower().EndsWith(".pls", System.StringComparison.OrdinalIgnoreCase))];
            if (pls.Length > 0)
            {
                cd.isPlaylist = true;
                cd.CDPath = Path.GetFullPath(pls[0]);
            }
            else
            {
                cd.CDPath = Path.GetFullPath(dirs[i]);
            }
            cd.LoadTrackData();
            //Load coverart.png if exists, else leave default.
            if (File.Exists(Path.Combine(dirs[i], "coverart.png")))
            {
                Texture2D t2d = new Texture2D(1, 1);
                t2d.LoadImage(File.ReadAllBytes(Path.Combine(dirs[i], "coverart.png")));
                //  cd.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                cdCase.ChangeLabels(t2d);
                cdCaseDObj.transform.GetChild(1).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                cdCaseDObj.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
            }
            else
            {
                cdCase.SetTextLabels(labelGenerator);
            }
            listOfCDs.Add(cd.gameObject);
            listOfCases.Add(cdCaseObj);
            listOfDisplayCases.Add(cdCaseDObj);
            cdCaseObj.SetActive(false);
            cdCaseDObj.SetActive(false);
        }
        if (ModLoader.CurrentGame == Game.MyWinterCar)
            FindCDPlayers_MWC();
        if (ModLoader.CurrentGame == Game.MySummerCar)
            FindPlayer_MSC();
        if (SaveLoad.ValueExists(this, "SaveData"))
        {
            LoadUnifiedSave();
        }
        if (cdloadedinplayer)
        {
            if (ModLoader.CurrentGame == Game.MySummerCar)
            {
                Transform cdpl = GameObject.Find("Database/DatabaseOrders/CD_player").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmGameObject("ThisPart").Value.transform.Find("Sled/cd_sled_pivot");
                cdpl.GetComponent<CarCDPlayer>().LoadCDFromSave();
            }
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
        //disable OG cds (probably not needed anymore since import is disabled)
        GameObject.Find("cd(item1)")?.SetActive(false);
        GameObject.Find("cd case(item1)")?.SetActive(false);
        GameObject.Find("cd(item2)")?.SetActive(false);
        GameObject.Find("cd case(item2)")?.SetActive(false);
        GameObject.Find("cd(item3)")?.SetActive(false);
        GameObject.Find("cd case(item3)")?.SetActive(false);
    }
    void SetupShop()
    {
        int cdPrice = 100;
        if (ModLoader.CurrentGame == Game.MyWinterCar) cdPrice = 75;
        ModsShop.Shop shop;
        shop = ModsShop.ModsShop.GetShopReference();
        ModsShop.ItemDetails rackitem = shop.CreateShopItem(this, "rack1", "Rack for 10 CDs", 50, true, BuyCDRack, assets.rack10, ModsShop.SpawnMethod.Instantiate);
        shop.AddDisplayItem(rackitem, assets.rack10_d, ModsShop.SpawnMethod.Instantiate);

        for (int i = 0; i < listOfCases.Count; i++)
        {
            GameObject go = listOfCases[i];
            if (!go.activeSelf)
            {

                ModsShop.ItemDetails item = shop.CreateShopItem(this, $"cd{i}", $"[CD] {go.GetComponent<CDCase>().CDName}", cdPrice, false, BuyCDs, go, ModsShop.SpawnMethod.SetActive);
                if (i == 0)
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
                if (!listOfCases[i].activeSelf || listOfCases[i].GetComponent<CDCase>().inRack)
                    continue;
                listOfCases[i].transform.position = new Vector3(-10.360f, 0.5610f, 5.747f);
                listOfCases[i].transform.localEulerAngles = new Vector3(270f, 270f, 0f);
            }
            for (int i = 0; i < listOfCDs.Count; i++)
            {
                if (!listOfCDs[i].activeSelf || listOfCDs[i].GetComponent<CD>().inPlayer || listOfCDs[i].GetComponent<CD>().inCase)
                    continue;
                listOfCDs[i].transform.position = new Vector3(-10.365f, 0.5563f, 5.581f);
                listOfCDs[i].transform.localEulerAngles = new Vector3(270f, 270f, 0f);
            }
            for (int i = 0; i < listOfRacks.Count; i++)
            {
                if (!listOfRacks[i].activeSelf)
                    continue;
                listOfRacks[i].transform.position = new Vector3(-10.070f, 0.5331f, 5.910f);
                listOfRacks[i].transform.transform.localEulerAngles = new Vector3(0f, 270f, 0f);
            }
            ModUI.ShowMessage("Lost CDs and racks should be now on kitchen table. (This resets only purchased racks and CDs)", "Reset CD positions");
        }
    }

    void FindPlayer_MSC()
    {
        PlayMakerFSM cdp = GameObject.Find("Database/DatabaseOrders/CD_player").GetComponent<PlayMakerFSM>();
        // cdp.FsmVariables.FindFsmBool("Purchased");
        cdp.FsmVariables.FindFsmGameObject("ThisPart").Value.transform.Find("Sled/cd_sled_pivot").gameObject.AddComponent<CarCDPlayer>().cdplayer = this;
        ModConsole.Print("<color=green>Your CD Player is now enhanced! Enjoy.</color>");
    }

    void FindCDPlayers_MWC()
    {
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/ButtonsCD/CDplrVolume"
        //"SORBET(190-200psi)/Functions/Radio/cd player(Clone)/Sled/cd_sled_pivot"
        PlayMakerFSM volume = GameObject.Find("SORBET(190-200psi)").transform.Find("Functions/Radio/cd player(Clone)/ButtonsCD/CDplrVolume").GetPlayMaker("Knob");
        GameObject.Find("SORBET(190-200psi)").transform.Find("Functions/Radio/cd player(Clone)/Sled/cd_sled_pivot").gameObject.AddComponent<CarCDPlayerMWC>().SetupMod(this, AttachedTo.Sorbet, volume);
        //"JOBS/TAXIJOB/MACHTWAGEN/LOD/Radio/cd player(Clone)/ButtonsCD/CDplrVolume"
        //"JOBS/TAXIJOB/MACHTWAGEN/LOD/Radio/cd player(Clone)/Sled/cd_sled_pivot"
        GameObject machtwagen = GameObject.Find("JOBS").transform.Find("TAXIJOB/MACHTWAGEN").gameObject;
        if (machtwagen == null)
            machtwagen = GameObject.Find("MACHTWAGEN").gameObject;
        PlayMakerFSM volume2 = machtwagen.transform.Find("LOD/Radio/cd player(Clone)/ButtonsCD/CDplrVolume").GetPlayMaker("Knob");
        machtwagen.transform.Find("LOD/Radio/cd player(Clone)/Sled/cd_sled_pivot").gameObject.AddComponent<CarCDPlayerMWC>().SetupMod(this, AttachedTo.Machtwagen, volume2);

        //"CORRIS/Assemblies/VINP_Radio/Sled/cd_sled_pivot"
        //"CORRIS/Assemblies/VINP_Radio/CDPlayer1/ButtonsCD/CDplrVolume"
        PlayMakerFSM volume3 = GameObject.Find("CORRIS").transform.Find("Assemblies/VINP_Radio/CDPlayer1/ButtonsCD/CDplrVolume").GetPlayMaker("Knob");
        GameObject.Find("CORRIS").transform.Find("Assemblies/VINP_Radio/Sled/cd_sled_pivot").gameObject.AddComponent<CarCDPlayerMWC>().SetupMod(this, AttachedTo.Corris, volume3);

        //"HOMENEW/Functions/FunctionsDisable/Stereos/Player/Sled/cd_sled_pivot/stereos_sled"
        //"HOMENEW/Functions/FunctionsDisable/Stereos/Player/ButtonsCD/Volume"
        PlayMakerFSM volume4 = GameObject.Find("HOMENEW").transform.Find("Functions/FunctionsDisable/Stereos/Player/ButtonsCD/Volume").GetPlayMaker("Knob");
        GameObject.Find("HOMENEW").transform.Find("Functions/FunctionsDisable/Stereos/Player/Sled/cd_sled_pivot/stereos_sled").gameObject.AddComponent<StereoCDPlayerMWC>().SetupMod(this, AttachedTo.ApartmentStereo, volume4);

        ModConsole.Print("<color=green>Your CD Players are now enhanced! Enjoy.</color>");
    }

}
#endif
