using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CowboyEnemy : MonoBehaviour
{
    [Header("General")]
    public float difficulty;
    public GameObject weapon;
    public GameObject muzzleflash;
    public GameObject player;
    public AudioClip GunShotSound;
    public AudioClip AngrySound;
    public AudioClip walkingSound;
    public AudioClip fleshHitSound;

    [Header("Animation")]
    public Animator animator;
    public int state;
    private AnimatorStateInfo stateInfo;
    private bool walking;
    private int walkState; // 0 = stand still, 1 = walk, 2 = run
    
    [Header("Navigation")]
    public Transform enemySpot;
    public Transform shakeSpot;
    public Transform playerSpot;
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
    public float reactionTime;

    public enum Behaviour
    {
        Formality,
        Standoff,
        Dueling
    }
    public Behaviour behaviour;

    private float extraRotationSpeed = 1000;
    public bool counted;
    public bool canShakeHands;

    private void Start()
    {
        behaviour = Behaviour.Formality;
        agent = GetComponent<NavMeshAgent>();
        targetPos = transform.position;
        reactionTime = 10 - difficulty;
    }

    private void Update()
    {
        if (ragdoll) return;

        HandleChallengeValue();
        SetAnimations();
        SetAnimationSpeeds();
        HandleMovement();

        switch (behaviour)
        {
            case Behaviour.Formality:
                HandleFormality();
                break;

            case Behaviour.Standoff:
                HandleStandoff();
                break;

            case Behaviour.Dueling:
                HandleDueling();
                break;
        }
    }

    public void Shot(float bulletImpactForce, Transform muzzlePoint, RaycastHit hit, Rigidbody hitBody)
    {
        if (behaviour != Behaviour.Dueling) return;
        VRUtils.Instance.PlaySpatialClipAt(fleshHitSound, transform.position, 0.75f);
        print("Shot Enemy!");
        animator.enabled = false;
        spawn.Endround();
        ActivateRagdoll();
        hitBody.AddForce(bulletImpactForce * muzzlePoint.forward, ForceMode.Impulse);
        ragdoll = true;
    }

    private void HandleChallengeValue()
    {
        if (challengeValue > 0 &&!challenged)
        {
            challengeValue -= 0.5f * Time.deltaTime;
            challenged = challengeValue > 3;
        }
    }

    private void SetAnimations()
    {
        animator.SetInteger("State", state);
        animator.SetBool("Walking", walking);
        animator.SetInteger("WalkState", walkState);
    }
    bool canAttack;
    private void SetAnimationSpeeds()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animator.speed = (stateInfo.IsName("Quickdraw") && state == 2) ? difficulty : 1;
        canAttack = stateInfo.IsName("Fulldraw_Idle");
    }

    private void HandleMovement()
    {
        walking = Vector3.Distance(transform.position, agent.destination) > agent.stoppingDistance + distanceTolerance;

        if (Vector3.Distance(transform.position, agent.destination) <= walkingDistance)
        {
            walkState = 1;
            agent.speed = walkspeed;
        }
        else
        {
            walkState = 2;
            agent.speed = sprintSpeed;
        }
    }

    private void HandleFormality()
    {
        if (!challenged)
        {
            state = 0;
            agent.destination = targetPos;
        }
        else if (!shookHands)
        {
            PrepareForHandshake();
        }
        else
        {
            ProceedToEnemySpot();
        }
    }

    bool playedSound;
    private void PrepareForHandshake()
    {
        animator.SetBool("Challenged", true);
        agent.destination = shakeSpot.position;
        if(playedSound == false)
        {
            VRUtils.Instance.PlaySpatialClipAt(AngrySound, transform.position, 0.75f);
            playedSound = true;
        }
        if (Vector3.Distance(transform.position, shakeSpot.position) < agent.stoppingDistance + 0.1f)
        {
            animator.SetBool("Inshakepos", true);
            RotateTowards(player.transform);
            canShakeHands = true;
        }
    }

    private void ProceedToEnemySpot()
    {
        animator.SetBool("Shook", true);
        agent.destination = enemySpot.position;

        if (Vector3.Distance(transform.position, enemySpot.position) < distanceTolerance)
        {
            RotateTowards(player.transform);
        }

        if (Vector3.Distance(transform.position, enemySpot.position) < agent.stoppingDistance + distanceTolerance &&
            Vector3.Distance(player.transform.position, playerSpot.position) < 1f)
        {
            spawn.StartCountdown();
            print("Started countdown");
            spawn.cowboyEnemy = this;
            behaviour = Behaviour.Standoff;
        }
    }

    private void HandleStandoff()
    {
        state = 1;
        
        if (Vector3.Distance(transform.position, agent.destination) < distanceTolerance)
        {
            if (counted)
            {
                behaviour = Behaviour.Dueling;
                
            }

            RotateTowards(player.transform);
        }
    }

    private void HandleDueling()
    {
        reactionTime -= Time.deltaTime;
        if (reactionTime <= 0)
        {
            state = 2;
            weapon.SetActive(true);
            RotateTowards(player.transform);
            Attack();
        }
    }

    float maxDelay = 2;
    float delay;
    float mindelay = 0;
    void Attack()
    {
        if (canAttack)
        {
            delay -= Time.deltaTime;
            if (delay <= mindelay)
            {
                VRUtils.Instance.PlaySpatialClipAt(GunShotSound, transform.position, 0.75f);
                shotRoutine = doMuzzleFlash();
                StartCoroutine(shotRoutine);
                delay = maxDelay;
            }
        }
    }

    protected virtual IEnumerator doMuzzleFlash()
    {
        muzzleflash.SetActive(true);
        yield return new WaitForSeconds(0.05f);

        randomizeMuzzleFlashScaleRotation();
        yield return new WaitForSeconds(0.05f);

        muzzleflash.SetActive(false);
    }
    void randomizeMuzzleFlashScaleRotation()
    {
        muzzleflash.transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f) *10;
        muzzleflash.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 90f));
    }
    IEnumerator shotRoutine;
    private void RotateTowards(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        }
    }

    private void ActivateRagdoll()
    {
        foreach (GameObject obj in ragdollParts)
        {
            var rb = obj.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
        }
    }
}
