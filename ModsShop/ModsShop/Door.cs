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

        // Update is called once per frame
        void FixedUpdate()
        {
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
                            isOpen = false;
                        }
                        else
                        {
                            animation.Play("door open");
                            isOpen = true;
                        }
                    }
                }
            }
        }
    }
}