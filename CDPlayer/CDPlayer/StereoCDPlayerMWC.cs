using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

//Standard unity MonoBehaviour class
namespace CDPlayer
{

    public class StereoCDPlayerMWC : MonoBehaviour
    {
#if !Mini
        public AttachedTo attachedCar;

        //CDPlayer
        private CDPlayer cdplayer;
        private ModAudio audioPlayer;
        private bool isPlayerOn;
        private bool isCDin;
        private Collider eject, nextTrack;
        private string[] audioFiles;
        private int currentSong = 0;
        private bool isCDplaying;
        private bool changingSong;
        private bool loadingCD;
        private bool cdTrayOpen = false;

        //Radio Streaming
        private ModAudioStream audioStreamPlayer;
        private int streamingChannel = 0;
        private bool isStreamPlaying = false;
        private bool isEmptyStreamingChannel = false;
        private bool scrollDone;
        private string DisplayRDS;
        private bool entered;
        private bool ready;

        private GameObject RadioChannels;
        private Collider enteredCD;
        private CD cd;

        private FsmBool isOnRadio;
        internal bool CDempty;
        private HashSet<string> allowedExtensions = ModAudio.allowedExtensions;
        private bool isCDLoadingFromSave = false;
        private bool LoadCdFromSaveRoutine = false;
        private TextMesh lcdText;

        private FsmBool GUIassemble, GUIdisassemble, GUIuse;
        private FsmString GUIinteraction;
        private FsmString channelFsmText;

        private AudioDistortionFilter distortionFilter;
        public void SetupMod(CDPlayer mod, AttachedTo car, PlayMakerFSM knobFsm)
        {
            cdplayer = mod;
            attachedCar = car;
            audioPlayer = gameObject.AddComponent<ModAudio>();
            audioStreamPlayer = gameObject.AddComponent<ModAudioStream>();

            lcdText = knobFsm.GetVariable<FsmGameObject>("Settings").Value.transform.parent.Find("LCD").GetComponent<TextMesh>();
            eject = knobFsm.GetVariable<FsmGameObject>("Eject").Value.GetComponent<SphereCollider>();
            eject.gameObject.GetPlayMaker("Use").GetVariable<FsmGameObject>("TriggerDisc").Value.SetActive(false);
            eject.gameObject.GetPlayMaker("Use").enabled = false;

            nextTrack = knobFsm.GetVariable<FsmGameObject>("ChannelSwitch").Value.GetComponent<SphereCollider>();
            PlayMakerFSM radioCDSwitch = knobFsm.GetVariable<FsmGameObject>("CDSwitch").Value.GetPlayMaker("Use");
            isOnRadio = radioCDSwitch.GetVariable<FsmBool>("RadioOn");
            RadioChannels = knobFsm.GetVariable<FsmGameObject>("RadioChannels").Value;
            knobFsm.FsmInject("On", TurnOn);
            knobFsm.FsmInject("Off", TurnOff);
            knobFsm.FsmInject("Elec off", TurnOff);
            knobFsm.GetVariable<FsmGameObject>("CDSwitch").Value.GetPlayMaker("Use").FsmInject("Off", SwitchedToCD); //Dumb state name
            knobFsm.GetVariable<FsmGameObject>("CDSwitch").Value.GetPlayMaker("Use").FsmInject("On", SwitchedToRadio); //Dumb state name

            channelFsmText = knobFsm.GetVariable<FsmString>("Channel");
            audioPlayer.audioSource = knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetComponent<AudioSource>();
            audioStreamPlayer.audioSource = knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetComponent<AudioSource>();
            distortionFilter = knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetComponent<AudioDistortionFilter>();
            knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetPlayMaker("Update").enabled = false;
        }
        void Start()
        {
            //Globals 
            GUIassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble");
            GUIdisassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble");
            GUIuse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
            GUIinteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
        }
        void TurnOn()
        {
            if (!isOnRadio.Value && isCDin && !isPlayerOn)
            {
                loadingCD = true;
                loadingCDr = StartCoroutine(LoadingCD());
            }
            isPlayerOn = true;
        }
        void TurnOff()
        {
            isPlayerOn = false;
            if (loadingCDr != null)
            {
                StopCoroutine(loadingCDr);
                loadingCDr = null;
            }
        }
        void SwitchedToCD()
        {
            if (isCDin && isPlayerOn)
            {
                loadingCD = true;
                loadingCDr = StartCoroutine(LoadingCD());
            }
        }
        void SwitchedToRadio()
        {
            if (loadingCDr != null)
            {
                StopCoroutine(loadingCDr);
                loadingCDr = null;
            }
        }

