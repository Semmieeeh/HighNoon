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
    public Transform t;
    public enum Behaviour
    {
        Formality,
        Standoff,
        Dueling
    }
    public Behaviour behaviour;

    public bool counted;
    public bool canShakeHands;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerHands")
        {
            challengeValue += 1 * Time.deltaTime;
        }
;
    }
    private Vector3 roamTarget;
    private float roamRadius = 2.0f;
    private float roamTime = 3.0f; // Time to roam before selecting a new point
    private float waitTime; // How long to wait at the roam target
    private bool isWaiting;
    private bool isLookingAround;
    private float lookAroundTimer;
    private Quaternion originalRotation;
    private void Start()
    {
        behaviour = Behaviour.Formality;
        agent = GetComponent<NavMeshAgent>();
        reactionTime = 2 - difficulty;
        agent.stoppingDistance = 0.5f;
        roamTarget = targetPos;  // Set initial roam target to the starting position
        waitTime = 7f; // No initial wait
        isWaiting = false;
        isLookingAround = false;
        originalRotation = transform.rotation;  // Store the original rotation for later
    }
    float deathtime = 10;
    bool dead;
    public void Die()
    {
        dead = true;
    }
    private void Update()
    {
        if(dead == true)
        {
            deathtime -= Time.deltaTime;
            if(deathtime <= 0)
            {
                Destroy(gameObject);
            }
        }
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
        hitBody.AddForce(bulletImpactForce * -muzzlePoint.forward, ForceMode.Impulse);
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

    // Updated HandleFormality method
    private void HandleFormality()
    {
        if (!challenged)
        {
            state = 0;
            RoamAndLookAround();  // Roam when not challenged and look around
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

    // Roaming logic within a 3x3 radius around the targetPos
    private void RoamAndLookAround()
    {
        if (isWaiting)
        {
            // Perform look around if waiting at a target
            LookAround();
            return;
        }

        // Check if we reached the destination
        if (Vector3.Distance(transform.position, roamTarget) <= agent.stoppingDistance + distanceTolerance)
        {
            if (!isWaiting)
            {
                // Start waiting at the destination
                waitTime = Random.Range(1.0f, 3.0f);  // Wait between 1 and 3 seconds
                isWaiting = true;
                isLookingAround = true;
                lookAroundTimer = waitTime / 2;  // Look around for half the waiting time
                originalRotation = transform.rotation;  // Store the current rotation
                StartCoroutine(WaitBeforeMoving());  // Start the waiting coroutine
            }
        }
        else
        {
            // Continue roaming
            walkState = 1;  // Walking animation
            agent.speed = walkspeed;
            agent.destination = roamTarget;
        }
    }

    private Vector3 GetRandomRoamingPoint()
    {
        Vector3 randomDirection = new Vector3(
            Random.Range(-roamRadius, roamRadius),
            0,
            Random.Range(-roamRadius, roamRadius)
        );
        return targetPos + randomDirection;
    }

    private IEnumerator WaitBeforeMoving()
    {
        yield return new WaitForSeconds(waitTime);
        roamTarget = GetRandomRoamingPoint();  // Get a new roam target
        isWaiting = false;
        isLookingAround = false;
    }


    private void LookAround()
    {
        // Look left and right during the idle time
        if (lookAroundTimer > 0)
        {
            float lookAngle = Mathf.Sin(Time.time * 2.0f) * 30f;  // Oscillate angle between -30 and +30 degrees
            Quaternion lookRotation = originalRotation * Quaternion.Euler(0, lookAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2.0f);
            lookAroundTimer -= Time.deltaTime;
        }
        else
        {
            // Reset to the original rotation after looking around
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 2.0f);
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
                Debug.Log("Muzzle position: " + muzzlePosition); // For debugging

                // Calculate direction towards the player from the muzzle
                Vector3 directionToPlayer = (player.transform.position - muzzlePosition).normalized;
                
                // Add random spread to the shooting direction
                Vector3 spread = new Vector3(
                    Random.Range(-spreadRadiusx, spreadRadiusx),
                    Random.Range(0, 0),
                    Random.Range(-spreadRadiusz, spreadRadiusz)
                );

                // Apply spread to the shooting direction
                Vector3 shootDirection = directionToPlayer + spread;

                // Store the ray for visualization
                lastRay = new Ray(muzzlePosition, shootDirection);

                // Perform raycast and store result for Gizmos
                didHit = Physics.Raycast(lastRay, out lastHit);

                if (didHit)
                {
                    // Fire bullet towards hit point
                    trail.FireBullet(lastHit.point);
                    Debug.Log("Hit " + lastHit.collider.name);
                    if(lastHit.collider.name == "Player")
                    {
                        lastHit.collider.gameObject.GetComponent<KillPlayer>().HandlePlayer();
                    }
                }
                else
                {
                    // Fire bullet towards a far point if it missed
                    trail.FireBullet(shootDirection * 1000f);
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
