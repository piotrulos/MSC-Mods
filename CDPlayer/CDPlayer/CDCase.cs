using UnityEngine;
using MSCLoader;
using HutongGames.PlayMaker;

namespace CDPlayer
{
    public class CDCase : MonoBehaviour
    {
        public CD cd;
        public string CDName;
        public Rigidbody rb;
        public CDTrigger cdt;
        public Animation animation;
        public Collider openColl;
        public MeshRenderer[] labels;
        public bool isOpen = false;
        public bool inRack = false;
        public int inRackNr = 0;
        public byte inRackSlot = 0;
        public TextMesh[] labelsText;
#if !Mini
        FsmBool GUIuse;
        FsmString GUIinteraction;

        void Start()
        {
            GUIuse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
            GUIinteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
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
        internal void LoadInRack(GameObject rack, byte rackSlot, int rackNr)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            transform.SetParent(rack.transform.GetChild(rackSlot), false);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            inRack = true;
            inRackSlot = rackSlot;
            inRackNr = rackNr;
            name = "cd case (" + (rackSlot + 1).ToString() + ")(itemz)";
        }
        public void ChangeLabels(Texture2D t2d)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].material.mainTexture = t2d;
            }
        }
        public void SetTextLabels()
        {
            if (CDName.Length <= 30)
                labelsText[0].text = CDName;
            else
                labelsText[0].text = CDName.Substring(0, 27) + "...";
            if (CDName.Length <= 22)
                labelsText[1].text = CDName;
            else
                labelsText[1].text = CDName.Substring(0, 19) + "...";
            labelsText[0].gameObject.SetActive(true);
            labelsText[1].gameObject.SetActive(true);
        }
        void Update()
        {
            if (UnifiedRaycast.GetHitInteraction(openColl) && !cdt.entered && !inRack)
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
#endif

    }
}