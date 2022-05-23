#if !Mini 
using MSCLoader;
#endif
using UnityEngine;

namespace ModsShop
{
    public class CustomShelves : MonoBehaviour
    {
        public GameObject[] placeAreas;
        int lastShelf = 0;
        bool displayedWarn = false;

        public void InsertSlelf(GameObject placeArea)
        {
            if(lastShelf > placeAreas.Length - 1)
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
            placeArea.transform.SetParent(placeAreas[lastShelf].transform, false);
            lastShelf++;
        }
    }
}