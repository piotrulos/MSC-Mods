using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace ModsShop;

public class Door : MonoBehaviour
{
    public Animation animation;
    public bool isOpen = false;
    public Collider coll;
#if !Mini
    private FsmBool GUIuse;

    void Start()
    {
        GUIuse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
    }

    void OnMouseExit()
    {
        GUIuse.Value = false;
    }

    public void CloseDoor()
    {
        animation.Play("door close");

        MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_close");
        isOpen = false;
    }
    public void OpenDoor()
    {
        animation.Play("door open");
        MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_open");
        isOpen = true;
    }

    void Update()
    {
        if (UnifiedRaycast.GetHit(coll))
        {
            GUIuse.Value = true;

            if (Input.GetMouseButtonDown(0))
            {
                if (ModsShop.GetShopReference().shopRefs.isShopClosed)
                {
                    MasterAudio.PlaySound3DAndForget("Store", transform, variationName: "door_locked");
                    return;
                }
                if (isOpen)
                {
                    CloseDoor();
                }
                else
                {
                    OpenDoor();
                }
            }
        }
    }
#endif
}