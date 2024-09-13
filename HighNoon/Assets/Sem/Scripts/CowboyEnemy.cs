using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public class CowboyEnemy : MonoBehaviour
{
    [Header("General")]
    public float difficulty;
    public GameObject weapon;

    [Header("Animation")]
    public Animator animator;
    public AnimatorStateInfo stateInfo;
    public bool walking;
    public int walkState; //0 = stand still, 1 = walk, 2 = run
    public int state;
    [Header("Navigation")]
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

    public enum Behaviour
    {
        Formality,
        Standoff,
        Dueling
    }

    Behaviour behaviour;

    AnimatorOverrideController overrideController;
    private void Start()
    {
        behaviour = Behaviour.Formality;
        agent = GetComponent<NavMeshAgent>();
        targetPos = transform.position;




        overrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
        animator.runtimeAnimatorController = overrideController;

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
    private void Update()
    {
        //extraRotation();
        if(ragdoll == true)
        {
            animator.enabled = false;
            foreach(GameObject obj in ragdollParts)
            {
                obj.GetComponent<Rigidbody>().isKinematic = false;
            }
            return;
        }
        else
        {
            foreach (GameObject obj in ragdollParts)
            {
                obj.GetComponent<Rigidbody>().isKinematic = true;
            }
            animator.enabled = true;
        }

        SetAnimations();
        SetAnimationSpeeds();
        agent.destination = targetPos;
        switch(behaviour)
        {
            case Behaviour.Formality:


                if(Vector3.Distance(transform.position, targetPos) > agent.stoppingDistance+distanceTolerance)
                {
                    walking = true;
                }
                else
                {
                    walking = false;
                }

                if(Vector3.Distance(transform.position, targetPos) <= walkingDistance)
                {
                    walkState = 1;
                    agent.speed = walkspeed;
                }
                else if(Vector3.Distance(transform.position,targetPos) > walkingDistance)
                {
                    walkState = 2;
                    agent.speed = sprintSpeed;
                }
                


                break;
            case Behaviour.Standoff:

                break;
            case Behaviour.Dueling:

                break;
        }
    }
}
