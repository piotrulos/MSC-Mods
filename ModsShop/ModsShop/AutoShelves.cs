#if !Mini
using MSCLoader;
#endif
using System;
using UnityEngine;

namespace ModsShop;

public class AutoShelves : MonoBehaviour
{
    public GameObject[] shelves;
    public GameObject shelfSticker;
#if !Mini
    private float lastX = 0f;
    private string lastModID = string.Empty;
    private int currentShelf = 0;
    private bool displayedWarn = false;

    private int numberOfItems = 0;

    private MeshRenderer prevMesh = null;
    private string prevMod = string.Empty;
    internal void SpawnItem(string modID, string modName, GameObject displayGO, int gap = 2, int yoffset = 0)
    {
        if (currentShelf > shelves.Length - 1)
        {
            if (!displayedWarn)
            {
                ModConsole.Warning("[ModsShop] Can't fit more items, we're full.");
                displayedWarn = true;
            }
            return;
        }

        Vector3 b = displayGO.GetComponent<BoxCollider>().bounds.extents;
        if (lastModID != string.Empty)
        {
            if (lastModID != modID)
            {
                lastX -= 0.05f;
            }
        }
        // Debug.Log(b);
        if (Math.Abs(lastX - b.x) > 4.3f)
        {
            lastX = 0f;
            currentShelf++;
            if (currentShelf > shelves.Length - 1)
                return;
        }
        displayGO.transform.SetParent(shelves[currentShelf].transform, false);
        float gaps = (float)Math.Round((gap / 50f), 2);

        if (lastX < 0)
            lastX -= b.x + gaps;
        displayGO.transform.localPosition = new Vector3(lastX, b.y, 0f);
        if (currentShelf % 2 != 0)
            displayGO.transform.localEulerAngles = new Vector3(displayGO.transform.localEulerAngles.x, displayGO.transform.localEulerAngles.y + 180f, displayGO.transform.localEulerAngles.z);
        // Debug.Log((gap / 10));
        if (lastModID == string.Empty || lastModID != modID)
        {
            UpdateLastSticker();
            numberOfItems = 0;
            GameObject sticker = GameObject.Instantiate(shelfSticker);
            sticker.transform.SetParent(shelves[currentShelf].transform, false);
            sticker.transform.localPosition = new Vector3(lastX, -0.026f, 0.162f);
            if (currentShelf % 2 != 0)
            {
                sticker.transform.localPosition = new Vector3(lastX, -0.026f, -0.162f);
                sticker.transform.localEulerAngles = new Vector3(sticker.transform.localEulerAngles.x, sticker.transform.localEulerAngles.y + 180f, sticker.transform.localEulerAngles.z);
            }
            prevMesh = sticker.transform.GetChild(0).GetComponent<MeshRenderer>();
            prevMod = modName;
        }
        numberOfItems++;
        lastX -= Math.Abs(b.x);
        lastModID = modID;
    }

    internal void UpdateLastSticker()
    {
        if (prevMesh == null) return;
        ModsShop.GetShopReference().shopRefs.stickerGeneratorCam.GetComponent<CamSticker>().Generate(prevMod, numberOfItems, prevMesh);
    }

#endif
}