using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour, IInteractable
{
    public string interactText = "Press E to Pick Up";

    //Settings and references required for the drop and pickup system
    public bool stackable;
    public Item itemScript;
    public RotateGun rotateGun;
    public Rigidbody rb;
    public BoxCollider coll;
    public Transform player, itemContainer, fpsCam;
    public InventoryManager inventoryManager;
    public PlayerMovement keyCodeHolder;

    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;

    public bool equipped;

    bool inGround;

    //Setup
    private void Start()
    {
        Debug.ClearDeveloperConsole();
        
        if (!equipped)
        {
            itemScript.enabled = false;
            itemScript.itemSway = false;
            if (rotateGun != null) rotateGun.enabled = false;
            rb.isKinematic = false;
            coll.isTrigger = false;
            SetGameLayerRecursive(transform.gameObject, 0);

            transform.GetComponent<Item>().icon.GetComponent<SlotUI>().ShowItem(false);
        }
        if (equipped)
        {
            itemScript.enabled = true;
            itemScript.itemSway = true;
            if (rotateGun != null) rotateGun.enabled = true;
            rb.isKinematic = true;
            coll.isTrigger = true;
            SetGameLayerRecursive(transform.gameObject, 6);
        }
    }

    private void Update()
    {
        //Drop if equipped and "Q" is pressed
        if (equipped && Input.GetKeyDown(keyCodeHolder.dropItemKey)) Drop();
        if (equipped) transform.localPosition = Vector3.zero;
    }

    public void Interact()
    {
        //Check if player is in range and "E" is pressed
        Vector3 distanceToPlayer = player.position - transform.position;
        if (!equipped && distanceToPlayer.magnitude <= pickUpRange)
        {
            //Checks if the object is stackable because the stackable and non-stackable objects
            //have different requirements to enter the inventory
            if (stackable)
            {
                //if the object is stackable we check if there is space in the inventory or if there is an object of the same type
                if (inventoryManager.isSpaceInInventory() || inventoryManager.isSameObjectInInventory(transform))
                    PickUp();
            }
            else
            {
                //if the object is not stackable we just check if there is space in the inventory
                if (inventoryManager.isSpaceInInventory())
                    PickUp();
            }
        }
    }

    private void PickUp()
    {
        equipped = true;

        //Make weapon a child of the camera and move it to default position
        transform.SetParent(itemContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);

        //Make Rigidbody kinematic and BoxCollider a trigger
        rb.isKinematic = true;
        coll.isTrigger = true;

        //Enable script
        itemScript.enabled = true;
        itemScript.itemSway = true;
        if (rotateGun != null) rotateGun.enabled = true;
        SetGameLayerRecursive(transform.gameObject, 6);

        //Update the inventor
        inventoryManager.UpdateInventory(transform, true, stackable);
    }

    private void Drop()
    {
        //Update the inventory
        inventoryManager.UpdateInventory(transform, false, stackable);
        //Hides the icon in the inventory
        transform.GetComponent<Item>().icon.GetComponent<SlotUI>().ShowItem(false);
        equipped = false;

        //Set parent to null
        transform.SetParent(null);

        //Make Rigidbody not kinematic and BoxCollider normal
        rb.isKinematic = false;
        coll.isTrigger = false;

        //Gun carries momentum of player
       // rb.velocity = player.GetComponent<Rigidbody>().velocity;

        //AddForce
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);

        //Disable script
        itemScript.enabled = false;
        itemScript.itemSway = false;
        if (rotateGun != null) rotateGun.enabled = false;
        SetGameLayerRecursive(transform.gameObject, 0);
    }

    //Helper function to set all child game objects of a gameobject to a certain layer
    private void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public string GetInteractText()
    {
        return interactText;
    }
}