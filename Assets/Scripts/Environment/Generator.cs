using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour, IInteractable
{
    public string interactText = "Press E to Use Generator";
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public GameObject itemHolder;
    public OpenCloseStateObject door;

    public LightScript[] lights;
    public GameObject wireTask;
    [HideInInspector] public bool taskCompleted = false;

    bool hasInteracted = false;

    public bool CanInteract()
    {
        return !hasInteracted;
    }

    public void Interact()
    {
        hasInteracted = true;
        interactText = "";

        StartCoroutine(WaitUntilTaskComplete());
    }

    IEnumerator WaitUntilTaskComplete()
    {
        playerMovement.enabled = false;
        mouseLook.enabled = false;
        itemHolder.SetActive(false);

        wireTask.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        while (wireTask.GetComponent<WireTask>().isTaskCompleted == false)
        {
            yield return new WaitForSeconds(0.5f);
        }

        playerMovement.enabled = true;
        mouseLook.enabled = true;
        itemHolder.SetActive(true);

        wireTask.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        taskCompleted = true;
        door.canOpen = true;

        foreach (LightScript lightScript in lights)
        {
            StartCoroutine(lightScript.FlickerLights());
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
