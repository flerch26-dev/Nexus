using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularGun : GunCore
{
    private void Update()
    {
        UpdateGun();

        //Shooting control
        if (Input.GetButton("Fire1"))
        {
            timePressed += Time.deltaTime;
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();

                //Checks if our shot touches someting and returns true if it does
                RaycastHit hit;
                if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range) && !isReloading)
                {
                    ShootRegularGun(hit);
                }
            }
        }
        else
        {
            timePressed = 0;
        }
    }

    //Core shooting function
    void ShootRegularGun(RaycastHit hit)
    {
        //Reloading check
        if (isReloading)
            return;

        //Checks if the object we hit is a target (meaning it can take damage and be destroyed)
        Target target = hit.transform.GetComponent<Target>();
        if (target != null)
            //Target substracts the amout of damage the gun does from it's total health
            target.TakeDamage(damage);

        //Checks if the object we hit has a rigidbody and if it does, apply force because of bullet impact
        if (hit.rigidbody != null)
            hit.rigidbody.AddForce(-hit.normal * impactForce);

        //Create a trail renderer, which is the bullet visual
        TrailRenderer bulletTrail = Instantiate(BulletTrail, bulletSpawnPoint.position, Quaternion.identity);

        //Sets the trail to a parent so it doesn't clog up the hierarchy
        bulletTrail.transform.parent = bulletParent;

        //Show the bullet trail
        StartCoroutine(SpawnTrail(bulletTrail, hit));
    }
}
