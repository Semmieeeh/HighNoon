using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BNG
{

    public class Revolver : GrabbableEvents
    {

        [Header("General : ")]
        public LineRenderer tracer;
        public BulletTrail trail;
        public bool inHolster;
        public bool canShoot;
        public bool shoot;
        public HingeJoint joint;
        public InsertBullet insert;
        public bool locked;
        private Quaternion originalRotation;
        public Transform ObjectToRotate; // The object that should rotate
        public Vector3 OriginalEulers;
        public float MaxRange = 25f;
        public float Damage = 25f;
        [Tooltip("Semi requires user to press trigger repeatedly, Auto to hold down")]
        public FiringType FiringMethod = FiringType.Semi;
        public ReloadType ReloadMethod = ReloadType.InfiniteAmmo;
        [Tooltip("Ex : 0.2 = 5 Shots per second")]
        public float FiringRate = 0.2f;
        float lastShotTime;

        [Tooltip("Amount of force to apply to a Rigidbody once damaged")]
        public float BulletImpactForce = 1000f;
        [Tooltip("Current Internal Ammo if you are keeping track of ammo yourself. Firing will deduct from this number. Reloading will cause this to equal MaxInternalAmmo.")]
        public float InternalAmmo = 0;
        [Tooltip("Maximum amount of internal ammo this weapon can hold. Does not account for attached clips.  For example, a shotgun has internal ammo")]
        public float MaxInternalAmmo = 10;

        /// <summary>
        /// Set true to automatically chamber a new round on fire. False to require charging. Example : Bolt-Action Rifle does not auto chamber.  
        /// </summary>
        [Tooltip("Set true to automatically chamber a new round on fire. False to require charging. Example : Bolt-Action Rifle does not auto chamber. ")]
        public bool AutoChamberRounds = true;

        /// <summary>
        /// Does it matter if rounds are chambered or not. Does the user have to charge weapon as soon as ammo is inserted
        /// </summary>
        [Tooltip("Does it matter if rounds are chambered or not. Does the user have to charge weapon as soon as ammo is inserted")]
        public bool MustChamberRounds = false;

        [Header("Projectile Settings : ")]

        [Tooltip("If true a projectile will always be used instead of a raycast")]
        public bool AlwaysFireProjectile = false;

        [Tooltip("If true the ProjectilePrefab will be instantiated during slowmo instead of using a raycast.")]
        public bool FireProjectileInSlowMo = true;

        [Tooltip("How fast to fire the weapon during slowmo. Keep in mind this is affected by Time.timeScale")]
        public float SlowMoRateOfFire = 0.3f;

        [Tooltip("Amount of force to apply to Projectile")]
        public float ShotForce = 10f;

        [Tooltip("Amount of force to apply to the BulletCasingPrefab object")]
        public float BulletCasingForce = 3f;

        [Header("Laser Guided Projectile : ")]

        [Tooltip("If true the projectile will be marked as Laser Guided and will follow a point from the ejection point")]
        public bool LaserGuided = false;

        [Tooltip("If specified the projectile will try to turn towards this object in world space. Otherwise will use a point from the muzzle of the raycast object")]
        public Transform LaserPoint;

        [Header("Recoil : ")]

        [Tooltip("How much force to apply to the tip of the barrel")]
        public Vector3 RecoilForce = Vector3.zero;


        [Tooltip("Time in seconds to allow the gun to be springy")]
        public float RecoilDuration = 0.3f;

        Rigidbody weaponRigid;

        [Header("Raycast Options : ")]
        public LayerMask ValidLayers;

        [Header("Weapon Setup : ")]

        [Tooltip("Transform of trigger to animate rotation of")]
        public Transform TriggerTransform;

        [Tooltip("Animate this back on fire")]
        public Transform SlideTransform;

        [Tooltip("Where our raycast or projectile will start from.")]
        public Transform MuzzlePointTransform;

        [Tooltip("Where to eject a bullet casing (optional)")]
        public Transform EjectPointTransform;

        [Tooltip("Transform of Chambered Bullet inside the weapon. Hide this when no bullet is chambered. (Optional)")]
        public Transform ChamberedBullet;

        [Tooltip("Make this active on fire. Randomize scale / rotation")]
        public GameObject MuzzleFlashObject;

        [Tooltip("Eject this at EjectPointTransform (optional)")]
        public GameObject BulletCasingPrefab;

        [Tooltip("If time is slowed this object will be instantiated at muzzle point instead of using a raycast")]
        public GameObject ProjectilePrefab;

        [Tooltip("Hit Effects spawned at point of impact")]
        public GameObject HitFXPrefab;

        [Tooltip("Play this sound on shoot")]
        public AudioClip GunShotSound;

        [Tooltip("Volume to play the GunShotSound clip at. Range 0-1")]
        [Range(0.0f, 1f)]
        public float GunShotVolume = 0.75f;

        [Tooltip("Play this sound if no ammo and user presses trigger")]
        public AudioClip EmptySound;

        [Tooltip("Volume to play the EmptySound clip at. Range 0-1")]
        [Range(0.0f, 1f)]
        public float EmptySoundVolume = 1f;

        [Header("Slide Configuration : ")]
        /// <summary>
        /// How far back to move the slide on fire
        /// </summary>
        [Tooltip("How far back to move the slide on fire")]
        public float SlideDistance = -0.028f;
        [Tooltip("Should the slide be forced back if we shoot the last bullet")]
        public bool ForceSlideBackOnLastShot = true;

        [Tooltip("How fast to move back the slide on fire. Default : 1")]
        public float slideSpeed = 1;
        [Header("Inputs : ")]
        [Tooltip("Controller Input used to eject clip")]
        public List<GrabbedControllerBinding> EjectInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };
        [Tooltip("Controller Input used to release the charging mechanism.")]
        public List<GrabbedControllerBinding> ReleaseSlideInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button1Down };
        [Tooltip("Controller Input used to release reload the weapon if ReloadMethod = InternalAmmo.")]
        public List<GrabbedControllerBinding> ReloadInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };
        [Header("Shown for Debug : ")]
        [Tooltip("Is there currently a bullet chambered and ready to be fired")]
        public bool BulletInChamber = false;
        [Tooltip("Is there currently a bullet chambered and that must be ejected")]
        public bool EmptyBulletInChamber = false;
        [Header("Events")]
        [Tooltip("Unity Event called when Shoot() method is successfully called")]
        private UnityEvent onShootEvent;
        [Tooltip("Unity Event called when something attaches ammo to the weapon")]
        private UnityEvent onAttachedAmmoEvent;
        [Tooltip("Unity Event called when something detaches ammo from the weapon")]
        private UnityEvent onDetachedAmmoEvent;
        [Tooltip("Unity Event called when the charging handle is successfully pulled back on the weapon")]
        private UnityEvent onWeaponChargedEvent;
        [Tooltip("Unity Event called when weapon damaged something")]
        private FloatEvent onDealtDamageEvent;
        [Tooltip("Passes along Raycast Hit info whenever a Raycast hit is successfully detected. Use this to display fx, add force, etc.")]
        private RaycastHitEvent onRaycastHitEvent;
        protected bool slideForcedBack = false;
        [Header("Chambers & Bullets")]
        public GameObject[] chambers; // Array of chamber positions
        public Bullet[] bullets; // Array of bullet objects corresponding to chambers
        public bool[] isBulletFired; // Array to track fired bullets
        protected WeaponSlide ws;
        public GameObject bulletPrefab;
        public int curChamber;
        public int minchamber = 1;
        public int maxchamber = 6;
        protected bool readyToShoot = true;
        public GameObject topcheck;
        public Animator hammerAnim;
        
        void Start()
        {
            curChamber = minchamber;
            weaponRigid = GetComponent<Rigidbody>();

            RefillRevolver();
            CheckChamber();
            OriginalEulers = ObjectToRotate.localEulerAngles;
            

            isBulletFired = new bool[chambers.Length];
            for (int i = 0; i < isBulletFired.Length; i++)
            {
                bullets[i] = chambers[i].transform.GetChild(0).GetComponent<Bullet>();
                isBulletFired[i] = false; // Initially, no bullets are fired
            }

            if (MuzzleFlashObject)
            {
                MuzzleFlashObject.SetActive(false);
            }

            ws = GetComponentInChildren<WeaponSlide>();

            updateChamberedBullet();
        }
        int i;
        void RefillRevolver()
        {
            foreach (GameObject chamber in chambers)
            {
                i = 0;
                if (chamber.transform.childCount == 0)
                {
                    GameObject Bullet = Instantiate(bulletPrefab, chamber.transform.position, chamber.transform.rotation);
                    Bullet.GetComponent<CapsuleCollider>().enabled = false;
                    Bullet.transform.parent = chamber.transform;
                    Vector3 newScale = new Vector3(0.1f, 0.1f, 0.13f);
                    Bullet.transform.localScale = newScale *12;
                    bullets[i] = Bullet.gameObject.GetComponent<Bullet>(); i++;
                    Bullet.GetComponent<Bullet>().canBeLoaded = false;
                    Bullet.GetComponent<Grabbable>().enabled = false;
                    

                }
            }
        }
        void CheckChamber()
        {
            GameObject closestChamber = null;  // Variable to store the closest chamber
            float closestDistance = Mathf.Infinity;  // Start with a very large number for comparison

            // Loop through each chamber
            foreach (GameObject chamber in chambers)
            {
                // Calculate the distance between the chamber and the "topcheck" object
                float distance = Vector3.Distance(chamber.transform.position, topcheck.transform.position);

                // Check if this chamber is closer than the previously stored closest chamber
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestChamber = chamber;
                }
            }

            // Now 'closestChamber' will contain the chamber closest to 'topcheck'
            if (closestChamber != null)
            {
                // Get the name of the closest chamber and convert it to an integer
                int closestChamberNumber = int.Parse(closestChamber.name);

                // Check if the closest chamber number is within the range of MinChamberCount and MaxChamberCount
                if (closestChamberNumber >= minchamber && closestChamberNumber <= maxchamber)
                {
                    // Assign the closest chamber number to CurChamber
                    curChamber = closestChamberNumber;
                    Debug.Log("The closest chamber is: " + closestChamber.name + " (Chamber number: " + curChamber + ")");
                }
                else
                {
                    Debug.Log("Closest chamber is outside the allowed chamber range.");
                }
            }
            else
            {
                Debug.Log("No chambers found!");
            }

        }
        

        public bool didShoot;
        public Transform Hammer;
        


    void checkSlideInput()
        {
            // Check for bound controller button to release the charging mechanism
            for (int x = 0; x < ReleaseSlideInput.Count; x++)
            {
                if (InputBridge.Instance.GetGrabbedControllerBinding(ReleaseSlideInput[x], thisGrabber.HandSide))
                {
                    UnlockSlide();
                    break;
                }
            }
        }

        void checkEjectInput()
        {
            bool inputDetected = false;

            // Check for bound controller button to eject magazine
            for (int x = 0; x < EjectInput.Count; x++)
            {
                if (InputBridge.Instance.GetGrabbedControllerBinding(EjectInput[x], thisGrabber.HandSide))
                {
                    print("holding button!");
                    inputDetected = true;
                    EjectMagazine(true); // Call EjectMagazine only when input is detected
                    break;
                }
            }

            // Do nothing if no input is detected
            if (inputDetected ==false)
            {
                EjectMagazine(false);
                return;
            }
        }


        public virtual void UnlockSlide()
        {
            if (ws != null)
            {
                ws.UnlockBack();
            }
        }


        public Rigidbody cylinderRb;
        public virtual void EjectMagazine(bool state)
        {
            // Create a new JointLimits struct and set the min value
            JointLimits limits = joint.limits;
            
            if (state == true &&locked == true)
            {
                joint.gameObject.GetComponent<Rigidbody>().mass = 10f;
                cylinderRb.mass = 100;
                limits.min = -90f;
                locked = false;
                foreach(Bullet b in bullets)
                {
                    if (b != null)
                    {
                        b.GetComponent<Grabbable>().enabled = true;
                    }
                }
                
            }
            else if( state == false && locked == false && EjectPointTransform.localEulerAngles.z < 20)
            {
                joint.gameObject.GetComponent<Rigidbody>().mass = 1f;
                cylinderRb.mass = 0.1f;
                locked = true;
                limits.min = 0f;
                foreach (Bullet b in bullets)
                {
                    if (b != null)
                    {
                        b.GetComponent<Grabbable>().enabled = false;
                    }
                }

            }
            
            
            // Assign the modified limits struct back to the joint
            joint.limits = limits;

            
        }

        protected bool playedEmptySound = false;
        public float ejectForce;
        public AudioClip[] holsterSound;
        private float rotation;
        bool playedOutHolster;
        bool playedInHolster;
        private void Update()
        {
            if (shoot == true)
            {
                Shoot();
                shoot = false;
            }

            if (inHolster == true && playedInHolster == false)
            {
                playedInHolster = true;
                playedOutHolster = false;
                VRUtils.Instance.PlaySpatialClipAt(holsterSound[1], transform.position, 0.75f);
            }
            else if (inHolster == false && playedOutHolster == false)
            {
                playedInHolster = false;
                playedOutHolster = true;
                VRUtils.Instance.PlaySpatialClipAt(holsterSound[0], transform.position, 0.75f);
            }


            if (EjectPointTransform.localEulerAngles.z < 90)
            {
                rotation = EjectPointTransform.localEulerAngles.z;

            }
            else if (EjectPointTransform.localEulerAngles.z > 90)
            {
                rotation = EjectPointTransform.localEulerAngles.z- 360;
            }
            //print(rotation);
            if (locked == false && rotation < -60)
            {
                
                print("Ejecting now!");

                for (int i = 0; i < bullets.Length; i++) 
                {
                    if (bullets[i] != null)
                    {
                        if (bullets[i].fired == true)
                        {
                            Vector3 localBackward = new Vector3(0, 0f, 0.1f);
                            Vector3 ejectDirection = bullets[i].transform.TransformDirection(localBackward);
                            Rigidbody rb = bullets[i].gameObject.GetComponent<Rigidbody>();
                            rb.isKinematic = false;
                            rb.AddForce(ejectDirection * ejectForce, ForceMode.Impulse);
                            rb.GetComponent<CapsuleCollider>().enabled = true;
                            bullets[i].transform.parent = null;
                            bullets[i] = null;
                        }
                    }
                    
                }
                foreach (Bullet b in bullets)
                {
                    if (b == null)
                    {
                        insert.canInsert = !locked;
                        break; // Exit early since we found an empty bullet slot
                    }
                    else
                    {
                        insert.canInsert = false;
                    }
                }
            }
        }

        public override void OnTrigger(float triggerValue)
        {
            
            // Sanitize for angles 
            triggerValue = Mathf.Clamp01(triggerValue);
            Debug.LogWarning(triggerValue);

            // Update trigger graphics
            if (TriggerTransform)
            {
                TriggerTransform.localEulerAngles = new Vector3(triggerValue * 15, 0, 0);
            }

            if (Hammer && didShoot == false)
            {
                Hammer.localEulerAngles = new Vector3(triggerValue * 30, 0, 0);
            }
            else
            {
                Hammer.localEulerAngles = Vector3.zero;
            }

            // Trigger up, reset values
            if (triggerValue <= 0.05f)
            {
                didShoot = false;
                readyToShoot = true;
                playedEmptySound = false;
            }
            
            // Rotate the object based on the triggerValue
            if (ObjectToRotate && didShoot == false && locked)
            {

                ObjectToRotate.localEulerAngles = new Vector3(OriginalEulers.x, OriginalEulers.y , OriginalEulers.z + triggerValue * 60);
            }
            if(didShoot == true && locked)
            {
                ObjectToRotate.localEulerAngles = new Vector3(OriginalEulers.x, OriginalEulers.y, OriginalEulers.z);
            }


            if (readyToShoot && triggerValue >= 0.9f)
            {
                Shoot();

                // Immediately ready to keep firing if in automatic mode
                readyToShoot = FiringMethod == FiringType.Automatic;
                OriginalEulers = new Vector3(OriginalEulers.x, OriginalEulers.y, OriginalEulers.z +60);
                didShoot = true;
                //print(OriginalEulers);
                
            }

            // These are here for convenience. Could be called through GrabbableUnityEvents instead
            checkSlideInput();
            checkEjectInput();
            CheckChamber();
            updateChamberedBullet();

            base.OnTrigger(triggerValue);
        }
        public bool shotBeforeBell;

        public AudioClip unjam;
        public void UnJam()
        {
            VRUtils.Instance.PlaySpatialClipAt(unjam, transform.position, GunShotVolume);
            shotBeforeBell = false;
            invoked = false;
        }
        bool invoked;
        public Transform TrailEnd;
        public virtual void Shoot()
        {
            ObjectToRotate.localEulerAngles = OriginalEulers;
            if (canShoot == false && inHolster == false)
            {
                if(invoked == false)
                {
                    CancelInvoke();
                    Invoke("UnJam", 10f);               
                    shotBeforeBell = true;
                    invoked = true;
                }
                VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, GunShotVolume);

                return;
            }
            if(shotBeforeBell == true)
            {
                VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, GunShotVolume);
                return;
            }
            CheckChamber();
            // Has enough time passed between shots
            Bullet b;
            if (bullets[curChamber - 1]!=null)
            {
                b = bullets[curChamber - 1].gameObject.GetComponent<Bullet>();
            }
            else
            {
                return;
            }
            float shotInterval = Time.timeScale < 1 ? SlowMoRateOfFire : FiringRate;
            if (Time.time - lastShotTime < shotInterval)
            {
                return;
            }

            //print(b.fired);
            if (b.fired == false && locked == true)
            {

                // Raycast to hit
                RaycastHit hit;
                b.bulletObj.SetActive(false);
                b.fired = true;
                isBulletFired[curChamber - 1] = b.fired;
                // RotateChamber
                if (Physics.Raycast(MuzzlePointTransform.position, -MuzzlePointTransform.forward, out hit, MaxRange, ValidLayers, QueryTriggerInteraction.Ignore))
                {
                    OnRaycastHit(hit,b);
                }
                else
                {
                    trail.FireBullet(TrailEnd.transform.position);
                }
                
                

            }
            else
            {
                return;
            }


            VRUtils.Instance.PlaySpatialClipAt(GunShotSound, transform.position, GunShotVolume);

            // Haptics
            if (thisGrabber != null)
            {
                input.VibrateController(0.1f, 0.2f, 0.1f, thisGrabber.HandSide);
            }

            // Apply recoil
            ApplyRecoil();

            

            // Call Shoot Event
            if (onShootEvent != null)
            {
                onShootEvent.Invoke();
            }

            // Store our last shot time to be used for rate of fire
            lastShotTime = Time.time;

            // Stop previous routine
            if (shotRoutine != null)
            {
                MuzzleFlashObject.SetActive(false);
                StopCoroutine(shotRoutine);
            }

            if (AutoChamberRounds)
            {
                shotRoutine = animateSlideAndEject();
                StartCoroutine(shotRoutine);
            }
            else
            {
                shotRoutine = doMuzzleFlash();
                StartCoroutine(shotRoutine);
            }
        }

        // Apply recoil by requesting sprinyness and apply a local force to the muzzle point
        public virtual void ApplyRecoil()
        {
            if (weaponRigid != null && RecoilForce != Vector3.zero)
            {

                // Make weapon springy for X seconds
                grab.RequestSpringTime(RecoilDuration);

                // Apply the Recoil Force
                weaponRigid.AddForceAtPosition(MuzzlePointTransform.TransformDirection(RecoilForce), MuzzlePointTransform.position, ForceMode.VelocityChange);
            }
        }

        // Hit something without Raycast. Apply damage, apply FX, etc.
        public virtual void OnRaycastHit(RaycastHit hit, Bullet previousBullet)
        {

            ApplyParticleFX(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit.collider);
            trail.FireBullet(hit.point);
            
            UiObject obj = hit.transform.gameObject.GetComponent<UiObject>();
            if(obj != null)
            {
                obj.DoFunction();
                previousBullet.fired = false;
            }

            MainUi obj2 = hit.transform.gameObject.GetComponent<MainUi>();
            if (obj2 != null)
            {               
                previousBullet.fired = false;
            }
            // Traverse up the hierarchy to find the topmost parent
            Transform current = hit.transform;
            Rigidbody hitRigid = hit.collider.attachedRigidbody;
            while (current.parent != null)
            {
                current = current.parent;
            }
            CowboyEnemy enemy = current.GetComponent<CowboyEnemy>();
            if (enemy != null)
            {
                
                enemy.Shot(BulletImpactForce, MuzzlePointTransform, hit, hitRigid);
            }


            // push object if rigidbody
            
            if (hitRigid != null)
            {
                hitRigid.AddForceAtPosition(BulletImpactForce * -MuzzlePointTransform.forward, hit.point);
            }

            // Damage if possible
            Damageable d = hit.collider.GetComponent<Damageable>();
            if (d)
            {
                d.DealDamage(Damage, hit.point, hit.normal, true, gameObject, hit.collider.gameObject);

                if (onDealtDamageEvent != null)
                {
                    onDealtDamageEvent.Invoke(Damage);
                }
            }

            // Call event
            if (onRaycastHitEvent != null)
            {
                onRaycastHitEvent.Invoke(hit);
            }
        }

        public virtual void ApplyParticleFX(Vector3 position, Quaternion rotation, Collider attachTo)
        {
            if (HitFXPrefab)
            {
                GameObject impact = Instantiate(HitFXPrefab, position, rotation) as GameObject;

                // Attach bullet hole to object if possible
                BulletHole hole = impact.GetComponent<BulletHole>();
                if (hole)
                {
                    hole.TryAttachTo(attachTo);
                }
            }
        }

        /// <summary>
        /// Something attached ammo to us
        /// </summary>
        public virtual void OnAttachedAmmo()
        {

            // May have ammo loaded
            updateChamberedBullet();

            if (onAttachedAmmoEvent != null)
            {
                onAttachedAmmoEvent.Invoke();
            }
        }

        // Ammo was detached from the weapon
        public virtual void OnDetachedAmmo()
        {
            // May have ammo loaded / unloaded
            updateChamberedBullet();

            if (onDetachedAmmoEvent != null)
            {
                onDetachedAmmoEvent.Invoke();
            }
        }

        public virtual int GetBulletCount()
        {
            if (ReloadMethod == ReloadType.InfiniteAmmo)
            {
                return 9999;
            }
            else if (ReloadMethod == ReloadType.InternalAmmo)
            {
                return (int)InternalAmmo;
            }
            else if (ReloadMethod == ReloadType.ManualClip)
            {
                return GetComponentsInChildren<Bullet>(false).Length;
            }

            // Default to bullet count
            return GetComponentsInChildren<Bullet>(false).Length;
        }

        public virtual void RemoveBullet()
        {

            // Don't remove bullet here
            if (ReloadMethod == ReloadType.InfiniteAmmo)
            {
                return;
            }

            else if (ReloadMethod == ReloadType.InternalAmmo)
            {
                InternalAmmo--;
            }
            else if (ReloadMethod == ReloadType.ManualClip)
            {
                Bullet firstB = GetComponentInChildren<Bullet>(false);
                // Deactivate gameobject as this bullet has been consumed
                if (firstB != null)
                {
                    Destroy(firstB.gameObject);
                }
            }

            // Whenever we remove a bullet is a good time to check the chamber
            updateChamberedBullet();
        }


        void updateChamberedBullet()
        {
            if (ChamberedBullet != null)
            {
                ChamberedBullet.gameObject.SetActive(BulletInChamber || EmptyBulletInChamber);
            }
        }
      

        protected IEnumerator shotRoutine;

        // Randomly scale / rotate to make them seem different
        void randomizeMuzzleFlashScaleRotation()
        {
            MuzzleFlashObject.transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f);
            MuzzleFlashObject.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 90f));
        }


        protected virtual IEnumerator doMuzzleFlash()
        {
            MuzzleFlashObject.SetActive(true);
            yield return new WaitForSeconds(0.05f);

            randomizeMuzzleFlashScaleRotation();
            yield return new WaitForSeconds(0.05f);

            MuzzleFlashObject.SetActive(false);
        }

        // Animate the slide back, eject casing, pull slide back
        protected virtual IEnumerator animateSlideAndEject()
        {

            // Start Muzzle Flash
            MuzzleFlashObject.SetActive(true);
            yield return new WaitForEndOfFrame();
            randomizeMuzzleFlashScaleRotation();
            yield return new WaitForEndOfFrame();
            MuzzleFlashObject.SetActive(false);           


            yield return new WaitForEndOfFrame();
            MuzzleFlashObject.SetActive(false);
            yield return new WaitForEndOfFrame();


            
        }
    }

    public enum FiringType
    {
        Semi,
        Automatic
    }

    public enum ReloadType
    {
        InfiniteAmmo,
        ManualClip,
        InternalAmmo
    }
}

