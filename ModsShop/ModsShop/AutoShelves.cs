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
            if (currentShelf > 19)
            {
                if (!displayedWarn)
                {

                    ModConsole.Warning("[ModsShop] Can't fit more items, we're full.");


                    displayedWarn = true;
                }
                return;
            }
            //  for (int i = 0; i < displayGos.Length; i++)
            //  {
            //  GameObject go = Instantiate(displayGos);
            var b = displayGos.GetComponent<BoxCollider>().bounds.extents;
            if(lastModID != string.Empty)
            {
                if(lastModID != modID)
                {
                    lastX -= 0.05f;
                }
            }
            // Debug.Log(b);
            if (Math.Abs(lastX - b.x) > 3f)
            {
                lastX = 0f;
                currentShelf++;
                //if (currentShelf > 19)
                //   break;
            }
            displayGos.transform.SetParent(shelves[currentShelf].transform, false);
            if (lastX < 0)
                lastX -= b.x;
            var gaps = (float)Math.Round((gap / 50f), 2);
            displayGos.transform.localPosition = new Vector3(lastX - gaps, b.y, 0f);
            // Debug.Log((gap / 10));
            lastX -= Math.Abs(b.x);
            // lastX -= Math.Abs(b.x) + gaps;
            //  }
            // lastX -= 0.05f;
            lastModID = modID;
        }
#endif
    }
}