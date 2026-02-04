using MSCLoader;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace CDPlayer;

/*public class StickerCam: MonoBehaviour
{
    public Texture2D label;
    public bool doIt = false;
    void OnPreRender()
    {
       ModConsole.Warning("OnPreRender");
    }
    void OnPostRender()
    {
        ModConsole.Warning("OnPostRender");
    }
}*/
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
   // public Texture2D label;

    Queue<Labels> queue = new Queue<Labels>();
    Coroutine generator = null;
    void Awake()
    {
      //  generatorCam.gameObject.AddComponent<StickerCam>();
    }
    internal void AddToQueue(string textb, string textc, MeshRenderer[] mr)
    {
        Labels l = new Labels(mr, textb, textc);
        queue.Enqueue(l);
        if (generator == null)
            generator = StartCoroutine(GenerateLabelsFromQueue());

    }

    private Texture2D GenerateSticker(int s)
    {
        Texture2D tex = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        tex.name = $"text_coverart{s}";
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = generatorCam.targetTexture;
        generatorCam.Render();
        tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        tex.Apply();
        RenderTexture.active = currentRT;
        return tex;
    }

    IEnumerator GenerateLabelsFromQueue()
    {
        yield return null;
        while (queue.Count > 0)
        {
            generatorCam.targetTexture.DiscardContents();
            Labels l = queue.Dequeue();
            boxText.text = $"{l.textb}";
            cdText.text = $"{l.textc}";
            yield return null;
            yield return new WaitForEndOfFrame();
            Texture2D label = GenerateSticker(queue.Count);
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

        doIt = false;
    }

}
