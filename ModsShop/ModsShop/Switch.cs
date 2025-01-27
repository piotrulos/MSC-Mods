using UnityEngine;

namespace ModsShop;

//DEBUG SWITCHES
public class Switch : MonoBehaviour
{
    public Lamp[] lamps;
    public Animation anim;
    public bool outside = false;
    private bool isOff = true;

    void Start()
    {
        MakeInteractable(false);
    }

    public void MakeInteractable(bool yep)
    {
        GetComponent<BoxCollider>().enabled = yep;
    }
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
    public void TurnOn()
    {
        if (!isOff) return;
        anim.Play("Switch on");
        isOff = false;
        for (int i = 0; i < lamps.Length; i++)
        {
            if (!outside)
                lamps[i].StartTurningOn(Random.Range(5, 15), Random.Range(3, 10), Random.Range(1f, 3f));
            else
                lamps[i].TurnNormalOn();
        }
        MasterAudio.PlaySound3DAndForget("HouseFoley", transform, variationName: "light_switch");
    }
    public void ForceOn()
    {
        for (int i = 0; i < lamps.Length; i++)
        {
            if (!outside)
                lamps[i].TurnOn();
        }
        MasterAudio.PlaySound3DAndForget("HouseFoley", transform, variationName: "light_switch");
    }
    public void TurnOff()
    {
        if (isOff) return;
        anim.Play("Switch off");
        isOff = true;
        for (int i = 0; i < lamps.Length; i++)
        {
            lamps[i].TurnOff();
        }
        MasterAudio.PlaySound3DAndForget("HouseFoley", transform, variationName: "light_switch");
    }
    void OnMouseDown()
    {
        if (isOff)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }
}