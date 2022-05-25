using UnityEngine;

namespace ModsShop
{
    public class ShopRefs : MonoBehaviour
    {
        public AutoShelves autoShelves;
        public CustomShelves customShelves;
        public CashRegister cashRegister;
        public RaycastHit hit;
        public bool hitted;
#if !Mini
        void Update()
        {
            if (Camera.main == null) return;
            hitted = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f);
        }
#endif
    }
}