        void SetLCDText(string text, bool onlyIfCDInside)
        {
            if (onlyIfCDInside && !isCDin)
                return;
            lcdText.text = text;
            channelFsmText.Value = text;
        }
        void ChangeChannel()
        {
            StopStream();
            streamingChannel++;
            if (streamingChannel > 2)
                streamingChannel = 1;
            switch (streamingChannel)
            {
                case 1:
                    PlayStream(1);
                    break;
                case 2:
                    PlayStream(2);
                    break;
                default:
                    PlayStream(1);
                    break;
            }
            //ModConsole.Print("streaming channel: " + streamingChannel.ToString());
        }
        void PlayStream(int channel)
        {
            string url = null;
            switch (channel)
            {
                case 1:
                    url = cdplayer.channel3url.GetValue();
                    break;
                case 2:
                    url = cdplayer.channel4url.GetValue();
                    break;
            }
            PlayMakerFSM radioCh = RadioChannels.GetComponent<PlayMakerFSM>();

            radioCh.FsmVariables.FindFsmBool("OnMuteChannel1").Value = true;
            radioCh.FsmVariables.FindFsmBool("OnMuteFolk").Value = true;
            radioCh.FsmVariables.FindFsmBool("OnStatic").Value = true;

            if (url != null && url != string.Empty)
            {
                if (cdplayer.debugInfo.GetValue())
                    audioStreamPlayer.showDebug = true;
                audioStreamPlayer.PlayStream(url);
                radioCh.FsmVariables.FindFsmBool("OnStatic").Value = false;

                isStreamPlaying = true;
                isEmptyStreamingChannel = false;
                scrollDone = true;
            }
            else
            {
                isEmptyStreamingChannel = true;
                radioCh.FsmVariables.FindFsmBool("OnStatic").Value = true;
            }
        }
        Coroutine RDS = null;
        IEnumerator RDSsimulator()
        {
            scrollDone = false;
            DisplayRDS = $"Channel {streamingChannel + 2}";
            yield return new WaitForSeconds(5);
            string oldText = audioStreamPlayer.songInfo;
            string currentText = audioStreamPlayer.songInfo;

            for (int i = 0; i < currentText.Length; i += 9)
            {
                if (oldText != audioStreamPlayer.songInfo) break;
                int l = 9;
                if (currentText.Length - i < 9)
                {
                    l = currentText.Length - i;
                    DisplayRDS = currentText.Substring(i, l) + new string(' ', 9 - l) + "\u00A0";
                }
                else
                    DisplayRDS = currentText.Substring(i, 9);
                yield return new WaitForSeconds(2f);
            }
            yield return new WaitForSeconds(1f);
            scrollDone = true;
            DisplayRDS = $"Channel {streamingChannel + 2}";
            RDS = null;
        }
        void StopStream()
        {
            if (isEmptyStreamingChannel)
                isEmptyStreamingChannel = false;
            if (isStreamPlaying)
            {
                isStreamPlaying = false;
                if (RDS != null)
                    StopCoroutine(RDS);
                audioStreamPlayer.StopStream();
                audioStreamPlayer.showDebug = false;
            }
        }
        void Play()
        {
            StopStream();
            if (!isCDplaying)
            {
                if (audioFiles.Length > 0)
                {
                    CDempty = false;
                    isCDplaying = true;
                    audioPlayer.LoadAudioFromFile(Path.GetFullPath(audioFiles[currentSong]), true, true);
                    audioPlayer.Play();
                }
                else
                {
                    CDempty = true;
                }
            }
        }

        void Stop()
        {
            CDempty = false;
            isCDplaying = false;
            audioPlayer.Stop();
            if (!isOnRadio.Value && isPlayerOn)
                SetLCDText($"{cd.GetComponent<CD>().tracksCount} - {cd.GetComponent<CD>().totalTime}", true);
        }
        void Next()
        {
            currentSong++;
            if (currentSong >= audioFiles.Length)
            {
                currentSong = 0;
                Stop();
                changingSong = false;
                return;
            }
            audioPlayer.Stop();
            audioPlayer.LoadAudioFromFile(Path.GetFullPath(audioFiles[currentSong]), true, true);
            audioPlayer.Play();
            changingSong = false;
        }

