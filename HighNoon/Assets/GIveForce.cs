using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GIveForce : MonoBehaviour
{
    // Start is called before the first frame update
    public float force;
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);
        Invoke("DestroyObj", 10f);
    }

    public void DestroyObj()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * 1, ForceMode.Force);
    }

}
