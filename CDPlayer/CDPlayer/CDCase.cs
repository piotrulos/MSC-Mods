using UnityEngine;
using MSCLoader;

namespace CDPlayer
{
    public class CDCase : MonoBehaviour
    {
        public string CDName;
        public Rigidbody rb;
        public CDTrigger cdt;
        public Animation animation;
        public bool isOpen = false;
        public bool ready = false;
        public bool inRack;
        public int inRackNr = 0;
        public int inRackSlot = 0;
        public bool purchased;

        void Start()
        {
            cdt.CDcase = this;
        }
        void FixedUpdate()
        {
            if (transform.parent == null && !gameObject.GetComponent<Rigidbody>().detectCollisions)
            {
                rb.detectCollisions = true;
                rb.isKinematic = false;
                inRack = false;
                gameObject.name = "cd case(itemz)";
            }
            if (transform.parent == null && !gameObject.GetComponent<Rigidbody>().useGravity)
            {
                rb.useGravity = true;
            }

        }
        void Update()
        {
            if (Camera.main != null) //sometimes playmaker disable camera.main for whatever reason
            {
                RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 1f);

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider == transform.GetChild(0).GetComponent<Collider>() && !cdt.entered && hits[i].transform.parent == null)
                    {
                        if (isOpen)
                        {
                            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Close Case";
                            if (cInput.GetButtonDown("Use"))
                            {
                                animation.Play("cd_close");
                                isOpen = false;

                                if (cdt.transform.childCount > 0)
                                {
                                    cdt.transform.GetChild(0).gameObject.layer = 0;
                                }
                            }
                        }
                        else
                        {
                            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Open Case";
                            if (cInput.GetButtonDown("Use"))
                            {
                                animation.Play("cd_open");
                                isOpen = true;

                                if (cdt.transform.childCount > 0)
                                {
                                   cdt.transform.GetChild(0).gameObject.MakePickable();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}