using UnityEngine;
using System.Collections;

public class StygianGun : GunCore
{
    [Header("Stygian Settings")]
    public float stygianAttractThreshold;
    public float duration = 0.3f;

    private void Update()
    {
        UpdateGun();

        //Shooting control
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;

            //Checks if our shot touches someting and returns true if it does
            RaycastHit hit;
            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range) && !isReloading)
            {
                Shoot();
                ShootStygianGun(hit);
            }
        }
    }

    void ShootStygianGun(RaycastHit hit)
    {
        //Create a trail renderer, which is the bullet visual
        TrailRenderer bulletTrail = Instantiate(BulletTrail, bulletSpawnPoint.position, Quaternion.identity);
        //Sets the trail to a parent so it doesn't clog up the hierarchy
        bulletTrail.transform.parent = bulletParent;
        //Show the bullet trail
        StartCoroutine(SpawnTrail(bulletTrail, hit));

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            float dst = Vector3.Distance(hit.point, enemy.transform.position);
            if (dst < stygianAttractThreshold)
            {
                Vector2 hitPoint = new Vector2(hit.point.x, hit.point.z);
                StartCoroutine(MoveEnemy(enemy, hitPoint));
            }
        }
    }

    IEnumerator MoveEnemy(GameObject enemy, Vector2 targetPosition)
    {
        float timeElapsed = 0;
        Vector2 startPosition = new Vector2(enemy.transform.position.x, enemy.transform.position.z);

        while (timeElapsed < duration)
        {
            Vector2 newPos = Vector2.Lerp(startPosition, targetPosition, timeElapsed / duration);
            enemy.transform.position = new Vector3(newPos.x, enemy.transform.position.y, newPos.y);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        enemy.transform.position = new Vector3(targetPosition.x, enemy.transform.position.y, targetPosition.y);
    }
}

