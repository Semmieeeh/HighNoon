using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCharController : MonoBehaviour
{
    // Start is called before the first frame update
    CapsuleCollider col;
    CharacterController cc;
    void Start()
    {
        cc = GetComponent<CharacterController>();
        col = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        col.height = cc.height;
        col.radius = cc.radius;
    }
}
