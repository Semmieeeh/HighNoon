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
    public BulletTrail trail;
    public Transform muzzle;
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
        reactionTime = 2 - difficulty;
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
        if (challengeValue > 0 && !challenged)
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
        if (playedSound == false)
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
    public float spreadRadiusx;
    public float spreadRadiusy;
    public float spreadRadiusz;
    private Ray lastRay;
    bool didHit;
    RaycastHit lastHit;
    void Attack()
    {
        if (canAttack)
        {
            delay -= Time.deltaTime;
            if (delay <= mindelay)
            {
                VRUtils.Instance.PlaySpatialClipAt(GunShotSound, transform.position, 0.75f);

                // Capture muzzle position and forward vector to prevent changes during the shot
                Vector3 muzzlePosition = muzzle.position;
                Vector3 muzzleForward = muzzle.forward;

                Debug.Log("Muzzle position: " + muzzlePosition); // For debugging

                // Calculate direction towards the player from the muzzle
                Vector3 directionToPlayer = (player.transform.position - muzzlePosition).normalized;

                // Add random spread to the shooting direction
                Vector3 spread = new Vector3(
                    Random.Range(-spreadRadiusx, spreadRadiusx),
                    Random.Range(-spreadRadiusy, spreadRadiusy),
                    Random.Range(-spreadRadiusz, spreadRadiusz)
                );

                // Apply spread to the shooting direction
                Vector3 shootDirection = directionToPlayer;// + spread;

                // Store the ray for visualization
                lastRay = new Ray(muzzlePosition, shootDirection);

                // Perform raycast and store result for Gizmos
                didHit = Physics.Raycast(lastRay, out lastHit);

                if (didHit)
                {
                    // Fire bullet towards hit point
                    trail.FireBullet(lastHit.point);
                    Debug.Log("Hit " + lastHit.collider.name);
                }
                else
                {
                    // Fire bullet towards a far point if it missed
                    trail.FireBullet(muzzlePosition + shootDirection * 1000f);
                    Debug.Log("Missed");
                }

                // Play muzzle flash effect
                shotRoutine = doMuzzleFlash();
                StartCoroutine(shotRoutine);

                // Reset the delay for the next shot
                delay = maxDelay;
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (lastRay.origin != Vector3.zero)
        {
            // Set Gizmo color for ray
            Gizmos.color = Color.red;

            if (didHit)
            {
                // Draw ray up to the point of impact
                Gizmos.DrawLine(lastRay.origin, lastHit.point);
                // Draw a small sphere at the point of impact
                Gizmos.DrawSphere(lastHit.point, 0.1f);
            }
            else
            {
                // Draw ray extending far if no hit
                Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * 100f);
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
        muzzleflash.transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f) * 10;
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
