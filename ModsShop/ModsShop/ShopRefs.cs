using System;
using UnityEngine;
namespace ModsShop
{
    public class ShopRefs : MonoBehaviour
    {
        public AutoShelves autoShelves;
        public CustomShelves customShelves;
        public CashRegister cashRegister;
        public Switch[] lightSwitches;
        public RaycastHit hit;
        public bool hitted;
        public Door door;
        public bool isShopClosed = false;
#if !Mini

        HutongGames.PlayMaker.FsmBool nightFSM;

        void Update()
        {
            if (Camera.main == null) return;
            if (isShopClosed) return;
            hitted = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f);
        }

        void OnEnable()        
        {
            nightFSM = GameObject.Find("MAP/SUN/Pivot/SUN").GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("Night");
            if (door.isOpen) door.CloseDoor();
            if(HutongGames.PlayMaker.FsmVariables.GlobalVariables.FindFsmInt("GlobalDay").Value == 7)
            {
                isShopClosed = true;
                cashRegister.anims.gameObject.SetActive(false);
                door.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                isShopClosed = false;
                cashRegister.anims.gameObject.SetActive(true);
                door.transform.GetChild(0).gameObject.SetActive(false);
                lightSwitches[0].ForceOn();
                if (nightFSM.Value) lightSwitches[1].ForceOn();
            }
          //  MSCLoader.ModConsole.Warning("Loaded");
        }
        void FixedUpdate()
        {
            if (nightFSM == null) return;
            if (isShopClosed)
            {
                lightSwitches[0].TurnOff();
                lightSwitches[1].TurnOff();
                lightSwitches[2].TurnOn();
                return;
            }
            lightSwitches[0].TurnOn();
            if (nightFSM.Value)
            {
                lightSwitches[1].TurnOn();
                lightSwitches[2].TurnOn();
            }
            else
            {
                lightSwitches[1].TurnOff();
                lightSwitches[2].TurnOff();
            }
        }

        internal void SetShadows()
        {
            for (int i = 0; i < lightSwitches[0].lamps.Length; i++)
            {
                if (ModsShop.instance.interiorShadows.GetValue())
                {
                    lightSwitches[0].lamps[i].light.shadows = LightShadows.None;
                    lightSwitches[1].lamps[i].light.shadows = LightShadows.None;
                }
                else
                {
                    lightSwitches[0].lamps[i].light.shadows = LightShadows.Hard;
                    lightSwitches[1].lamps[i].light.shadows = LightShadows.Hard;
                }
            }
        }
#endif
    }
}