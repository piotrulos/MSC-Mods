using System.Collections;
using UnityEngine;

namespace CDPlayer
{
    public class CDRack : MonoBehaviour
    {
        public int rackNr = 0;
        private bool entered;
        private GameObject cdcase;
        private byte rackSlot;
#if !Mini
        HutongGames.PlayMaker.FsmBool GUIassemble;
        HutongGames.PlayMaker.FsmBool GUIdisassemble;
        HutongGames.PlayMaker.FsmString GUIinteraction;
        void Start()
        {
            GUIassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble");
            GUIdisassemble = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble");
            GUIinteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
        }

        void FixedUpdate()
        {
            if (transform.parent == null && !gameObject.GetComponent<Rigidbody>().useGravity)
            {
                gameObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }

        IEnumerator PutCDinRack()
        {
            yield return null;
            GUIassemble.Value = false;
            GUIinteraction.Value = string.Empty;
            cdcase.transform.SetParent(transform.GetChild(rackSlot), false);
            cdcase.GetComponent<Rigidbody>().isKinematic = true;
            cdcase.GetComponent<Rigidbody>().detectCollisions = false;
            cdcase.transform.localPosition = Vector3.zero;
            cdcase.transform.localEulerAngles = Vector3.zero;
            cdcase.GetComponent<CDCase>().inRack = true;
            cdcase.GetComponent<CDCase>().inRackNr = rackNr;
            cdcase.GetComponent<CDCase>().inRackSlot = rackSlot;
            cdcase.name = "cd case (" + (rackSlot + 1).ToString() + ")(itemz)";
            MasterAudio.PlaySound3DAndForget("CarBuilding", transform, variationName: "assemble");
        }
        void Update()
        {
            if (entered)
            {
                GUIassemble.Value = true;
                GUIinteraction.Value = "Put case into rack";
                if (Input.GetMouseButtonDown(0))
                {
                    if (cdcase.GetComponent<CDCase>().isOpen)
                    {
                        entered = false;
                        GUIassemble.Value = false;
                        GUIinteraction.Value = string.Empty;
                        return;
                    }
                    entered = false;
                    StartCoroutine(PutCDinRack());

                }
            }

        }

        private void OnTriggerStay(Collider other)
        {
            if (other.transform.parent == null) return;
            if (other.name == "cd case(itemz)")
            {
                if (other.transform.parent.name != "ItemPivot") return;

                for (byte i = 0; i < 10; i++)
                {
                    if (transform.GetChild(i).childCount == 0)
                    {
                        cdcase = other.gameObject;
                        entered = true;
                        GUIassemble.Value = true;
                        GUIinteraction.Value = "Put case into rack";
                        rackSlot = i;
                        break;
                    }
                }
                if (!entered)
                {
                    GUIdisassemble.Value = true;
                    GUIinteraction.Value = "Rack is full";
                }
            }
        }
        private void OnTriggerExit()
        {
            GUIassemble.Value = false;
            GUIdisassemble.Value = false;
            GUIinteraction.Value = string.Empty;
            entered = false;
        }
#endif

    }
}