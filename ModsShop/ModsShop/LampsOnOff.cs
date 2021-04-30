using UnityEngine;
using System.Collections.Generic;
namespace ModsShop
{
    public class LampsOnOff : MonoBehaviour
    {

        public List<Lamp> lamps;
        void Awake()
        {
            lamps = new List<Lamp>();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
                foreach (Lamp l in lamps)
                    l.StartTurningOn(Random.Range(5, 15), Random.Range(3, 10), Random.Range(1f, 3f));
            if (Input.GetKeyDown(KeyCode.Alpha6))
                foreach (Lamp l in lamps)
                    l.TurnOff();
        }
    }
}