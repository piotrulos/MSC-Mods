using MSCLoader;
using UnityEngine;

namespace ModsShop
{
    public class Door : MonoBehaviour
    {
        public Animation animation;
        public bool isOpen = false;

        void OnMouseExit()
        {
#if !Mini
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = false;
#endif
        }
        public void CloseDoor()
        {
            animation.Play("door close");
            MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_close");
            isOpen = false;
        }
        public void OpenDoor()
        {
            animation.Play("door open");
            MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_open");
            isOpen = true;
        }
#if !Mini
        void Update()
        {
            if (Camera.main == null) return;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1f))
            {
                if (hit.transform.gameObject == gameObject)
                {

                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (ModsShop.GetShopReference().shopRefs.isShopClosed)
                        {
                            MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_locked");
                            return;
                        }
                        if (isOpen)
                        {
                            CloseDoor();
                        }
                        else
                        {
                            OpenDoor();
                        }
                    }
                }
            }
        }
#endif
    }
}