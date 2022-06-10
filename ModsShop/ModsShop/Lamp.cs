using UnityEngine;
using System.Collections;

namespace ModsShop
{
    public class Lamp : MonoBehaviour
    {
        public GameObject[] bulbs = new GameObject[2];
        public Light light;
        public bool outsideStuff = false;

        private float FullIntesity = 0.9f;
        private bool halfOn = false;
        private bool turnedOn = true;

        void Awake()
        {
            TurnOff();
        }

        public void TurnNormalOn()
        {
            if (turnedOn)
                return;
            for (int i = 0; i < bulbs.Length; i++)
            {
                bulbs[i].GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            }
            turnedOn = true;
        }
        IEnumerator TryToTurnOn(float delay, int tries, int halfAfter)
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i <= tries; i++)
            {
                if (halfAfter == i)
                    TurnHalfOn();
                if (halfOn)
                {
                    SetOnOffEmmision(true, bulbs[1].GetComponent<Renderer>());
                    light.intensity = FullIntesity;
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.08f));
                    SetOnOffEmmision(false, bulbs[1].GetComponent<Renderer>());
                    light.intensity = FullIntesity / 2f;
                }
                else
                {
                    if (i % 2 == 0)
                        SetOnOffEmmision(true, bulbs[0].GetComponent<Renderer>());
                    else
                        SetOnOffEmmision(true, bulbs[1].GetComponent<Renderer>());

                    light.intensity = FullIntesity / 2f;
                    light.gameObject.SetActive(true);
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                    if (i % 2 == 0)
                        SetOnOffEmmision(false, bulbs[0].GetComponent<Renderer>());
                    else
                        SetOnOffEmmision(false, bulbs[1].GetComponent<Renderer>());

                    light.gameObject.SetActive(false);
                }
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            }
            TurnOn();
        }
        public void StartTurningOn(int tries, int halfAfter, float delay)
        {
            if (turnedOn)
                return;
            StartCoroutine(TryToTurnOn(delay, tries, halfAfter));
        }
        void SetOnOffEmmision(bool on, Renderer rnd)
        {
            if (on)
                rnd.material.EnableKeyword("_EMISSION");
            else
                rnd.material.DisableKeyword("_EMISSION");
        }
        public void TurnOff()
        {
            StopAllCoroutines();
            turnedOn = false;
            for (int i = 0; i < bulbs.Length; i++)
            {
                SetOnOffEmmision(false, bulbs[i].GetComponent<Renderer>());
            }
            light.gameObject.SetActive(false);
        }
        void TurnHalfOn()
        {
            halfOn = true;
            SetOnOffEmmision(true, bulbs[0].GetComponent<Renderer>());
            light.gameObject.SetActive(true);
            light.intensity = FullIntesity / 2f;
        }
        internal void TurnOn()
        {
            halfOn = false;
            turnedOn = true;
            for (int i = 0; i < bulbs.Length; i++)
            {
                SetOnOffEmmision(true, bulbs[i].GetComponent<Renderer>());
            }
            light.gameObject.SetActive(true);
            light.intensity = FullIntesity;
        }
    }
}