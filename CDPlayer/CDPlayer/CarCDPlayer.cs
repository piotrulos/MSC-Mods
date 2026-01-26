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
    public class CarCDPlayer : MonoBehaviour
    {
#if !Mini     
        public CDPlayer cdplayer;

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

        private GameObject insertedCD, RadioChannels, CDPlayerSpeaker;
        private Collider enteredCD;
        private CD cd;
        private Transform rootCDplayer;
        private GameObject radioVol;
        private FsmBool isOnRadio, subwooferInstalled;
        private PlayMakerFSM radioVolFSM;
        internal bool CDempty;
        private HashSet<string> allowedExtensions = ModAudio.allowedExtensions;
        private bool isCDLoadingFromSave = false;
        private bool LoadCdFromSaveRoutine = false;
        private TextMesh lcdText;

        private FsmBool GUIassemble, GUIdisassemble, GUIuse;
        private FsmString GUIinteraction;
        void Start()
        {
            //Setup palyer
            rootCDplayer = transform.parent.parent;
            rootCDplayer.transform.Find("trigger_disc").gameObject.SetActive(false);
            audioPlayer = gameObject.AddComponent<ModAudio>();
            audioStreamPlayer = gameObject.AddComponent<ModAudioStream>();
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
            gameObject.GetComponent<BoxCollider>().size = new Vector3(0.15f, 0.12f, 0.049f);
            gameObject.GetComponent<BoxCollider>().center = new Vector3(0f, 0.05f, 0f);

            //Get stuff from FSMs
            lcdText = rootCDplayer.transform.Find("LCD").GetComponent<TextMesh>();
            GameObject buttonsCD = rootCDplayer.transform.Find("ButtonsCD").gameObject;
            eject = buttonsCD.transform.Find("Eject").GetComponent<SphereCollider>();
            nextTrack = buttonsCD.transform.Find("TrackChannelSwitch").GetComponent<SphereCollider>();
            isOnRadio = buttonsCD.transform.Find("RadioCDSwitch").GetPlayMaker("Use").FsmVariables.FindFsmBool("RadioOn");
            RadioChannels = buttonsCD.transform.Find("RadioCDSwitch").GetPlayMaker("Use").FsmVariables.FindFsmGameObject("RadioChannels").Value;
            radioVol = buttonsCD.transform.Find("RadioVolume").gameObject;
            radioVol.FsmInject("Knob", "On", TurnOn);
            radioVol.FsmInject("Knob", "Decrease", VolDec);
            radioVol.FsmInject("Knob", "Off", TurnOff);
            radioVol.FsmInject("Knob", "Off 2", TurnOff);
            radioVolFSM = radioVol.GetComponent<PlayMakerFSM>();
            subwooferInstalled = GameObject.Find("Database/PartsStatus/Subwoofers").GetPlayMaker("Subwoofer").FsmVariables.FindFsmBool("Subwoofers");
            CDPlayerSpeaker = GameObject.Find("Database/PartsStatus/Subwoofers").GetPlayMaker("Subwoofer").FsmVariables.FindFsmGameObject("CD").Value;
            audioPlayer.audioSource = CDPlayerSpeaker.GetComponent<AudioSource>();
            audioStreamPlayer.audioSource = CDPlayerSpeaker.GetComponent<AudioSource>();

            //Globals 
            GUIassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble");
            GUIdisassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble");
            GUIuse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
            GUIinteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
        }
        void TurnOn()
        {
            if (isPlayerOn)
            {
                if (volumeWait != null)
                    StopCoroutine(volumeWait);
                volumeWait = StartCoroutine(volChangedWait());
            }
            isPlayerOn = true;
        }
        void TurnOff()
        {
            isPlayerOn = false;
        }
        Coroutine volumeWait;
        IEnumerator volChangedWait()
        {
            waiting = true;
            yield return null;
            if (!isOnRadio.Value || isStreamPlaying)
                radioVolFSM.FsmVariables.FindFsmString("Channel").Value = radioVolFSM.FsmVariables.FindFsmString("Data2").Value;
            yield return new WaitForSeconds(3f);
            waiting = false;
            volumeWait = null;
        }
        void VolDec()
        {
            if (isPlayerOn)
            {
                if (volumeWait != null)
                    StopCoroutine(volumeWait);
                volumeWait = StartCoroutine(volChangedWait());
            }
        }
        void PlayCDPlayerBeep()
        {
            MasterAudio.PlaySound3DAndForget("CarFoley", transform, variationName: "cd_button");
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
            if (subwooferInstalled.Value)
                CDPlayer.FilterChange();

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
            for (int i = 0; i < currentText.Length; i += 10)
            {
                if (oldText != audioStreamPlayer.songInfo) break;
                int l = 10;
                if (currentText.Length - i < 10)
                {
                    l = currentText.Length - i;
                    DisplayRDS = currentText.Substring(i, l) + new string(' ', 10 - l) + ".";
                }
                else
                    DisplayRDS = currentText.Substring(i, 10);
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
                    if (subwooferInstalled.Value)
                        CDPlayer.FilterChange();
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
                    lcdText.text = "Load";
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
                        lcdText.text = DisplayRDS;
                    else
                        lcdText.text = $"Channel {streamingChannel + 2}";
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
                    lcdText.text = $"{currentSong + 1}   {audioPlayer.Time().Minutes:D1}'{audioPlayer.Time().Seconds:D2}";
            }

            if (CDempty && !isOnRadio.Value)
            {
                if (!waiting)
                    lcdText.text = "CD Empty";
            }

        }

        IEnumerator LoadingCD()
        {
            yield return new WaitForSeconds(1f);
            isOnRadio.Value = false;
            RadioChannels.transform.SetParent(GameObject.Find("RADIO").transform, false);
            nextTrack.gameObject.GetPlayMaker("ChangeTrack").enabled = true;
            nextTrack.gameObject.GetPlayMaker("ChangeChannel").enabled = false;
            yield return new WaitForSeconds(4f);
            loadingCD = false;
        }
        void LoadCD()
        {
            ReadFiles(cd.isPlaylist, cd.CDPath);
            eject.gameObject.SetActive(true);
            cd.inPlayer = true;
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
            insertedCD.transform.SetParent(transform, false);
            insertedCD.layer = 0;
            insertedCD.transform.localPosition = Vector3.zero;
            insertedCD.transform.localEulerAngles = Vector3.zero;
            transform.GetComponent<Animation>().Play("cd_sled_in");
            ReadFiles(cd.isPlaylist, cd.CDPath);

            eject.gameObject.SetActive(true);
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
        void Update()
        {
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
            if (hit.collider == eject && isPlayerOn)
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
                    lcdText.text = "NO CD";
                    transform.GetChild(0).MakePickable();
                }
            }

            if (hit.collider == nextTrack && !isOnRadio.Value && isPlayerOn)
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
            if (hit.collider == nextTrack && isOnRadio.Value && isPlayerOn)
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