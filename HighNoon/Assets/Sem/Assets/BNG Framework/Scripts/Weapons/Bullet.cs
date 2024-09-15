using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    public class Bullet : MonoBehaviour {
        public bool fired;
        public bool canBeLoaded;
        public bool ejected;
        public GameObject bulletObj;

        private void OnTriggerExit(Collider other)
        {
            if(other.tag == "Cylinder")
            {
                //GetComponent<Collider>().isTrigger = false;
            }
        }

    }


    
}
