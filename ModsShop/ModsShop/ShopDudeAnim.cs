using UnityEngine;

namespace ModsShop
{
    public class ShopDudeAnim : MonoBehaviour
    {
        public Animation anim;

        void Start()
        {
            anim.Play("teimo_lean_table_in");
        }

        public void CashRegisterAnim()
        {
            anim.Play("teimo_cash_register");
        }

    }
}
