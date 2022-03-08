#if !Mini 
using MSCLoader;
#endif
using UnityEngine;

namespace ModsShop
{
    public class CustomShelves : MonoBehaviour
    {
        public GameObject[] placeAreas;
        public int availableSpace = 28;
        int lastShelf = 0;
        bool displayedWarn = false;

        public void InsertSlelf(GameObject placeArea)
        {
            if(lastShelf > availableSpace)
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