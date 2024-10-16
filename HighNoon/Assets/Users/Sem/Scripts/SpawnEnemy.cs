using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public float standoffTime;
    public GameObject enemy;
    public Transform enemySpawnPos;
    public Transform barPos;
    public Transform enemySpot;
    public AudioSource aud;
    public CowboyEnemy cowboyEnemy;
    public Transform playerspot;
    public Transform shake;
    RevolverManager manager;
    public float difficulty;
    bool canShoot;



    public void Initiate()
    {
        Vector3 enemyPos = new Vector3(5, -1, 0);
        Quaternion spawnRotation = Quaternion.Euler(0, -90, 0);
        manager = GameObject.Find("PlayerRig").GetComponent<RevolverManager>();
        GameObject e = Instantiate(enemy, enemyPos, spawnRotation);
        CowboyEnemy f = e.GetComponent<CowboyEnemy>();
        cowboyEnemy = f;
        f.targetPos = barPos.GetChild(Random.Range(0,2)).gameObject.transform.position;
        f.spawn = this;
        f.difficulty = difficulty;
        f.playerSpot = playerspot;
        f.player = GameObject.Find("Player");
        f.shakeSpot = shake;
        f.enemySpot = enemySpot;

    }
    public void UnInitiate()
    {
        // Destroy the instantiated enemy game object
        if (cowboyEnemy != null)
        {
            Destroy(cowboyEnemy.gameObject); // Destroy the enemy object
            cowboyEnemy = null; // Reset the reference to null
        }
    }

    bool counted;
    public void StartCountdown()
    {
        StartCoroutine(nameof(StartCountdownRoutine));
        counted = false;
    }
    public IEnumerator StartCountdownRoutine()
    {
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
        if(cowboyEnemy == null)
        {
            return;
        }
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
        manager.revolver.GetComponent<Revolver>().canShoot = true;
        manager.revolver.GetComponent<Revolver>().shotBeforeBell = false;

    }
    
}
