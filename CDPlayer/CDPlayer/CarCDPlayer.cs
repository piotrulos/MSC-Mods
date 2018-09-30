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

        public bool CDempty { get; private set; }

        void Start()
        {
            audioPlayer = gameObject.AddComponent<ModAudio>();
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
            eject = transform.FindChild("ButtonsCD/Eject").GetComponent<SphereCollider>();
            nextTrack = transform.FindChild("ButtonsCD/TrackChannelSwitch").GetComponent<SphereCollider>();
        }
        void Play()
        {
            if (!isCDplaying)
            {
                if (audioFiles.Length > 0)
                {
                    CDempty = false;
                    isCDplaying = true;
                    if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/CD1") != null)
                        audioPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/CD1").GetComponent<AudioSource>();
                    else
                        audioPlayer.audioSource = GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/CD1").GetComponent<AudioSource>();
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
            if(currentSong >= audioFiles.Length)
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
                    currentSong = audioFiles.Length-1;
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
            if (isOnCD && isCDin && isRadioOn && transform.FindChild("Sled/cd_sled_pivot/cd(item1)") == null && eject.gameObject.activeSelf && !loadingCD)
                Play();
            else
            {
                if (isCDplaying)
                    Stop();
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
                GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerDash/RadioChannels").transform.SetParent(GameObject.Find("RADIO").transform,false);
            if (GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels") != null)
                GameObject.Find("SATSUMA(557kg, 248)/Electricity/SpeakerBass/RadioChannels").transform.SetParent(GameObject.Find("RADIO").transform, false);
            PlayMakerFSM[] sees = transform.FindChild("ButtonsCD/TrackChannelSwitch").GetComponents<PlayMakerFSM>();
            foreach(PlayMakerFSM saas in sees)
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
                if (hit.collider == eject)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Eject CD";
                    if (Input.GetMouseButtonDown(0))
                    {
                        eject.gameObject.SetActive(false);
                        transform.FindChild("Sled/cd_sled_pivot/cd(item2)").localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-360f, 360f));
                        transform.FindChild("Sled/cd_sled_pivot").GetComponentInParent<Animation>().Play("cd_sled_out");
                        transform.GetChild(0).GetComponent<TextMesh>().text = "NO CD";
                    }
                    break;
                }

                if (hit.collider == nextTrack && isOnCD)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Next/Previous Song";
                    if (Input.GetMouseButtonDown(0))
                    {
                        if(isCDin)
                            Next();
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        if (isCDin)
                            Previous();
                    }
                    break;
                }
            }
        }
        void OnTriggerEnter(Collider other)
        {

            if (other.gameObject.name == "cd(item2)" && isRadioOn && !isCDin)
            {
                currentSong = 0;
                other.GetComponent<Rigidbody>().isKinematic = true;
                other.GetComponent<Rigidbody>().detectCollisions = false;
                other.transform.SetParent(transform.FindChild("Sled/cd_sled_pivot").transform, false);
                other.transform.localPosition = Vector3.zero;
                other.transform.localEulerAngles = Vector3.zero;
                other.transform.GetComponentInParent<Animation>().Play("cd_sled_in");
                audioFiles = Directory.GetFiles(other.GetComponent<CD>().CDPath, "*.*").
                    Where(file => file.ToLower().EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                                  file.ToLower().EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                  file.ToLower().EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                  file.ToLower().EndsWith(".aiff", StringComparison.OrdinalIgnoreCase)).ToArray();
                eject.gameObject.SetActive(true);
                other.GetComponent<CD>().inPlayer = true;
                loadingCD = true;
                StartCoroutine(LoadingCD());
            }
        }
    }
}
