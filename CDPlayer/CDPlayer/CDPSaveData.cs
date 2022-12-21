using System.Collections.Generic;

namespace CDPlayer
{
    public class CDPSaveData
    {
        public List<CDPSaveList> goSaveList = new List<CDPSaveList>();
    }
    public class CDPSaveList
    {
        public byte goType = 0; //0 - cd, 1 - case, 2 - rack
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ;
        public string CDName;
        public bool inCase = true;
        public int rackID = 0;
        public byte RackSlot = 0;
        public bool purchased;
    }
}
