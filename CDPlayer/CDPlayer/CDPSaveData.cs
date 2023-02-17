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
       // public bool inRack = true;
        public int rackID = -1;
        public byte RackSlot = 255;
     //   public bool purchased = false;       
    }
}
