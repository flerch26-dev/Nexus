using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGun : MonoBehaviour
{
    //Reference to the grappling gun
    public GrapplingGun grappling;

    //Variables used later
    private Quaternion desiredRotation;
    private float rotationSpeed = 5f;

    //note : we are disbaling the weapon sway script because the two scripts have conflicting logics and the gun acts weird if we don't
    void Update()
    {
        //checks if we are grappling
        //if we are not
        if (!grappling.isGrappling())
        {
            //reset position and enab;e the weapon sway script
            transform.rotation = new Quaternion(0, 0, 0, 0);
            grappling.GetComponent<Item>().itemSway = true;
        }
        //if we are
        else
        {
            //define the wanted rotation of the gun so that the gun looks towards the grappling point
            desiredRotation = Quaternion.LookRotation(grappling.getGrapplingPoint() - transform.position);
            //Sets the rotation to a blend between the current position and the desired position
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);

            //Disable the weapong sway script
            grappling.GetComponent<Item>().itemSway = false;
        }
    }
}
