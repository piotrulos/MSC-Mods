using UnityEngine;

//Standard unity MonoBehaviour class
namespace CDPlayer
{
    public class CDTrigger : MonoBehaviour
    {
        public CDCase CDcase;
        bool ready;
        Collider cd;
        public bool entered = false;

        void Update()
        {
            if (entered && !ready)
            {
                PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = true;
                PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Wrong Case";
            }
            if (entered && ready)
            {
                PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = true;
                PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Put CD back";
                if (Input.GetMouseButtonDown(0) && cd != null)
                {
                    cd.GetComponent<CD>().InCase();
                    cd.transform.SetParent(CDcase.transform.GetChild(2), false);
                    ready = false;
                    entered = false;
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = false;
                    PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = false;
                    PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = string.Empty;

                }
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.name == "cd(itemy)" && CDcase.isOpen && col.transform.parent == null)
                entered = false;
            if (col.name == "cd(itemy)" && CDcase.isOpen && col.transform.parent != null)
            {
                if (!col.GetComponent<CD>().inCase)
                {
                    entered = true;
                    if (col.GetComponent<CD>().CDName == CDcase.CDName)
                    {
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Put CD back";
                        ready = true;
                        cd = col;

                    }
                    else
                    {
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Wrong Case";
                        ready = false;
                        cd = null;
                    }
                }
            }

        }

        void OnTriggerExit()
        {
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIassemble").Value = false;
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIdisassemble").Value = false;
            ready = false;
            entered = false;
            cd = null;
        }

    }
}
