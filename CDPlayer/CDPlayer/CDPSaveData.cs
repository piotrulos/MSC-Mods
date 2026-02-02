using System.Collections.Generic;
using UnityEngine;

namespace CDPlayer
{
    public class CDExternalFolders
    {
        public List<string> Folders = new List<string>();
    }
    public class ExternalFoldersResult
    {
        public string folderPath;
    }
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
        public int rackID = -1;
        public byte RackSlot = 255;
        public bool inPlayer = false;
        public sbyte inPlayerID = -1;

        public CDPSaveList() { }
        public CDPSaveList(byte type, string cdName, Vector3 pos, Vector3 rot)
        {
            goType = type;
            CDName = cdName;
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
            rotX = rot.x;
            rotY = rot.y;
            rotZ = rot.z;
        }
        public CDPSaveList(byte type, int rackid, Vector3 pos, Vector3 rot)
        {
            goType = type;
            rackID = rackid;
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
            rotX = rot.x;
            rotY = rot.y;
            rotZ = rot.z;
        }

        public CDPSaveList(byte type, string cdName, int rackid, byte rackslot)
        {
            goType = type;
            CDName = cdName;
            rackID = rackid;
            RackSlot = rackslot;
        }
    }
}
