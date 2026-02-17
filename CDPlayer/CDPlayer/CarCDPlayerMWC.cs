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
    public enum AttachedTo
    {
        Sorbet,
        Corris,
        Machtwagen,
        ApartmentStereo,
        Custom
    }
    public class SpeakerWatcherCDPE : MonoBehaviour
    {
       public  CarCDPlayerMWC cdp;
        public CarCDPlayer cdp2;
        void OnTransformParentChanged()
        {
            if (ModLoader.CurrentGame == Game.MyWinterCar)
            {
                //"CORRIS/Functions/Radio/SoundSpeakerBass/CDAudioSourceCorris"
                if (transform.parent.name == "SoundSpeakerBass")
                {
                    cdp.FilterUpdate();
                }
                else
                {
                    cdp.FilterUpdate(true);
                }
            }
            if(ModLoader.CurrentGame == Game.MySummerCar)
            {
                if (transform.parent.name == "SpeakerBass")
                {
                    cdp2.FilterUpdate(false, true);
                }
                else
                {
                    cdp2.FilterUpdate(true);
                }
            }

        }
    }
    public class CarCDPlayerMWC : MonoBehaviour
    {
#if !Mini
        public AttachedTo attachedCar;

        private CDPlayer cdplayer;
        private ModAudio audioPlayer;
        private bool isPlayerOn;
        private bool isCDin;
        private Collider eject, nextTrack;
        private string[] audioFiles;
        private int currentSong = 0;
        private bool isCDplaying;
        private bool changingSong;
        private bool waiting;
        private bool loadingCD;

        private ModAudioStream audioStreamPlayer;
        private int streamingChannel = 0;
        private bool isStreamPlaying = false;
        private bool isEmptyStreamingChannel = false;
        private bool scrollDone;
        private string DisplayRDS;
        private bool entered;
        private bool ready;
        private bool noAntenna = false;

        private GameObject insertedCD, RadioChannels;
        private Collider enteredCD;
        private CD cd;
        private FsmBool isOnRadio;
        private PlayMakerFSM radioCDSwitch;
        internal bool CDempty;
        private HashSet<string> allowedExtensions = ModAudio.allowedExtensions;
        private bool isCDLoadingFromSave = false;
        private bool LoadCdFromSaveRoutine = false;
        private TextMesh lcdText;

        private FsmBool GUIassemble, GUIdisassemble, GUIuse;
        private FsmString GUIinteraction;
        private FsmString channelFsmText;

        private AudioDistortionFilter distortionFilter;
        internal void SetupMod(CDPlayer mod, AttachedTo car, PlayMakerFSM knobFsm)
        {
            if(transform.childCount > 0) //if Vanilla CD was inserted before installing mod
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(0).SetParent(null);
            }
            cdplayer = mod;
            attachedCar = car;
            audioPlayer = gameObject.AddComponent<ModAudio>();
            audioStreamPlayer = gameObject.AddComponent<ModAudioStream>();
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
            gameObject.GetComponent<BoxCollider>().size = new Vector3(0.15f, 0.12f, 0.049f);
            gameObject.GetComponent<BoxCollider>().center = new Vector3(0f, 0.05f, 0f);
            lcdText = (TextMesh)knobFsm.GetVariable<FsmObject>("LCD").Value;
            eject = knobFsm.transform.parent.Find("Eject").GetComponent<SphereCollider>();
            nextTrack = knobFsm.GetVariable<FsmGameObject>("ChannelSwitch").Value.GetComponent<SphereCollider>();
            radioCDSwitch = knobFsm.GetVariable<FsmGameObject>("CDSwitch").Value.GetPlayMaker("Use");
            isOnRadio = radioCDSwitch.GetVariable<FsmBool>("RadioOn");
            RadioChannels = knobFsm.GetVariable<FsmGameObject>("RadioChannels").Value;
            knobFsm.gameObject.FsmInject("Knob", "On", TurnOn);
            knobFsm.gameObject.FsmInject("Knob", "Volume dec", VolDec);
            knobFsm.gameObject.FsmInject("Knob", "Off", TurnOff);
            knobFsm.gameObject.FsmInject("Knob", "Elec off", TurnOff);
            knobFsm.gameObject.FsmInject("Knob", "State 2", ResetWait);
            channelFsmText = knobFsm.GetVariable<FsmString>("Channel");
            eject.gameObject.GetPlayMaker("Use").GetVariable<FsmGameObject>("TriggerDisc").Value.GetPlayMaker("Data").GetVariable<FsmBool>("CDin").Value = false;
            eject.gameObject.GetPlayMaker("Use").GetVariable<FsmGameObject>("TriggerDisc").Value.SetActive(false);
            eject.gameObject.GetPlayMaker("Use").enabled = false;
            audioPlayer.audioSource = knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetComponent<AudioSource>();
            audioStreamPlayer.audioSource = knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetComponent<AudioSource>();
            knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetPlayMaker("Update").enabled = false;
            distortionFilter = knobFsm.GetVariable<FsmGameObject>("SoundSource").Value.GetComponent<AudioDistortionFilter>();

            if (attachedCar == AttachedTo.Corris)
            {
                audioPlayer.gameObject.AddComponent<SpeakerWatcherCDPE>().cdp = this;
            }
        }
        internal void FilterUpdate(bool forceEnable = false)
        {
            if (forceEnable)
            {
                distortionFilter.enabled = true;
                return;
            }
            if (audioPlayer.transform.parent.name == "SoundSpeakerBass")
            {
                distortionFilter.enabled = !cdplayer.bypassDisCar.GetValue();
            }
        }
        void Start()
        {
            //Globals 
            GUIassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble");
            GUIdisassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble");
            GUIuse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
            GUIinteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
        }
        void ResetWait()
        {
            waiting = false;
        }
        void TurnOn()
        {
            isPlayerOn = true;
            waiting = true;
        }
        void TurnOff()
        {
            isPlayerOn = false;
            waiting = false;
        }

        void VolDec()
        {
            waiting = true;
        }
        void PlayCDPlayerBeep()
        {
            MasterAudio.PlaySound3DAndForget("CarFoley", transform, variationName: "cd_button");
        }
        void SetLCDText(string text)
        {
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
            noAntenna = radioCh.FsmVariables.FindFsmBool("OnStaticAntenna1").Value;

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

            if (noAntenna)
            {
                //Garbage text
                System.Random r = new();
                currentText = new string([.. audioStreamPlayer.songInfo.ToCharArray().OrderBy(s => (r.Next(2) % 2) == 0)]);
            }
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
        }
        void Next()
        {
            currentSong++;
            if (currentSong >= audioFiles.Length)
                currentSong = 0;
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


            if (loadingCD)
            {
                if (!waiting)
                    SetLCDText("Load");
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
                if (!waiting)
                {
                    if (cdplayer.RDSsim.GetValue() && !isEmptyStreamingChannel)
                        SetLCDText(DisplayRDS);
                    else
                        SetLCDText($"Channel {streamingChannel + 2}");
                }
            }
            if (!isOnRadio.Value && (isStreamPlaying || isEmptyStreamingChannel))
            {
                StopStream();
            }

            if (!isOnRadio.Value && isCDin && eject.gameObject.activeSelf && !loadingCD)
            {
                Play();
            }
            else
            {
                if (isCDplaying)
                    Stop();
            }

            if (isCDplaying)
            {
                if (!waiting)
                    SetLCDText($"{currentSong + 1}   {audioPlayer.Time().Minutes:D1}'{audioPlayer.Time().Seconds:D2}");
            }

            if (CDempty && !isOnRadio.Value)
            {
                if (!waiting)
                    SetLCDText("CD Empty");
            }

        }

        IEnumerator LoadingCD()
        {
            yield return new WaitForSeconds(1f);
            if (isOnRadio.Value)
            {
                radioCDSwitch.SendEvent("SUSKI"); //WTF is this event name to switch radio to cd
            }

            yield return new WaitForSeconds(4f);
            loadingCD = false;
        }
        void LoadCD()
        {
            ReadFiles(cd.isPlaylist, cd.CDPath);
            eject.gameObject.SetActive(true);
            cd.inPlayer = true;
            cd.inPlayerID = (sbyte)attachedCar;
            loadingCD = true;
            isCDin = true;
            currentSong = 0;
            StartCoroutine(LoadingCD());
        }
        IEnumerator InsertCD()
        {
            yield return null;
            cd = insertedCD.GetComponent<CD>();
            cd.rb.isKinematic = true;
            cd.rb.detectCollisions = false;
            insertedCD.transform.SetParent(transform, false);
            insertedCD.layer = 0;
            insertedCD.transform.localPosition = Vector3.zero;
            insertedCD.transform.localEulerAngles = Vector3.zero;
            transform.GetComponent<Animation>().Play("cd_sled_in");
            GUIassemble.Value = false;
            GUIdisassemble.Value = false;
            GUIinteraction.Value = string.Empty;
            LoadCD();
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
            insertedCD = transform.GetChild(0).gameObject;
            cd = insertedCD.GetComponent<CD>();
            cd.rb.isKinematic = true;
            cd.rb.detectCollisions = false;
            insertedCD.layer = 0;
            /*  insertedCD.transform.SetParent(transform, false);

              insertedCD.transform.localPosition = Vector3.zero;
              insertedCD.transform.localEulerAngles = Vector3.zero;*/
            transform.GetComponent<Animation>().Play("cd_sled_in");
            ReadFiles(cd.isPlaylist, cd.CDPath);

            eject.gameObject.SetActive(true);
            cd.inPlayer = true;
            cd.inPlayerID = (sbyte)attachedCar;
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
        IEnumerator EjectWait(GameObject cd)
        {
            while(gameObject.GetComponentInParent<Animation>().isPlaying)
                yield return null;
            cd.MakePickable();
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
                    insertedCD = enteredCD.gameObject;
                    StartCoroutine(InsertCD());
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
            if (hit.collider == eject)
            {
                GUIuse.Value = true;
                GUIinteraction.Value = "Eject CD";
                if (Input.GetMouseButtonDown(0))
                {
                    isCDin = false;
                    PlayCDPlayerBeep();
                    eject.gameObject.SetActive(false);
                    loadingCD = false;
                    CDempty = false;
                    transform.GetChild(0).localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0f, 359f));
                    gameObject.GetComponentInParent<Animation>().Play("cd_sled_out");
                    SetLCDText("NO CD");
                    StartCoroutine(EjectWait(transform.GetChild(0).gameObject));
                }
            }

            if (hit.collider == nextTrack && !isOnRadio.Value)
            {
                GUIuse.Value = true;
                GUIinteraction.Value = "Next/Previous Song";
                if (Input.GetMouseButtonDown(0))
                {
                    PlayCDPlayerBeep();
                    if (isCDin && !loadingCD)
                        Next();
                }
                if (Input.GetMouseButtonDown(1))
                {
                    PlayCDPlayerBeep();
                    if (isCDin && !loadingCD)
                        Previous();
                }
            }
            if (hit.collider == nextTrack && isOnRadio.Value)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    PlayCDPlayerBeep();
                    StopStream();
                }
                if (Input.GetMouseButtonDown(1))
                {
                    PlayCDPlayerBeep();
                    ChangeChannel();
                }
            }
        }

        void OnTriggerStay(Collider col)
        {
            if (col.gameObject.name == "cd(itemz)" && col.transform.parent != null)
            {
                entered = true;
                if (!isPlayerOn)
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
                        GUIinteraction.Value = "Play this CD";
                    }
                    else
                    {
                        GUIdisassemble.Value = true;
                        GUIinteraction.Value = "CD already inside";
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