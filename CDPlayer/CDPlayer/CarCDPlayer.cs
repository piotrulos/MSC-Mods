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
        public bool CDempty { get; private set; }

        void Start()
        {
            audioPlayer = gameObject.AddComponent<ModAudio>();
            audioStreamPlayer = gameObject.AddComponent<ModAudioStream>();
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
            eject = transform.FindChild("ButtonsCD/Eject").GetComponent<SphereCollider>();
            nextTrack = transform.FindChild("ButtonsCD/TrackChannelSwitch").GetComponent<SphereCollider>();
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
                audioStreamPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/CDPlayer").GetComponent<AudioSource>();
                radioCh = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels").GetComponent<PlayMakerFSM>();
            }
            radioCh.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("OnMuteChannel1").Value = true;
            radioCh.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("OnMuteFolk").Value = true;
            radioCh.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("OnStatic").Value = true;

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
                        audioPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/CDPlayer").GetComponent<AudioSource>();
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
            if (transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Ok").Value &&
                transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value > 0f)
                isRadioOn = true;
            else
                isRadioOn = false;

            if (!transform.FindChild("ButtonsCD/RadioCDSwitch").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("RadioOn").Value)
                isOnCD = true;
            else
                isOnCD = false;

            if (transform.FindChild("Sled/cd_sled_pivot").transform.childCount != 0 && eject.gameObject.activeSelf && transform.FindChild("Sled/cd_sled_pivot/cd(item1)") == null)
                isCDin = true;
            else
                isCDin = false;
            if (loadingCD)
            {
                if (!waiting && volChanged != transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting)
                    transform.GetChild(0).GetComponent<TextMesh>().text = "Loading...";
                else
                    transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;

            }
            if (isStreamPlaying)
            {
                if (scroolDone)
                {
                    StartCoroutine("RDSsimulator");
                    scroolDone = false;
                }
            }
            if (isStreamPlaying || isEmptyStreamingChannel)
            {
                if (!waiting && volChanged != transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting && isRadioOn)
                {
                    if((bool)cdplayer.RDSsim.GetValue() && !isEmptyStreamingChannel)
                        transform.GetChild(0).GetComponent<TextMesh>().text = DisplayRDS;
                    else
                        transform.GetChild(0).GetComponent<TextMesh>().text = string.Format("Channel {0}", streamingChannel + 2); 
                }
                else
                    transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;
            }
            if(isOnCD && isStreamPlaying)
            {
                StopStream();
            }
            if (!isRadioOn && isStreamPlaying)
            {
                StopStream();
            }
            if (isOnCD && isCDin && isRadioOn && transform.FindChild("Sled/cd_sled_pivot/cd(item1)") == null && eject.gameObject.activeSelf && !loadingCD)
                Play();
            else
            {
                if (isCDplaying)
                    Stop();
               // StopStream();
            }
            if (isRadioOn && !isCDplaying)
            {
                if (volChanged != transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                    volChanged = transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value;
            }

            if (isCDplaying)
            {
                if (!waiting && volChanged != transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting)
                    transform.GetChild(0).GetComponent<TextMesh>().text = string.Format("{0} - {1:D2}:{2:D2}", currentSong + 1, audioPlayer.Time().Minutes, audioPlayer.Time().Seconds);
                else
                    transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;

            }
            if (CDempty)
            {
                if (!waiting && volChanged != transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value)
                {
                    waiting = true;
                    StartCoroutine(volChangedWait());
                }
                if (!waiting)
                    transform.GetChild(0).GetComponent<TextMesh>().text = "CD Empty";
                else
                    transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Channel").Value = transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmString("Data2").Value;
            }

        }
        IEnumerator volChangedWait()
        {
            yield return new WaitForSeconds(2f);
            waiting = false;
            volChanged = transform.FindChild("ButtonsCD/RadioVolume").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Volume").Value;
        }
        IEnumerator LoadingCD()
        {
            yield return new WaitForSeconds(1f);
            transform.FindChild("ButtonsCD/RadioCDSwitch").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("RadioOn").Value = false;
            if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/RadioChannels") != null)
                GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/RadioChannels").transform.SetParent(GameObject.Find("RADIO").transform, false);
            if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels") != null)
                GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels").transform.SetParent(GameObject.Find("RADIO").transform, false);
            PlayMakerFSM[] sees = transform.FindChild("ButtonsCD/TrackChannelSwitch").GetComponents<PlayMakerFSM>();
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
        void Update()
        {
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
                        transform.FindChild("Sled/cd_sled_pivot/cd(itemy)").localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-360f, 360f));
                        transform.FindChild("Sled/cd_sled_pivot").GetComponentInParent<Animation>().Play("cd_sled_out");
                        transform.GetChild(0).GetComponent<TextMesh>().text = "NO CD";
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
        void OnTriggerEnter(Collider other)
        {

            if (other.gameObject.name == "cd(itemy)" && isRadioOn && !isCDin)
            {
                currentSong = 0;
                other.GetComponent<Rigidbody>().isKinematic = true;
                other.GetComponent<Rigidbody>().detectCollisions = false;
                other.transform.SetParent(transform.FindChild("Sled/cd_sled_pivot").transform, false);
                other.transform.localPosition = Vector3.zero;
                other.transform.localEulerAngles = Vector3.zero;
                other.transform.GetComponentInParent<Animation>().Play("cd_sled_in");
                if (other.GetComponent<CD>().isPlaylist)
                {
                    if (other.GetComponent<CD>().CDPath.ToLower().EndsWith(".m3u", StringComparison.OrdinalIgnoreCase) || other.GetComponent<CD>().CDPath.ToLower().EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                        audioFiles = Playlists.m3uPlaylist(other.GetComponent<CD>().CDPath).ToArray();
                    if (other.GetComponent<CD>().CDPath.ToLower().EndsWith(".pls", StringComparison.OrdinalIgnoreCase))
                        audioFiles = Playlists.plsPlaylist(other.GetComponent<CD>().CDPath).ToArray();
                }
                else
                {
                    audioFiles = Directory.GetFiles(other.GetComponent<CD>().CDPath, "*.*").
                        Where(file => file.ToLower().EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                                      file.ToLower().EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                      file.ToLower().EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                      file.ToLower().EndsWith(".aiff", StringComparison.OrdinalIgnoreCase)).ToArray();
                }
                eject.gameObject.SetActive(true);
                other.GetComponent<CD>().inPlayer = true;
                loadingCD = true;
                StartCoroutine(LoadingCD());
            }
        }
    }
}
