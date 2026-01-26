using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CDPlayer;

public class LabelGenerator : MonoBehaviour
{
    class Labels(MeshRenderer[] labels, string textb, string textc)
    {
        public MeshRenderer[] labels = labels;
        public string textb = textb;
        public string textc = textc;
    }
    public Camera generatorCam = null;
    public Text boxText = null;
    public Text cdText = null;
    public bool doIt = false;
    public Texture2D label;

    Queue<Labels> queue = new Queue<Labels>();
    Coroutine generator = null;

    internal void AddToQueue(string textb, string textc, MeshRenderer[] mr)
    {
        Labels l = new Labels(mr, textb, textc);
        queue.Enqueue(l);
        if (generator == null)
            generator = StartCoroutine(GenerateLabelsFromQueue());

    }

    private Texture2D GenerateSticker()
    {
        Texture2D tex = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        tex.name = "text_coverart";
        RenderTexture.active = generatorCam.targetTexture;
        generatorCam.Render();
        tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }
    IEnumerator GenerateLabelsFromQueue()
    {
        yield return null;
        while (queue.Count > 0)
        {
            Labels l = queue.Dequeue();
            boxText.text = $"{l.textb}";
            cdText.text = $"{l.textc}";
            doIt = true;
            while (doIt) yield return null; //wait for label to be generated
            MeshRenderer[] mr = l.labels;
            for (int i = 0; i < mr.Length; i++)
            {
                mr[i].material.mainTexture = label;
            }
            yield return null;
        }
        generator = null;
    }
    void LateUpdate()
    {
        if (!doIt) return;
        label = GenerateSticker();
        doIt = false;
    }

}
