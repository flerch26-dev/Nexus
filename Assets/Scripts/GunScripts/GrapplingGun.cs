using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGun : Item
{
    //Bunch of setup variables
    private LineRenderer lr;
    private Vector3 grapplePoint;

    [Header("Grapple Settings and Transforms")]
    public LayerMask whatIsGrapleable;
    public Transform gunTip, cam, player;
    private float maxDst = 100f;
    private SpringJoint joint;
    public Transform weaponHandler;

    void Awake()
    {
        //Refernce to the line renderer componenet
        lr = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        //Make sure the gun is in the right state when we enable it
        weaponHandler.GetComponent<Animator>().SetBool("Reloading", false);
    }

    //Runs every frame
    //Core of the script
    void Update()
    {
        //Start grapple if the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }

        //Stop grapple if the left mouse button is released
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }

    void LateUpdate()
    {
        //Draws the rope after we have updated the rest so the rope appears at the right spot and not at a delayed position
        DrawRope();
    }

    //Main function of the grapple script
    void StartGrapple()
    {
        //Checks if we are aiming at an object and not in the void
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxDst, whatIsGrapleable))
        {
            //Defines grapple point
            //Adds a spring joint component
            //Sets the anchor of the joint to the grapple point
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            //keeps the player at a certain distance from the grapple point, with some slight variation (gives the impression of a spring)
            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //settings for the spring joint (tweak later depending on game feel)
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
            lr.positionCount = 2;
        }
    }

    //draw rope function
    void DrawRope()
    {
        //checks if there is a spring joint
        if (!joint) return;
        //defines the two positions of the line renderer
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    //Stop grappling function
    //Hides the line renderer and gets rid of the spring joint
    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }

    //helper  functions to determine if we are grappling and where
    public bool isGrappling()
    {
        return joint != null;
    }

    public Vector3 getGrapplingPoint()
    {
        return grapplePoint;
    }
}