using System.Collections;
using UnityEngine;

namespace ModsShop;

//Just a way to "Bake" TextMesh into a texture.
internal class CamSticker : MonoBehaviour
{
    public Camera stickerGeneratorCam = null;
    public TextMesh stickerGeneratorText = null;
    public bool doIt = false;
    public Texture2D sticker;
    void Awake()
    {
        transform.SetParent(null);
    }
    internal void Generate(string name, int num, MeshRenderer mr)
    {
        stickerGeneratorText.text = $"{name}{System.Environment.NewLine}Items: {(num == -1 ? "???" : num)}";
        doIt = true;
        StartCoroutine(ApplySticker(mr));
    }
    private Texture2D GenerateSticker()
    {
        RenderTexture rt = new RenderTexture(512, 256, 32, RenderTextureFormat.ARGB32);
        stickerGeneratorCam.targetTexture = rt;

        Texture2D tex = new Texture2D(512, 256, TextureFormat.ARGB32, false);
        tex.name = "sticker_gen";
        stickerGeneratorCam.Render();
        RenderTexture.active = rt;

        tex.ReadPixels(new Rect(0, 0, 512, 256), 0, 0);
        tex.Apply();
        stickerGeneratorCam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        return tex;
    }
    IEnumerator ApplySticker(MeshRenderer mr)
    {
        while (doIt) yield return null; //Wait for single frame to be sure it's done.
        Texture2D t = sticker;
        mr.material.mainTexture = t;
    }
    void LateUpdate()
    {
        if (!doIt) return;
        sticker = GenerateSticker();
        doIt = false;
    }
}