        void Previous()
        {
            if (audioPlayer.audioSource.time > 5)
            {
                audioPlayer.Stop();
                audioPlayer.LoadAudioFromFile(Path.GetFullPath(audioFiles[currentSong]), true, true);
                audioPlayer.Play();
            }
            else
            {

                if (currentSong == 0)
                    currentSong = audioFiles.Length - 1;
                else
                    currentSong--;
                audioPlayer.Stop();
                audioPlayer.LoadAudioFromFile(Path.GetFullPath(audioFiles[currentSong]), true, true);
                audioPlayer.Play();
                changingSong = false;
            }
        }
        void FixedUpdate()
        {
            if (!isPlayerOn)
            {
                if (isCDplaying)
                    Stop();
                if (isStreamPlaying || isEmptyStreamingChannel)
                    StopStream();
                return;
            }


            if (isStreamPlaying)
            {
                if (scrollDone)
                {
                    RDS = StartCoroutine(RDSsimulator());
                    scrollDone = false;
                }
            }
            if ((isStreamPlaying || isEmptyStreamingChannel) && isOnRadio.Value)
            {
                if (cdplayer.RDSsim.GetValue() && !isEmptyStreamingChannel)
                    SetLCDText(DisplayRDS, false);
                else
                    SetLCDText($"Channel {streamingChannel + 2}", false);

            }
            if (!isOnRadio.Value && (isStreamPlaying || isEmptyStreamingChannel))
            {
                StopStream();
            }
            if (loadingCD)
            {
                return;
            }

            if (isOnRadio.Value || !isCDin || loadingCD)
            {
                if (loadingCDr != null)
                {
                    StopCoroutine(loadingCDr);
                    loadingCDr = null;
                }

                if (isCDplaying)
                    Stop();
            }

            if (isCDplaying)
            {
                SetLCDText($"{currentSong + 1} -  {audioPlayer.Time().Minutes:D2}:{audioPlayer.Time().Seconds:D2}", true);
            }

            if (CDempty && !isOnRadio.Value)
            {

                SetLCDText("CD Empty", true);
            }

        }
        Coroutine loadingCDr = null;
        IEnumerator LoadingCD()
        {
            for (int i = 0; i < 6; i++)
            {
                SetLCDText("Reading", true);
                yield return new WaitForSeconds(.5f);
                SetLCDText("", true);
                yield return new WaitForSeconds(.5f);
            }
            loadingCD = false;
            SetLCDText($"{cd.GetComponent<CD>().tracksCount} - {cd.GetComponent<CD>().totalTime}", true);
            loadingCDr = null;
        }
        void LoadCD()
        {
            ReadFiles(cd.isPlaylist, cd.CDPath);
            cd.inPlayer = true;
            if (!isOnRadio.Value)
                loadingCD = true;
            isCDin = true;
            currentSong = 0;
            if (!isOnRadio.Value)
                loadingCDr = StartCoroutine(LoadingCD());
        }
        IEnumerator InsertCD(GameObject insertedCD)
        {
            yield return null;
            cd = insertedCD.GetComponent<CD>();
            cd.rb.isKinematic = true;
            cd.rb.detectCollisions = false;
            insertedCD.transform.SetParent(transform, false);
            insertedCD.layer = 0;
            insertedCD.transform.localPosition = new Vector3(-0.1340341f, 0f, 0.1132002f); //Vanilla values
            insertedCD.transform.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0f, 359f));
            GUIassemble.Value = false;
            GUIdisassemble.Value = false;
            GUIinteraction.Value = string.Empty;

        }
        void OnEnable()
        {
            if (LoadCdFromSaveRoutine && !isCDLoadingFromSave)
            {
                //  StopCoroutine(LoadCdFromSaveRoutine);
                StartCoroutine(LoadSavedCD());
            }

        }
        IEnumerator LoadSavedCD()
        {
            isCDLoadingFromSave = true;
            yield return null;
            GameObject insertedCD = transform.GetChild(0).gameObject;
            cd = insertedCD.GetComponent<CD>();
            cd.rb.isKinematic = true;
            cd.rb.detectCollisions = false;
            insertedCD.transform.SetParent(transform, false);
            insertedCD.layer = 0;
            insertedCD.transform.localPosition = new Vector3(-0.1340341f, 0f, 0.1132002f); //Vanilla values
            insertedCD.transform.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0f, 359f));
            transform.GetComponent<Animation>().Play("cd_sled_in");
            ReadFiles(cd.isPlaylist, cd.CDPath);

            cd.inPlayer = true;
            isCDin = true;
            currentSong = 0;
            LoadCdFromSaveRoutine = false;
            isCDLoadingFromSave = false;
        }

        internal void LoadCDFromSave()
        {
            LoadCdFromSaveRoutine = true;
            StartCoroutine(LoadSavedCD());
        }

        internal void ReadFiles(bool isPlaylist, string path)
        {
            if (isPlaylist)
            {
                if (path.ToLower().EndsWith(".m3u", StringComparison.OrdinalIgnoreCase) || path.ToLower().EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                    audioFiles = [.. Playlists.m3uPlaylist(path)];
                if (path.ToLower().EndsWith(".pls", StringComparison.OrdinalIgnoreCase))
                    audioFiles = [.. Playlists.plsPlaylist(path)];
            }
            else
            {

                audioFiles = [.. Directory.GetFiles(path, "*.*").Where(file => allowedExtensions.Contains(Path.GetExtension(file)))];
            }
        }
        bool trayWait = false;
        IEnumerator EjectWait(GameObject trayCD, bool eject)
        {
            while (gameObject.GetComponentInParent<Animation>().isPlaying)
                yield return null;
            if (eject && trayCD != null)
                trayCD.MakePickable();
            else
            {
                if (trayCD == null)
                {
                    cd = null;
                    if (!isOnRadio.Value)
                        SetLCDText("No CD", false);
                }
                else
                {
                    cd = transform.GetChild(0).gameObject.GetComponent<CD>();
                    LoadCD();
                }
            }
            trayWait = false;

        }
        void Update()
        {
            if (!isPlayerOn) return;
            if (entered && ready)
            {
                if (Input.GetMouseButtonDown(0) && enteredCD != null)
                {
                    ready = false;
                    entered = false;
                    StartCoroutine(InsertCD(enteredCD.gameObject));
                }
            }
            if (isCDplaying && isPlayerOn)
            {
                if (!audioPlayer.audioSource.isPlaying && !changingSong)
                {
                    changingSong = true;
                    Next();
                }
            }

            RaycastHit hit = UnifiedRaycast.GetRaycastHitInteraction();
            if (hit.collider == eject && !trayWait)
            {
                GUIuse.Value = true;

                GUIinteraction.Value = cdTrayOpen ? "Close Tray" : "Open Tray";
                if (Input.GetMouseButtonDown(0))
                {
                    trayWait = true;
                    if (cdTrayOpen)
                    {
                        gameObject.GetComponentInParent<Animation>().Play("cd_sled_in");
                        if (!isOnRadio.Value)
                            SetLCDText("Close", false);
                        StartCoroutine(EjectWait(transform.childCount > 0 ? transform.GetChild(0).gameObject : null, false));
                    }
                    else
                    {
                        if (loadingCDr != null)
                        {
                            StopCoroutine(loadingCDr);
                            loadingCDr = null;
                        }
                        if (transform.childCount > 0)
                            transform.GetChild(0).localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0f, 359f));
                        gameObject.GetComponentInParent<Animation>().Play("cd_sled_out");
                        if (!isOnRadio.Value)
                            SetLCDText("Open", false);
                        StartCoroutine(EjectWait(transform.childCount > 0 ? transform.GetChild(0).gameObject : null, true));
                        isCDin = false;
                        loadingCD = false;
                        CDempty = false;
                    }

                    cdTrayOpen = !cdTrayOpen;

                }
            }

            if (hit.collider == nextTrack && !isOnRadio.Value)
            {
                GUIuse.Value = true;
                GUIinteraction.Value = isCDplaying ? "Next/Previous Song" : "Play";
                if (!isCDin || loadingCD) return;
                if (Input.GetMouseButtonDown(0))
                {
                    //  PlayCDPlayerBeep();
                    if (!isCDplaying)
                    {
                        Play();
                    }
                    else
                    {

                        Next();
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {

                    if (!isCDplaying)
                    {
                        Play();
                    }
                    else
                    {
                        Previous();
                    }
                }
            }
            if (hit.collider == nextTrack && isOnRadio.Value)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //   PlayCDPlayerBeep();
                    StopStream();
                }
                if (Input.GetMouseButtonDown(1))
                {
                    //   PlayCDPlayerBeep();
                    ChangeChannel();
                }
            }
        }

        void OnTriggerStay(Collider col)
        {
            if (col.gameObject.name == "cd(itemz)" && col.transform.parent != null)
            {
                entered = true;
                if (!isPlayerOn) //isTrayOpen
                {
                    GUIdisassemble.Value = true;
                    GUIinteraction.Value = "CD Player is off";
                    ready = false;
                }
                else
                {
                    if (!isCDin)
                    {
                        enteredCD = col;
                        ready = true;
                        GUIassemble.Value = true;
                        GUIinteraction.Value = "Insert this CD";
                    }
                    else
                    {
                        GUIdisassemble.Value = true;
                        GUIinteraction.Value = "Other CD on tray";
                        ready = false;
                    }
                }
            }
        }
        void OnTriggerExit()
        {
            GUIassemble.Value = false;
            GUIdisassemble.Value = false;
            GUIinteraction.Value = string.Empty;
            ready = false;
            entered = false;
            enteredCD = null;
        }
#endif
    }
}