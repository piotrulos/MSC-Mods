using MSCLoader;
using System;
using System.IO;
using UnityEngine;

//Standard unity MonoBehaviour class
namespace CDPlayer
{
    public class CD : MonoBehaviour
    {
        public CDCase cdCase;
        public string CDName;
        public string CDPath = null;
        public bool isPlaylist = false;
        public bool inCase = false;
        public bool inPlayer = false;
        public sbyte inPlayerID = -1;
        public Rigidbody rb;
        public BoxCollider trig;

        public string totalTime = "0:00";
        public string trackList = string.Empty;
        public int tracksCount = 0;


#if !Mini
        void Awake()
        {
            rb.detectCollisions = false;
        }

        public void LoadTrackData()
        {
            if (CDPath != null)
            {
                TimeSpan tt = new TimeSpan(0);
                string[] files = Directory.GetFiles(CDPath);
                for (int i = 0; i < files.Length; i++)
                {
                    if (!ModAudio.allowedExtensions.Contains(Path.GetExtension(files[i]).ToLower())) continue;
                    try
                    {
                        using (TagLib.File t = TagLib.File.Create(files[i]))
                        {
                            tt += t.Properties.Duration;
                            trackList += $"{t.Properties.Duration.Minutes:D1}:{t.Properties.Duration.Seconds:D2} - {t.Tag.Title ?? "Track " + (i + 1)}{Environment.NewLine}";

                        }
                    }
                    catch (Exception e)
                    {
                        ModConsole.Error($"Error reading file <b>{Path.GetFileName(files[i])}</b>: {e.Message}");
                        Console.WriteLine(e);
                    }
                    tracksCount++;
                }
                int remainingSeconds = (int)(tt.TotalSeconds % 60);
                totalTime = $"{(int)tt.TotalMinutes:D2}:{remainingSeconds:D2}";
            }
        }

        void FixedUpdate()
        {
            if (transform.parent != null && transform.parent.name == "ItemPivot" && rb.isKinematic)
            {
                rb.isKinematic = false;
            }

            if (!rb.detectCollisions && transform.parent != null)
            {
                if (transform.parent.name == "ItemPivot")
                {

                    rb.detectCollisions = true;
                    inPlayer = false;
                    inPlayerID = -1;
                    inCase = false;
                }
            }
            if (!rb.detectCollisions && transform.parent == null)
            {
                rb.detectCollisions = true;
                inPlayer = false;
                inPlayerID = -1;
                inCase = false;
            }
            if (transform.parent == null && !rb.useGravity)
            {
                rb.useGravity = true;
            }
        }
        public void PutInCase()
        {
            rb.detectCollisions = false;
            rb.isKinematic = true;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            inCase = true;
        }
#endif
    }
}
