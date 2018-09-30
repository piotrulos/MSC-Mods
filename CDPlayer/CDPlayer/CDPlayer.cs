using MSCLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CDPlayer
{
    public class CDSaveData
    {
        public List<CDSaveDataList> save = new List<CDSaveDataList>();
    }
    public class CDSaveDataList
    {
        public Vector3 pos;
        public float rotX, rotY, rotZ;
        public string CDName;
        public bool inCase;
    }

    public class CaseSaveData
    {
        public List<CaseSaveDataList> save = new List<CaseSaveDataList>();
    }
    public class CaseSaveDataList
    {
        public Vector3 pos;
        public float rotX, rotY, rotZ;
        public string CDName;
    }

    public class CDPlayer : Mod
    {
        public override string ID => "CDPlayer";
        public override string Name => "CDPlayer Enchanced";
        public override string Author => "Piotrulos";
        public override string Version => "1.1.3";

        //Set this to true if you will be load custom assets from Assets folder.
        //This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;

        //   GameObject cd, cd2, cdCase, cdCase2;
        public string path = Path.GetFullPath("CD");

        Settings resetCds = new Settings("resetcd", "Reset CDs", ResetPosition);


        public override void ModSettings()
        {
            Settings.AddButton(this, resetCds, "Respawn CDs and Cases on kitchen table");
        }

        public override void OnSave()
        {
            CDSaveData sd = new CDSaveData();
            foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
            {
                if (go.name == "cd(item2)")
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
                    sd.save.Add(sdl);
                }
            }
            SaveLoad.SerializeSaveFile(this, sd, "cd.save");

            CaseSaveData csd = new CaseSaveData();
            foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
            {
                if (go.name == "cd case(item2)")
                {
                    CaseSaveDataList sdl = new CaseSaveDataList
                    {
                        pos = go.transform.position,
                        rotX = go.transform.rotation.eulerAngles.x,
                        rotY = go.transform.rotation.eulerAngles.y,
                        rotZ = go.transform.rotation.eulerAngles.z,
                        CDName = go.GetComponent<CDCase>().CDName
                    };
                    csd.save.Add(sdl);
                }
            }
            SaveLoad.SerializeSaveFile(this, csd, "case.save");

        }
        //Called when mod is loading
        public override void OnLoad()
        {
            GameObject cd = GameObject.Find("cd(item1)");
            cd.GetComponent<PlayMakerFSM>().SendEvent("REMOVE");
            GameObject cdCase = GameObject.Find("cd case(Clone)");
            string[] dirs = Directory.GetDirectories(path);
            int i = 0;
            foreach (string dir in dirs)
            {
                Quaternion e = Quaternion.Euler(new Vector3(270, 0, 0));
                GameObject cd2 = (GameObject)Object.Instantiate(cd, new Vector3(-9.76f, 0.17f, 6.47f), e);
                GameObject cdCase2 = GameObject.Instantiate(cdCase);
                cd2.name = "cd(item2)";
                cdCase2.name = "cd case(item2)";
                cd2.AddComponent<CD>().CDName = new DirectoryInfo(dir).Name;
                cdCase2.AddComponent<CDCase>().CDName = new DirectoryInfo(dir).Name;
                if (i == dirs.Length - 1)
                    cdCase2.GetComponent<CDCase>().cdp = this;
                // cdCase2.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
                if (File.Exists(Path.Combine(dir, "folder.txt")))
                {
                    string[] txtDirs = File.ReadAllLines(Path.Combine(dir, "folder.txt"));
                    cd2.GetComponent<CD>().CDPath = Path.GetFullPath(txtDirs[0]);
                }
                else
                    cd2.GetComponent<CD>().CDPath = Path.GetFullPath(dir);
                cd2.GetComponent<CD>().cdCase = cdCase2.GetComponent<CDCase>();

                cd2.GetComponent<CD>().inCase = true;
                cd2.transform.SetParent(cdCase2.transform.GetChild(2), false);

                //Load coverart.png if exists, else leave default.
                if (File.Exists(Path.Combine(dir, "coverart.png")))
                {
                    Texture2D t2d = new Texture2D(1, 1);
                    t2d.LoadImage(File.ReadAllBytes(Path.Combine(dir, "coverart.png")));
                    cd2.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCase2.transform.GetChild(3).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                    cdCase2.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = t2d;
                }
                i++;
            }
            cd.SetActive(false);
            cdCase.SetActive(false);
            FindPlayer();
            //Load();

        }

        public static void ResetPosition()
        {
            if (Application.loadedLevelName == "MainMenu")
            {
                ModUI.ShowMessage("Please use this when you are in game!", "Reset CD positions");
            }
            else
            {
                Quaternion e = Quaternion.Euler(new Vector3(270, 0, 0));
                foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (go.name == "cd(item2)")
                    {
                        if (!go.GetComponent<CD>().inPlayer && !go.GetComponent<CD>().inCase)
                        {
                            go.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
                            go.transform.rotation = e;
                        }
                    }
                    else if (go.name == "cd case(item2)")
                    {
                        go.transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
                        go.transform.rotation = e;
                    }
                }
            }
        }    

        void FindPlayer()
        {
            List<Transform> players = Resources.FindObjectsOfTypeAll<Transform>().Where(x => x.name == "cd player(Clone)").ToList();

            foreach(Transform p in players)
            {
                //find correct cd player object
                if(p.parent.name != "Boxes" && p.parent.name != "Products")
                {
                    p.gameObject.AddComponent<CarCDPlayer>();
                    ModConsole.Print("<color=green>Your CD Player is now enchanced! Enjoy.</color>");
                }
            }
        }

        public void Load()
        {
            string old_path = Path.Combine(ModLoader.GetModConfigFolder(this), "cds.save");

            if (File.Exists(old_path))
                File.Delete(old_path);

            CaseSaveData cdata = SaveLoad.DeserializeSaveFile<CaseSaveData>(this, "case.save");
            if (cdata != null)
            {
                foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (go.name == "cd case(item2)")
                    {
                        for (int i = 0; i < cdata.save.Count; i++)
                        {
                            if (cdata.save[i].CDName == go.GetComponent<CDCase>().CDName)
                            {
                                go.transform.position = cdata.save[i].pos;
                                go.transform.rotation = Quaternion.Euler(cdata.save[i].rotX, cdata.save[i].rotY, cdata.save[i].rotZ);
                            }
                        }
                    }
                }
            }

            CDSaveData data = SaveLoad.DeserializeSaveFile<CDSaveData>(this, "cd.save");
            if (data != null)
            {
                foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (go.name == "cd(item2)")
                    {
                        for (int i = 0; i < data.save.Count; i++)
                        {
                            if (data.save[i].CDName == go.GetComponent<CD>().CDName)
                            {
                                if (!data.save[i].inCase)
                                {
                                    go.GetComponent<CD>().inCase = false;
                                    go.transform.SetParent(null);
                                    go.transform.position = data.save[i].pos;
                                    go.transform.rotation = Quaternion.Euler(data.save[i].rotX, data.save[i].rotY, data.save[i].rotZ);
                                }
                            }
                        }
                    }
                }
            }
        }
        // Update is called once per frame
        public override void Update()
        {

        }
    }
}
