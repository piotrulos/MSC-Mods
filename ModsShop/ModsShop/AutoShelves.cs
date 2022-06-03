#if !Mini
using MSCLoader;
#endif
using System;
using UnityEngine;

namespace ModsShop
{
    public class AutoShelves : MonoBehaviour
    {
        public GameObject[] shelves;
        float lastX = 0f;
        string lastModID = string.Empty;
        int currentShelf = 0;
        bool displayedWarn = false;
#if !Mini
        public void SpawnItem(string modID, GameObject displayGos, int gap = 2, int yoffset = 0)
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

            var b = displayGos.GetComponent<BoxCollider>().bounds.extents;
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
            displayGos.transform.SetParent(shelves[currentShelf].transform, false);
            var gaps = (float)Math.Round((gap / 50f), 2);

            if (lastX < 0)
                lastX -= b.x + gaps;
            displayGos.transform.localPosition = new Vector3(lastX, b.y, 0f);
            if (currentShelf % 2 != 0)
                displayGos.transform.localEulerAngles = new Vector3(displayGos.transform.localEulerAngles.x, displayGos.transform.localEulerAngles.y + 180f, displayGos.transform.localEulerAngles.z);
            // Debug.Log((gap / 10));
            lastX -= Math.Abs(b.x);
            lastModID = modID;
        }
#endif
    }
}