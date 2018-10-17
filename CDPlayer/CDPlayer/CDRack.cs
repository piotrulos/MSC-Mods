using MSCLoader;
using UnityEngine;

namespace CDPlayer
{
    public class CDRack : MonoBehaviour
    {
        private bool entered;
        private GameObject cdcase;
        private int rackSlot;
        public bool purchased;

        void Update()
        {
            if (entered)
            {
                PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = true;
                PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Put case into rack";
                if (Input.GetMouseButtonDown(0))
                {
                    if (cdcase.GetComponent<CDCase>().isOpen)
                    {
                        entered = false;
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = false;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
                        return;
                    }
                    cdcase.transform.SetParent(transform.GetChild(rackSlot), false);
                    cdcase.GetComponent<Rigidbody>().isKinematic = true;
                    cdcase.GetComponent<Rigidbody>().detectCollisions = false;
                    cdcase.transform.localPosition = Vector3.zero;
                    cdcase.transform.localEulerAngles = Vector3.zero;
                    entered = false;
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = false;
                    GameObject.Find("MasterAudio/CarBuilding/assemble").GetComponent<AudioSource>().Play();
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
                    cdcase.GetComponent<CDCase>().inRack = true;
                    cdcase.GetComponent<CDCase>().inRackSlot = rackSlot;
                    cdcase.name = "cd case (" + (rackSlot + 1).ToString() + ")(item2)";
                }
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name == "cd case(item2)" && other.transform.parent != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (transform.GetChild(i).childCount == 0)
                    {
                        cdcase = other.gameObject;
                        entered = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Put case into rack";
                        rackSlot = i;
                        break;
                    }
                }
                if (!entered)
                {
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = true;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Rack is full";
                }
            }
        }
        private void OnTriggerExit()
        {
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
            entered = false;
        }
    }
}