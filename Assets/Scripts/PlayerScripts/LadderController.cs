using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderController : MonoBehaviour
{
    public Transform chController;
    bool inside = false;
    public float speedUpDown;
    public float speedLeftRight;
    public PlayerMovement FPSInput;
    public Rigidbody rb;
    public Transform currentLadder;

    bool touchingLadder = false;

    void Start()
    {
        FPSInput = GetComponent<PlayerMovement>();
        inside = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Ladder")
        {
            touchingLadder = true;
            currentLadder = col.gameObject.transform;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Ladder")
        {
            touchingLadder = false;
            currentLadder = null;
            GetOffLadder();
        }
    }

    void Update()
    {
        if (inside)
        {
            FPSInput.CheckGrounded();
        }

        if (touchingLadder && Input.GetKeyDown(FPSInput.ladderKey))
        {
            GetOnLadder();
        }

        if (inside && FPSInput.grounded == true && Input.GetKey("s"))
        {
            GetOffLadder();
        }

        if (inside && Input.GetKey("w"))
            chController.transform.position += Vector3.up / speedUpDown;

        if (inside && Input.GetKey("s"))
            chController.transform.position += Vector3.down / speedUpDown;

        /*if (inside == true && Input.GetKey("d"))
            chController.transform.position += -currentLadder.right / speedLeftRight;

        if (inside == true && Input.GetKey("a"))
            chController.transform.position += currentLadder.right / speedLeftRight;*/
    }

    void GetOnLadder()
    {
        FPSInput.enabled = false;
        inside = true;
        rb.useGravity = false;
    }

    void GetOffLadder()
    {
        FPSInput.enabled = true;
        inside = false;
        rb.useGravity = true;
    }
}
