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
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (ready)
            {
                if (Input.GetMouseButtonDown(0) && cd != null)
                {
                    cd.GetComponent<CD>().inCase = true;
                    cd.transform.SetParent(CDcase.transform.GetChild(2), false);
                }
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.name == "cd(item2)" && CDcase.isOpen)
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
