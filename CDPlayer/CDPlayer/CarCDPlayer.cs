using MSCLoader;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

//Standard unity MonoBehaviour class
namespace CDPlayer
{
    public class CarCDPlayer : MonoBehaviour
    {
        public CDPlayer cdplayer;

        private ModAudio audioPlayer;
        private bool isRadioOn;
        private bool isOnCD;
        private bool isCDin;
        private Collider eject, nextTrack;
        private string[] audioFiles;
        private int currentSong = 0;
        private bool isCDplaying;
        private bool notchanging;
        private float volChanged;
        private bool waiting;
        private bool loadingCD;

        private ModAudioStream audioStreamPlayer;
        private int streamingChannel = 0;
        private bool isStreamPlaying = false;
        private bool isEmptyStreamingChannel = false;
        private bool scroolDone;
        private string DisplayRDS;
        SoundGroupVariation button_sound;
        private bool entered;
        private bool ready;
        private bool noAntenna = false;

        private GameObject insertedCD;
        private Collider enteredCD;
        private CD cd;
        private Transform rootCDplayer;


        public bool CDempty { get; private set; }

        void Start()
        {
            rootCDplayer = transform.parent.parent;
            audioPlayer = gameObject.AddComponent<ModAudio>();
            audioStreamPlayer = gameObject.AddComponent<ModAudioStream>();
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
            gameObject.GetComponent<BoxCollider>().size = new Vector3(0.15f, 0.12f, 0.049f);
            gameObject.GetComponent<BoxCollider>().center = new Vector3(-0.003f, -0.005f, 0.0015f);

            eject = rootCDplayer.Find("ButtonsCD/Eject").GetComponent<SphereCollider>();
            nextTrack = rootCDplayer.Find("ButtonsCD/TrackChannelSwitch").GetComponent<SphereCollider>();
            button_sound = GameObject.Find("MasterAudio/CarFoley/cd_button").GetComponent<SoundGroupVariation>();
        }
        void PlayCDPlayerBeep()
        {
            button_sound.Play(1f, 1f, gameObject.name, 1f, 1f, 1f, transform, false, 0f, false, true);
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
            ModConsole.Print("streaming channel: " + streamingChannel.ToString());
        }
        void PlayStream(int channel)
        {
            string url = null;
            switch (channel)
            {
                case 1:
                    url = (string)cdplayer.channel3url.GetValue();
                    break;
                case 2:
                    url = (string)cdplayer.channel4url.GetValue();
                    break;
            }
            PlayMakerFSM radioCh;
            if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/CDPlayer") != null)
            {
                audioStreamPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/CDPlayer").GetComponent<AudioSource>();
                radioCh = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/RadioChannels").GetComponent<PlayMakerFSM>();
            }
            else
            {
                CDPlayer.FilterChange();
                audioStreamPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/CDPlayer").GetComponent<AudioSource>();
                radioCh = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels").GetComponent<PlayMakerFSM>();
            }
            radioCh.FsmVariables.FindFsmBool("OnMuteChannel1").Value = true;
            radioCh.FsmVariables.FindFsmBool("OnMuteFolk").Value = true;
            radioCh.FsmVariables.FindFsmBool("OnStatic").Value = true;
            noAntenna = radioCh.FsmVariables.FindFsmBool("OnStaticAntenna1").Value;

            if (url != null && url != string.Empty)
            {
                //transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = string.Format("Channel ", streamingChannel + 2);
                if ((bool)cdplayer.debugInfo.GetValue())
                    audioStreamPlayer.showDebug = true;
                audioStreamPlayer.PlayStream(url);
                radioCh.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("OnStatic").Value = false;

                isStreamPlaying = true;
                isEmptyStreamingChannel = false;
                scroolDone = true;
            }
            else
            {
                isEmptyStreamingChannel = true;
                radioCh.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("OnStatic").Value = true;
            }
        }
        IEnumerator RDSsimulator()
        {
            scroolDone = false;
            DisplayRDS = string.Format("Channel {0}", streamingChannel + 2);
            yield return new WaitForSeconds(5);
            string sas = new string(' ', 10) + audioStreamPlayer.songInfo + new string(' ', 10);
            if (noAntenna)
            {
                System.Random r = new System.Random();
                string random = new string(sas.ToCharArray().OrderBy(s => (r.Next(2) % 2) == 0).ToArray());
                sas = random;
            }
            for (int i = 0; i <= sas.Length - 10; i++)
            {
                DisplayRDS = sas.Substring(i, 10);
                yield return new WaitForSeconds(.3f);
            }
            scroolDone = true;
            DisplayRDS = string.Format("Channel {0}", streamingChannel + 2);
        }
        void StopStream()
        {
            if (isEmptyStreamingChannel)
                isEmptyStreamingChannel = false;
            if(isStreamPlaying)
            {
                isStreamPlaying = false;
                StopCoroutine("RDSsimulator");
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
                    if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/CDPlayer") != null)
                        audioPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/CDPlayer").GetComponent<AudioSource>();
                    else
                    {
                        CDPlayer.FilterChange();
                        audioPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/CDPlayer").GetComponent<AudioSource>();
                    }
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
            notchanging = false;
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
                notchanging = false;
            }
        }
        void FixedUpdate()
        {
            if (rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Ok").Value &&
                rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value > 0f)
                isRadioOn = true;
            else
                isRadioOn = false;

            if (!rootCDplayer.Find("ButtonsCD/RadioCDSwitch").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("RadioOn").Value)
                isOnCD = true;
            else
                isOnCD = false;

            if (transform.childCount != 0 && eject.gameObject.activeSelf && transform.Find("cd(item1)") == null)
                isCDin = true;
            else
                isCDin = false;
            if (loadingCD)
            {
                if (!waiting && volChanged != rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting)
                    rootCDplayer.GetChild(0).GetComponent<TextMesh>().text = "Loading...";
                else
                    rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;

            }
            if (isStreamPlaying)
            {
                if (scroolDone)
                {
                    StartCoroutine("RDSsimulator");
                    scroolDone = false;
                }
            }
            if ((isStreamPlaying || isEmptyStreamingChannel) && !isOnCD)
            {
                if (!waiting && volChanged != rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting && isRadioOn)
                {
                    if((bool)cdplayer.RDSsim.GetValue() && !isEmptyStreamingChannel)
                        rootCDplayer.GetChild(0).GetComponent<TextMesh>().text = DisplayRDS;
                    else
                        rootCDplayer.GetChild(0).GetComponent<TextMesh>().text = string.Format("Channel {0}", streamingChannel + 2); 
                }
                else
                    rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;
            }
            if(isOnCD && (isStreamPlaying || isEmptyStreamingChannel))
            {
                StopStream();
            }
            if (!isRadioOn && (isStreamPlaying || isEmptyStreamingChannel))
            {
                StopStream();
            }
            if (isOnCD && isCDin && isRadioOn && transform.Find("cd(item1)") == null && eject.gameObject.activeSelf && !loadingCD)
                Play();
            else
            {
                if (isCDplaying)
                    Stop();
               // StopStream();
            }
            if (isRadioOn && !isCDplaying)
            {
                if (volChanged != rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                    volChanged = rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value;
            }

            if (isCDplaying)
            {
                if (!waiting && volChanged != rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting)
                    rootCDplayer.GetChild(0).GetComponent<TextMesh>().text = string.Format("{0} - {1:D2}:{2:D2}", currentSong + 1, audioPlayer.Time().Minutes, audioPlayer.Time().Seconds);
                else
                    rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;

            }
            if (CDempty)
            {
                if (!waiting && volChanged != rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting)
                    rootCDplayer.GetChild(0).GetComponent<TextMesh>().text = "CD Empty";
                else
                    rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;
            }

        }
        IEnumerator volChangedWait()
        {
            yield return new WaitForSeconds(2f);
            waiting = false;
            volChanged = rootCDplayer.Find("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value;
        }
        IEnumerator LoadingCD()
        {
            yield return new WaitForSeconds(1f);
            rootCDplayer.Find("ButtonsCD/RadioCDSwitch").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("RadioOn").Value = false;
            if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/RadioChannels") != null)
                GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/RadioChannels").transform.SetParent(GameObject.Find("RADIO").transform, false);
            if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels") != null)
                GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels").transform.SetParent(GameObject.Find("RADIO").transform, false);
            PlayMakerFSM[] sees = rootCDplayer.Find("ButtonsCD/TrackChannelSwitch").GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM saas in sees)
            {
                if (saas.FsmName == "ChangeTrack")
                    saas.enabled = true;
                if (saas.FsmName == "ChangeChannel")
                    saas.enabled = false;
            }
            //Really?
            yield return new WaitForSeconds(4f);
            loadingCD = false;
        }
        void LoadCD()
        {
            if (cd.isPlaylist)
            {
                if (cd.CDPath.ToLower().EndsWith(".m3u", StringComparison.OrdinalIgnoreCase) || cd.CDPath.ToLower().EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                    audioFiles = Playlists.m3uPlaylist(cd.CDPath).ToArray();
                if (cd.CDPath.ToLower().EndsWith(".pls", StringComparison.OrdinalIgnoreCase))
                    audioFiles = Playlists.plsPlaylist(cd.CDPath).ToArray();
            }
            else
            {
                audioFiles = Directory.GetFiles(cd.CDPath, "*.*").
                    Where(file => file.ToLower().EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                                  file.ToLower().EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                  file.ToLower().EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                  file.ToLower().EndsWith(".aiff", StringComparison.OrdinalIgnoreCase)).ToArray();
            }
            eject.gameObject.SetActive(true);
            cd.inPlayer = true;
            loadingCD = true;
            StartCoroutine(LoadingCD());
        }
        void Update()
        {
            if (entered && ready)
            {
                if (Input.GetMouseButtonDown(0) && enteredCD != null)
                {
                    // carCDPlayer.isCDin = true;
                    insertedCD = enteredCD.gameObject;
                    cd = insertedCD.GetComponent<CD>();
                    insertedCD.GetComponent<Rigidbody>().isKinematic = true;
                    insertedCD.GetComponent<Rigidbody>().detectCollisions = false;
                    insertedCD.transform.SetParent(transform, false);
                    insertedCD.transform.localPosition = Vector3.zero;
                    insertedCD.transform.localEulerAngles = Vector3.zero;
                    insertedCD.transform.GetComponentInParent<Animation>().Play("cd_sled_in");
                    ready = false;
                    entered = false;
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = false;
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = false;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
                    LoadCD();
                }
            }
            if (isCDplaying)
            {
                if (!audioPlayer.audioSource.isPlaying && !notchanging)
                {
                    notchanging = true;
                    Next();
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1f);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == eject && isRadioOn)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Eject CD";
                    if (Input.GetMouseButtonDown(0))
                    {
                        PlayCDPlayerBeep();
                        eject.gameObject.SetActive(false);
                        transform.Find("cd(itemy)").localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-360f, 360f));
                        gameObject.GetComponentInParent<Animation>().Play("cd_sled_out");
                        rootCDplayer.GetChild(0).GetComponent<TextMesh>().text = "NO CD";
                    }
                    break;
                }

                if (hit.collider == nextTrack && isOnCD && isRadioOn)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Next/Previous Song";
                    if (Input.GetMouseButtonDown(0))
                    {
                        PlayCDPlayerBeep();
                        if (isCDin)
                            Next();
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        PlayCDPlayerBeep();
                        if (isCDin)
                            Previous();
                    }
                    break;
                }
                if (hit.collider == nextTrack && !isOnCD && isRadioOn)
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
                    break;

                }
            }
        }

        void OnTriggerStay(Collider col)
        {
            if (col.gameObject.name == "cd(itemy)" && col.transform.parent != null)
            {
                entered = true;
                if (!isRadioOn)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "CD Player is off";
                    ready = false;
                }
                else
                {
                    if (!isCDin)
                    {
                        enteredCD = col;
                        ready = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Play this CD";
                    }
                    else
                    {
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "CD already inside";
                        ready = false;
                    }
                }
            }
        }
        void OnTriggerExit()
        {
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
            ready = false;
            entered = false;
            enteredCD = null;
        }
    }
}
