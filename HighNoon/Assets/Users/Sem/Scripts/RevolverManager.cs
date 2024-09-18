using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverManager : MonoBehaviour
{
    public SnapZone snapZone;
    public GameObject revolver;

    private void Start()
    {
        revolver = snapZone.StartingItem.gameObject;
    }
}
