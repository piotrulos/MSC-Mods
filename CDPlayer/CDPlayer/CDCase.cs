using UnityEngine;

namespace CDPlayer
{
    public class CDCase : MonoBehaviour
    {
        public string CDName;
        public bool isOpen = false;
        CDTrigger cdt;
        public bool ready = false;
        public CDPlayer cdp;
        void Start()
        {
            //Destroy all playmakers
            foreach (PlayMakerFSM c in gameObject.GetComponentsInChildren<PlayMakerFSM>())
                Destroy(c);
            foreach (PlayMakerFSM c in gameObject.GetComponents<PlayMakerFSM>())
                Destroy(c);
            Destroy(transform.GetChild(1).GetComponent("PlayMakerTriggerEnter"));
            transform.GetChild(1).gameObject.AddComponent<BoxCollider>().isTrigger = true;
            transform.GetChild(1).gameObject.GetComponent<BoxCollider>().size = new Vector3(0.1f,0.1f,0.1f);
            cdt = transform.GetChild(1).gameObject.AddComponent<CDTrigger>();
            cdt.CDcase = this;
            transform.position = new Vector3(-9.76f, 0.17f, 6.47f);
            if(cdp != null)
              cdp.Load();
            Destroy(transform.GetChild(1).GetComponent<PlayMakerFSM>()); //Why you keep doing this???
            transform.GetChild(1).gameObject.SetActive(true);
        }

        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //if (Physics.Raycast(ray, out RaycastHit hit, 1f))
            RaycastHit[] hits = Physics.RaycastAll(ray, 1f);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == gameObject.GetComponent<Collider>() && !cdt.entered)
                {
                    if (isOpen)
                    {
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Close Case";
                        if (cInput.GetButtonDown("Use"))
                        {
                            gameObject.transform.GetChild(0).GetComponent<Animation>().Play("cd_close");
                            isOpen = false;
                        }
                    }
                    else
                    {
                        PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
                        PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction").Value = "Open Case";
                        if (cInput.GetButtonDown("Use"))
                        {
                            gameObject.transform.GetChild(0).GetComponent<Animation>().Play("cd_open");
                            isOpen = true;
                            if (transform.GetChild(2).childCount > 0)
                            {

                                if (transform.GetChild(2).GetChild(0).gameObject.name == "cd(item1)")
                                {
                                    transform.GetChild(2).GetChild(0).SetParent(null);

                                }
                                transform.GetChild(2).GetChild(0).GetComponent<CD>().inCase = false;
                                transform.GetChild(2).GetChild(0).SetParent(null);
                            }
                        }

                    }
                }
            }
        }


    }
}
