using UnityEngine;

//Standard unity MonoBehaviour class
namespace CDPlayer
{
    public class CD : MonoBehaviour
    {
        public CDCase cdCase;
        public string CDName;
        public string CDPath = null;
        public bool isPlaylist = false;
        public bool inCase = false;
        public bool inPlayer = false;
        public Rigidbody rb;
        public BoxCollider coll;
        public BoxCollider trig;

#if !Mini
        void FixedUpdate()
        {
            if (!rb.detectCollisions && transform.parent != null)
            {
                if (transform.parent.name == "ItemPivot")
                {
                    rb.detectCollisions = true;
                    inPlayer = false;
                    inCase = false;
                }
            }
            if (!rb.detectCollisions && transform.parent == null)
            {
                rb.detectCollisions = true;
                inPlayer = false;
                inCase = false;
            }
            if (transform.parent == null && !rb.useGravity)
            {
                rb.useGravity = true;
            }
        }
        public void PutInCase()
        {
            rb.detectCollisions = false;
            rb.isKinematic = true;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            inCase = true;
        }
#endif
    }
}
