using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemy;
    public Transform enemySpawnPos;
    public Transform enemySpot;
    public AudioSource aud;
    public CowboyEnemy cowboyEnemy;
    public Transform playerspot;
    public Transform shake;
    
    void Start()
    {
        Vector3 enemyPos = new Vector3(5, -1, 0);
        Quaternion spawnRotation = Quaternion.Euler(0, -90, 0);

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
    public IEnumerator StartCountdownRoutine()
    {
        yield return new WaitForSeconds(10);
        if(counted == false)
        {
            aud.Play();
            cowboyEnemy.counted = true;
            counted = true;
        }


    }
    void Update()
    {
        
    }
}
