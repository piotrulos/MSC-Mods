using System.Collections;
using UnityEngine;

//Standard unity MonoBehaviour class
namespace CDPlayer
{
    public class CDTrigger : MonoBehaviour
    {
        public CDCase CDcase;
        public CD cd;
        bool ready;
        public bool entered = false;
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

        IEnumerator PutCDBack()
        {
            yield return null;
            //"MasterAudio/HouseFoley/cd_cdin"
            cd.transform.SetParent(transform, false);
            cd.PutInCase();
            MasterAudio.PlaySound3DAndForget("HouseFoley", transform, variationName: "cd_cdin");
            GUIassemble.Value = false;
            GUIdisassemble.Value = false;
            GUIinteraction.Value = string.Empty;
        }
        void Update()
        {
            if (entered && ready)
            {
                GUIassemble.Value = true;
                GUIinteraction.Value = "Put CD back";
                if (Input.GetMouseButtonDown(0))
                {
                    ready = false;
                    entered = false;
                    StartCoroutine(PutCDBack());
                }
            }
        }

        void OnTriggerStay(Collider col)
        {
            if (col.transform.parent == null) return;
            if (!col.isTrigger) return;
            if (col.name == "cd(itemz)" && CDcase.isOpen)
            {
                if (col.transform.parent.name != "ItemPivot") return;
                entered = true;
                if (col != cd.trig)
                {
                    GUIdisassemble.Value = true;
                    GUIinteraction.Value = "Wrong Case";
                    ready = false;
                    return;
                }
                GUIassemble.Value = true;
                GUIinteraction.Value = "Put CD back";
                ready = true;
            }
        }

        void OnTriggerExit()
        {
            GUIassemble.Value = false;
            GUIdisassemble.Value = false;
            GUIinteraction.Value = string.Empty;
            ready = false;
            entered = false;
        }
#endif

    }
}
