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
        int currentShelf = 0;
        bool displayedWarn = false;

        public void SpawnItem(GameObject displayGos, int gap = 1, int yoffset = 0)
        {
            if (currentShelf > 19)
            {
                if (!displayedWarn)
                {
#if !Mini
                    ModConsole.Warning("[ModsShop] Can't fit more items, we're full.");
#endif

                    displayedWarn = true;
                }
                return;
            }
          //  for (int i = 0; i < displayGos.Length; i++)
          //  {
                GameObject go = Instantiate(displayGos);
                var b = go.GetComponent<BoxCollider>().bounds.extents;
               // Debug.Log(b);
                if (Math.Abs(lastX - b.x) > 3f)
                {
                    lastX = 0f;
                    currentShelf++;
                    //if (currentShelf > 19)
                     //   break;
                }
                go.transform.SetParent(shelves[currentShelf].transform, false);

                if (lastX < 0)
                    lastX -= b.x;
                go.transform.localPosition = new Vector3(lastX, b.y, 0f);
                var gaps = (float)Math.Round((gap / 50f), 2);
               // Debug.Log((gap / 10));
                lastX -= Math.Abs(b.x) + gaps;
          //  }
            lastX -= 0.05f;
        }

    }
}