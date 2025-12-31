using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    //Reference to the player object, as well as the object keeping track of the camera orientation
    public Transform playerBody;
    public Transform orientation;

    //variable to define how sensible the camera is
    public int mouseSensitivity;

    //variables to keep track of the x and y rotations
    public float wantedXRotation;
    public float wantedYRotation;

    PlayerMovement playerScript;

    void Start()
    {
        //Lock the cursor to the center of the screen at the beggining of the game
        Cursor.lockState = CursorLockMode.Locked;
        playerScript = playerBody.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        //Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Change the x and y rotation based on the inputs
        wantedYRotation += mouseX;
        wantedXRotation -= mouseY;
        //clamp the x rotation so that the camera cannot look behind the player
        wantedXRotation = Mathf.Clamp(wantedXRotation, -90f, 90f);

        //Change the camera's orientation
        transform.localRotation = Quaternion.Euler(wantedXRotation, wantedYRotation, 0f);
        orientation.localRotation = Quaternion.Euler(0, wantedYRotation, 0);
    }
}
