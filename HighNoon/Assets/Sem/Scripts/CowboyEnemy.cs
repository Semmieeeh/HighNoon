using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.HID;

public class CowboyEnemy : MonoBehaviour
{
    [Header("General")]
    public float difficulty;
    public GameObject weapon;
    public GameObject player;
    [Header("Animation")]
    public Animator animator;
    public AnimatorStateInfo stateInfo;
    public bool walking;
    public int walkState; //0 = stand still, 1 = walk, 2 = run
    public int state;
    [Header("Navigation")]
    [HideInInspector]
    public Transform enemySpot;
    public Transform shakeSpot;
    public Vector3 targetPos;
    public float walkspeed = 1.5f;
    public float sprintSpeed = 2.25f;
    public float walkingDistance = 10;
    public float distanceTolerance;
    private NavMeshAgent agent;
    [Header("State Progression")]
    public bool shookHands;
    public GameObject[] ragdollParts;
    public bool ragdoll;
    public bool challenged;
    public float challengeValue;
    public SpawnEnemy spawn;
    public Transform playerSpot;
    public float reactionTime;



    public enum Behaviour
    {
        Formality,
        Standoff,
        Dueling
    }

    public Behaviour behaviour;



    public void Shot(float BulletImpactForce, Transform MuzzlePointTransform, RaycastHit hit, Rigidbody hitBody)
    {
        print("Shot Enemy!");
        animator.enabled = false;
        foreach (GameObject obj in ragdollParts)
        {
            obj.GetComponent<Rigidbody>().isKinematic = false;
            obj.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        }
        hitBody.AddForce(BulletImpactForce * MuzzlePointTransform.forward, ForceMode.Impulse);


    }
    private void Start()
    {
        behaviour = Behaviour.Formality;
        agent = GetComponent<NavMeshAgent>();
        targetPos = transform.position;
        reactionTime = 2-difficulty;

    }

    private void SetAnimations()
    {
        animator.SetInteger("State", state);
        animator.SetBool("Walking", walking);
        animator.SetInteger("WalkState", walkState);
    }

    float extraRotationSpeed = 1000;

    void extraRotation()
    {
        Vector3 lookrotation = agent.steeringTarget - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookrotation), extraRotationSpeed * Time.deltaTime);

    }
    void SetAnimationSpeeds()
    {

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Quickdraw") && state ==2)
        {
            print("Drawing Gun!");
            animator.speed = difficulty;
            
        }
        else
        {
            animator.speed = 1;
        }
        
        
    }
    public bool counted;
    private void Update()
    {
        //extraRotation();

        if(challengeValue > 0)
        {
            challengeValue -= 0.5f * Time.deltaTime;
            if(challengeValue > 3)
            {
                challenged = true;
            }
        }
        

        SetAnimations();
        SetAnimationSpeeds();
        if (Vector3.Distance(transform.position, agent.destination) > agent.stoppingDistance + distanceTolerance)
        {
            walking = true;
        }
        else
        {
            walking = false;
        }
        if (Vector3.Distance(transform.position, agent.destination) <= walkingDistance)
        {
            walkState = 1;
            agent.speed = walkspeed;
        }
        else if (Vector3.Distance(transform.position, agent.destination) > walkingDistance)
        {
            walkState = 2;
            agent.speed = sprintSpeed;
        }
        switch (behaviour)
        {
            case Behaviour.Formality:


                if (!challenged)
                {
                    state = 0;
                    agent.destination = targetPos;
                    

                    
                }

                if (challenged && !shookHands)
                {
                    animator.SetBool("Challenged", true);
                    agent.destination = shakeSpot.transform.position;

                    if (Vector3.Distance(transform.position, shakeSpot.transform.position) < agent.stoppingDistance + 0.1f)
                    {
                        animator.SetBool("Inshakepos", true);

                        // Only rotate left or right to face the player object (Y-axis rotation only)
                        Vector3 direction = player.transform.position - transform.position;
                        direction.y = 0;  // Ignore the Y-axis to avoid tilting forward or backward

                        // Check if direction vector is non-zero to avoid errors with Quaternion.LookRotation
                        if (direction != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(direction);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
                        }
                    }

                    // Look at player when standing near the shakespot
                }

                if (challenged && shookHands)
                {
                    animator.SetBool("Shook", true);
                    agent.destination = enemySpot.position;
                    if(Vector3.Distance(transform.position, agent.destination)< distanceTolerance)
                    {
                        Vector3 direction = player.transform.position - transform.position;
                        direction.y = 0;  // Ignore the Y-axis to avoid tilting forward or backward

                        // Check if direction vector is non-zero to avoid errors with Quaternion.LookRotation
                        if (direction != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(direction);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
                        }
                    }
                    if (Vector3.Distance(transform.position, enemySpot.position) < agent.stoppingDistance + distanceTolerance && Vector3.Distance(player.transform.position, playerSpot.position) < 3f)
                    {
                        spawn.StartCountdown();
                        print("started countdown");
                        spawn.cowboyEnemy = this;
                        behaviour = Behaviour.Standoff;


                    }
                }

                break;
            case Behaviour.Standoff:
                state = 1;
                if(Vector3.Distance(transform.position, agent.destination)< distanceTolerance)
                {
                    if(counted == true)
                    {
                        reactionTime -= 1 * Time.deltaTime;
                        if(reactionTime <= 0)
                        {
                            behaviour = Behaviour.Dueling;
                        }
                        
                    }
                    Vector3 direction2 = player.transform.position - transform.position;
                    direction2.y = 0;  // Ignore the Y-axis to avoid tilting forward or backward

                    // Check if direction vector is non-zero to avoid errors with Quaternion.LookRotation
                    if (direction2 != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction2);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
                    }
                }
                break;
            case Behaviour.Dueling:
                state = 2;
                Vector3 direction3 = player.transform.position - transform.position;
                direction3.y = 0;  // Ignore the Y-axis to avoid tilting forward or backward

                // Check if direction vector is non-zero to avoid errors with Quaternion.LookRotation
                if (direction3 != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction3);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
                }
                break;
        }
    }
}
