#if !Mini
using MSCLoader;
#endif
using System;
using UnityEngine;

namespace ModsShop;

public class AutoShelves : MonoBehaviour
{
    public GameObject[] shelves;

#if !Mini
    private float lastX = 0f;
    private string lastModID = string.Empty;
    private int currentShelf = 0;
    private bool displayedWarn = false;
    internal void SpawnItem(string modID, GameObject displayGO, int gap = 2, int yoffset = 0)
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
        lastX -= Math.Abs(b.x);
        lastModID = modID;
    }
#endif
}