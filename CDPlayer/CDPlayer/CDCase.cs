using UnityEngine;
using MSCLoader;
using HutongGames.PlayMaker;

namespace CDPlayer
{
    public class CDCase : MonoBehaviour
    {
        public string CDName;
        public Rigidbody rb;
        public CDTrigger cdt;
        public Animation animation;
        public Collider openColl;
        public MeshRenderer[] labels;
        public bool isOpen = false;
        public bool inRack = false;
        public int inRackNr = 0;
        public int inRackSlot = 0;

        private Camera mainCam;
#if !Mini
        FsmBool GUIuse;
        FsmString GUIinteraction;

        void Start()
        {
            GUIuse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
            GUIinteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
            mainCam = FsmVariables.GlobalVariables.FindFsmGameObject("POV").Value.GetComponent<Camera>();
        }
        void FixedUpdate()
        {
            if (transform.parent == null && !rb.detectCollisions)
            {
                rb.detectCollisions = true;
                rb.isKinematic = false;
                inRack = false;
                gameObject.name = "cd case(itemz)";
            }
            if (transform.parent == null && !rb.useGravity)
            {
                rb.useGravity = true;
            }

        }
        public void ChangeLabels(Texture2D t2d)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].material.mainTexture = t2d;
            }
        }
        void Update()
        {
            if (mainCam == null) return; //sometimes playmaker disable camera.main for whatever reason

            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1f))
            {
                if (hit.collider == openColl && !cdt.entered && !inRack)
                {
                    if (isOpen)
                    {
                        GUIuse.Value = true;
                        GUIinteraction.Value = "Close Case";
                        if (cInput.GetButtonDown("Use"))
                        {
                            animation.Play("cd_close");
                            isOpen = false;
                            MasterAudio.PlaySound3DAndForget("HouseFoley", transform, variationName: "cd_caseclose");
                            //"MasterAudio/HouseFoley/cd_caseopen"
                            if (cdt.transform.childCount > 0)
                            {
                                cdt.transform.GetChild(0).gameObject.layer = 0;
                            }
                        }
                    }
                    else
                    {
                        GUIuse.Value = true;
                        GUIinteraction.Value = "Open Case";
                        if (cInput.GetButtonDown("Use"))
                        {
                            animation.Play("cd_open");
                            isOpen = true;
                            MasterAudio.PlaySound3DAndForget("HouseFoley", transform, variationName: "cd_caseopen");
                            //"MasterAudio/HouseFoley/cd_caseopen"
                            if (cdt.transform.childCount > 0)
                            {
                                cdt.transform.GetChild(0).gameObject.MakePickable();
                            }
                        }
                    }
                }
            }
        }
#endif

    }
}