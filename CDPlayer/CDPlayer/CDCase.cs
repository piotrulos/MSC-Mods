using UnityEngine;

namespace CDPlayer
{
    public class CDCase : MonoBehaviour
    {
        public string CDName;
        public bool isOpen = false;
        public bool ready = false;
       // public CDPlayer cdp;
        public bool inRack;
        public int inRackNr = 0;
        public int inRackSlot = 0;
        private CDTrigger cdt;
        private Animator animator;
        public bool purchased;

        void Start()
        {
            animator = transform.GetChild(0).GetComponent<Animator>();
            cdt = transform.GetChild(2).gameObject.AddComponent<CDTrigger>();
            cdt.CDcase = this;
        }
        void FixedUpdate()
        {
            if (transform.parent == null && !gameObject.GetComponent<Rigidbody>().detectCollisions)
            {
                gameObject.GetComponent<Rigidbody>().detectCollisions = true;
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                inRack = false;
                gameObject.name = "cd case(itemy)";
            }
            if (transform.parent == null && !gameObject.GetComponent<Rigidbody>().useGravity)
            {
                gameObject.GetComponent<Rigidbody>().useGravity = true;
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
                                animator.SetBool("open", false);
                                isOpen = false;

                                if (transform.GetChild(2).childCount > 0)
                                {
                                    transform.GetChild(2).GetChild(0).gameObject.layer = 0;
                                }
                            }
                        }
                        else
                        {
                            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                            PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Open Case";
                            if (cInput.GetButtonDown("Use"))
                            {
                                animator.SetBool("open", true);
                                isOpen = true;

                                if (transform.GetChild(2).childCount > 0)
                                {
                                    MSCLoader.LoadAssets.MakeGameObjectPickable(transform.GetChild(2).GetChild(0).gameObject);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}