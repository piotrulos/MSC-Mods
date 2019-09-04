using UnityEngine;

//Standard unity MonoBehaviour class
namespace CDPlayer
{
    public class CD : MonoBehaviour
    {
        public string CDName;
        public string CDPath = null;
        public bool isPlaylist = false;
        public bool inCase = false;
        public CDCase cdCase;
        public bool inPlayer = false;

        void FixedUpdate()
        {
            if (transform.parent == null && !gameObject.GetComponent<Rigidbody>().detectCollisions)
            {
                gameObject.GetComponent<Rigidbody>().detectCollisions = true;
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                inPlayer = false;
                inCase = false;

            }
        }
        public void InCase()
        {
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            inCase = true;
        }
    }
}
