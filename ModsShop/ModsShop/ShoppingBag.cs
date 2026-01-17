using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace ModsShop;
internal class ShoppingBag : MonoBehaviour
{
    public BoxCollider GetItemsTrigger = null;

#if !Mini
    private FsmBool GUIUse;
    private FsmString GUIInteraction;
    void Start()
    {
        GUIUse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
        GUIInteraction = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
    }
    void OnMouseExit()
    {
        GUIUse.Value = false;
        GUIInteraction.Value = string.Empty;
    }
    void Update()
    {
        if (UnifiedRaycast.GetHit(GetItemsTrigger))
        {
            GUIUse.Value = true;
            GUIInteraction.Value = $"Get item from bag ({transform.childCount} items)";
            if (cInput.GetButtonDown("Use"))
            {
                if (transform.childCount > 0)
                {
                    GameObject item = transform.GetChild(0).gameObject;
                    item.transform.localPosition = new Vector3(0, 0.35f, 0);
                    item.transform.parent = null;
                    item.SetActive(true);
                }
            }
        }
    }
#endif
}
