using UnityEngine;
using System.Collections;

namespace ModsShop
{
    public class Lamp : MonoBehaviour
    {
        public GameObject[] bulbs = new GameObject[2];
        public Light light;
        public float FullIntesity = 0.75f;

        private MaterialPropertyBlock mpb;
        private Color matOff;
        private Color matOn;
        private bool halfOn = false;
        private bool turnedOn = false;

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            matOff = new Color(0, 0, 0);
            matOn = new Color(1, 1, 1);
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
            rnd.GetPropertyBlock(mpb);
            if (on)
                mpb.SetColor("_EmissionColor", matOn);
            else
                mpb.SetColor("_EmissionColor", matOff);
            rnd.SetPropertyBlock(mpb);
        }
        public void TurnOff()
        {
            turnedOn = false;
            SetOnOffEmmision(false, bulbs[0].GetComponent<Renderer>());
            SetOnOffEmmision(false, bulbs[1].GetComponent<Renderer>());
            light.gameObject.SetActive(false);
        }
        void TurnHalfOn()
        {
            halfOn = true;
            SetOnOffEmmision(true, bulbs[0].GetComponent<Renderer>());
            light.gameObject.SetActive(true);
            light.intensity = FullIntesity / 2f;
        }
        void TurnOn()
        {
            halfOn = false;
            turnedOn = true;
            SetOnOffEmmision(true, bulbs[0].GetComponent<Renderer>());
            SetOnOffEmmision(true, bulbs[1].GetComponent<Renderer>());
            light.gameObject.SetActive(true);
            light.intensity = FullIntesity;
        }
    }
}