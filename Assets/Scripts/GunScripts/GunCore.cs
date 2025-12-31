using UnityEngine;
using System.Collections;

public class GunCore : Item
{
    //General gun settings
    [Header ("General Settings")]
    public float damage = 10f;
    public float range = 100f;
    public float impactForce = 30f;
    public float fireRate = 15f;

    //Settings determining the firerate
    [Header ("Firerate settings")]
    public int maxAmmo = 10;
    private int currentAmmo;
    public float reloadTime = 1f;
    [HideInInspector] public bool isReloading = false;

    //References to differente scripts and objects
    [Header ("Transforms")]
    public Camera fpsCam;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public Transform effectParent;

    //Effect transforms
    [Space]
    public Transform bulletSpawnPoint;
    public TrailRenderer BulletTrail;
    public Transform bulletParent;
    public GameObject bulletHolePrefab;
    public PlayerMovement keyCodeHolder;

    //Animation settings (not a lot, hope to add later)
    [Header("Animations")]
    public Animator animator;

    [Header("Recoil")]
    public bool addRecoil;
    [Range(0, 10f)] public float maxRecoilTime;
    [Range(0, 3f)] public float recoilAmountX;
    [Range(0, 7f)] public float recoilAmountY;

    [HideInInspector] public float timePressed;
    [HideInInspector] public float currentRecoilXPos;
    [HideInInspector] public float currentRecoilYPos;

    //Aiming settings
    [Header("Aiming")]
    public MouseLook mouseLook;
    public int defaultMouseSensitivity;
    public int aimingMouseSensitivity;
    public Vector3 defaultPos;
    public Vector3 aimPos;
    public int defaultFOV;
    public int aimFOV;
    KeyCode aimKey;
    //private bool isAiming;

    //Bunch of variables
    [HideInInspector] public float nextTimeToFire = 0f;

    //Setup
    void Start()
    {
        currentAmmo = maxAmmo;

        aimKey = keyCodeHolder.aimKey;
    }

    void OnEnable()
    {
        //Make sure the gun is in the correct state when switching
        isReloading = false;
        animator.SetBool("Reloading", false);
    }

    //Runs every frame
    //Core of the script
    public void UpdateGun()
    {
        if (itemSway) ItemSway();

        //Aim Control
        if (Input.GetKeyDown(aimKey))
            Aim();
        if (Input.GetKeyUp(aimKey))
            UnAim();

        //Reloading check
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
    }

    //Coroutine for reload animation
    IEnumerator Reload()
    {
        isReloading = true;

        //Animate
        animator.SetBool("Reloading", true);
        yield return new WaitForSeconds(reloadTime - .25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(.25f);

        //Reset ammo
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    //Aim function
    void Aim()
    {
        //Sets all the settings to correct aim settings like fov, mouse sensitivity and position
        mouseLook.mouseSensitivity = aimingMouseSensitivity;
        transform.GetChild(0).localPosition = aimPos;
        fpsCam.fieldOfView = aimFOV;
    }

    //Reset aim function
    void UnAim()
    {
        //Resets all variables changed by the aim function to default
        mouseLook.mouseSensitivity = defaultMouseSensitivity;
        transform.GetChild(0).localPosition = defaultPos;
        fpsCam.fieldOfView = defaultFOV;
    }

    public void Shoot()
    {
        //Reloading check
        if (isReloading)
            return;

        if (addRecoil) RecoilMath();

        //Play muzzle flash particle effect
        muzzleFlash.Play();

        //Substrat one ammo, and adds recoil
        currentAmmo--;
    }

    //Coroutine for the bullet trail
    public IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
    {
        //Show the bullet trail over period of time
        float time = 0;
        Vector3 startPosition = trail.transform.position;
        //Takes a second
        while (time < 1)
        {
            //Change the bullet trail position based on the start and target position
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            //inscreases the time
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        //Creates the game object for the impact particle effect and sets the parent so as to not clog up the hierarchy
        GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        impactGO.transform.parent = effectParent;

        //make sure we don't spwan a bullet decal on other guns (looks wierd)
        if (hit.transform != null)
        {
            if (hit.transform.GetComponent<Item>() == null)
            {
                //create the bullet decal and set it's position and parent
                GameObject bulletHoleGO = Instantiate(bulletHolePrefab, hit.point, Quaternion.LookRotation(hit.normal));
                bulletHoleGO.transform.position += bulletHoleGO.transform.forward / 1000;
                bulletHoleGO.transform.parent = hit.transform;
                //Destroy the bullet decal after 5 seconds
                if (bulletHoleGO != null)
                    Destroy(bulletHoleGO, 5f);
            }
        }

        //Destroy the impact particle system and the bullet trail after they are done
        Destroy(impactGO, 1f);
        Destroy(trail.gameObject, trail.time);
    }

    public void RecoilMath()
    {
        currentRecoilXPos = ((Random.value - 0.5f) / 2) * recoilAmountX;
        currentRecoilYPos = ((Random.value - 0.5f) / 2) * (timePressed >= maxRecoilTime ? recoilAmountY / 4 : recoilAmountY);

        mouseLook.wantedXRotation -= Mathf.Abs(currentRecoilYPos);
        mouseLook.wantedYRotation -= currentRecoilXPos;
    }
}