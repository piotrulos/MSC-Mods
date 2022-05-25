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

        void Update()
        {
            if (Camera.main == null) return;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1f))
            {
                if (hit.transform.gameObject == gameObject)
                {
#if !Mini
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
#endif
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (isOpen)
                        {
                            animation.Play("door close");
                            MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_close");
                            isOpen = false;
                        }
                        else
                        {
                            animation.Play("door open");
                            MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_open");
                            isOpen = true;
                        }
                    }
                }
            }
        }
    }
}