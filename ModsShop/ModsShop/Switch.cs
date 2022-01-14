using MSCLoader;
using UnityEngine;

namespace ModsShop
{
    public class Switch : MonoBehaviour
    {
        public Lamp[] lamps;
        public Animation anim;
        private bool isOff = true;

        void OnMouseOver()
        {
#if !Mini
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = true;
#endif
        }
        void OnMouseExit()
        {
#if !Mini
            PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse").Value = false;
#endif

        }

        void OnMouseDown()
        {
            if (isOff)
            {
                anim.Play("Switch on");
                isOff = false;
                for (int i = 0; i < lamps.Length; i++)
                {
                    lamps[i].StartTurningOn(Random.Range(5, 15), Random.Range(3, 10), Random.Range(1f, 3f));
                }
            }
            else
            {
                anim.Play("Switch off");
                isOff = true;
                for (int i = 0; i < lamps.Length; i++)
                {
                    lamps[i].TurnOff();
                }
            }
        }
    }
}