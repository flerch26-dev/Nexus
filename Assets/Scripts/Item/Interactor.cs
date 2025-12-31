using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

interface IInteractable
{
    public void Interact();
    public bool CanInteract();
    public string GetInteractText();
}

public class Interactor : MonoBehaviour
{
    public Transform player;
    public Transform cam;
    public Transform interactText;
    public float interactRange;

    public Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Ray r = new Ray(cam.position, cam.forward);
        interactText.gameObject.SetActive(false);

        if (Physics.Raycast(r, out RaycastHit hitInfo, interactRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
            {
                interactText.GetComponent<TMP_Text>().text = interactObj.GetInteractText();
                interactText.gameObject.SetActive(true);
                if (interactObj.CanInteract())
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactObj.Interact();
                    }
                }
            }
        }
    }
}
