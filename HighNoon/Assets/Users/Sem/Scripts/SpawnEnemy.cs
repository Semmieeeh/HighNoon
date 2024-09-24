using BNG;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public float standoffTime;
    public GameObject enemy;
    public Transform enemySpawnPos;
    public Transform enemySpot;
    public AudioSource aud;
    public CowboyEnemy cowboyEnemy;
    public Transform playerspot;
    public Transform shake;
    RevolverManager manager;
    bool canShoot;

    void Start()
    {
        Vector3 enemyPos = new Vector3(5, -1, 0);
        Quaternion spawnRotation = Quaternion.Euler(0, -90, 0);
        manager = GameObject.Find("PlayerRig").GetComponent<RevolverManager>();
        GameObject e = Instantiate(enemy, enemyPos, spawnRotation);
        CowboyEnemy f = e.GetComponent<CowboyEnemy>();
        cowboyEnemy = f;
        f.spawn = this;
        f.playerSpot = playerspot;
        f.player = GameObject.Find("Player");
        f.shakeSpot = shake;
        f.enemySpot = enemySpot;

    }

    // Update is called once per frame
    bool counted;
    public void StartCountdown()
    {
        StartCoroutine(nameof(StartCountdownRoutine));
    }
    bool inCombat;
    public IEnumerator StartCountdownRoutine()
    {
        inCombat = true;
        manager.revolver.GetComponent<Revolver>().canShoot = false;
        yield return new WaitForSeconds(Random.Range(standoffTime, standoffTime));
        manager.revolver.GetComponent<Revolver>().canShoot = true;
        if (counted == false)
        {
            aud.Play();
            cowboyEnemy.counted = true;
            counted = true;
        }
        

    }
    private void Update()
    {
        if(cowboyEnemy.ragdoll == true)
        {
            cowboyEnemy.Die();
            Vector3 enemyPos = new Vector3(5, -1, 0);
            Quaternion spawnRotation = Quaternion.Euler(0, -90, 0);
            manager = GameObject.Find("PlayerRig").GetComponent<RevolverManager>();
            GameObject e = Instantiate(enemy, enemyPos, spawnRotation);
            CowboyEnemy f = e.GetComponent<CowboyEnemy>();
            cowboyEnemy = f;
            f.spawn = this;
            f.playerSpot = playerspot;
            f.player = GameObject.Find("Player");
            f.shakeSpot = shake;
            f.enemySpot = enemySpot;
            StopAllCoroutines();
        }
        
    }

    public void Endround()
    {
        inCombat = false;
        manager.revolver.GetComponent<Revolver>().canShoot = true;
        manager.revolver.GetComponent<Revolver>().shotBeforeBell = false;

    }
    
}
