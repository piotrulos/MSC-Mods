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

        void FixedUpdate()
        {
            if (transform.parent == null && !gameObject.GetComponent<Rigidbody>().useGravity)
            {
                gameObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }
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
                    //assembleSound.Play(1f, 1f, gameObject.name, 1f, 1f, 1f, transform, false, 0f, false, true); //for whatever reason here doesn't work (just playmaker things...)
                    MasterAudio.PlaySound3DAndForget("CarBuilding", transform, variationName: "assemble");
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;
                    cdcase.GetComponent<CDCase>().inRack = true;
                    cdcase.GetComponent<CDCase>().inRackSlot = rackSlot;
                    cdcase.name = "cd case (" + (rackSlot + 1).ToString() + ")(itemz)";
                }
            }

        }

        private void OnTriggerStay(Collider other)
        {
            if (other.name == "cd case(itemz)" && other.transform.parent != null)
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