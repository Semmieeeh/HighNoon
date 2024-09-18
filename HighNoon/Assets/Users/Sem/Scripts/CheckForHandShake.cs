using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForHandShake : MonoBehaviour
{
    public CowboyEnemy enemy;
    private void OnTriggerEnter(Collider other)
    {
        if (enemy.challenged == true && enemy.canShakeHands == true)
        {
            if (other.gameObject.tag == "PlayerHands")
            {
                enemy.shookHands = true;
            }
        }
    }
}
