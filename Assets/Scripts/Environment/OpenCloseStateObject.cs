using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseStateObject : MonoBehaviour, IInteractable
{
    public enum ObjectType { Door, Ladder };
    public ObjectType objectType;
    public string interactText;
    public Transform player;
    public InventoryManager inventoryManager;
    public Transform keycard;
    public float totalTime;
    public float maxActivationDst;

    public Vector3 closedPos;
    public Vector3 openedPos;

    enum State { open, closed };
    State state;

    bool inCoroutine;
    bool hasInteracted = false;
    [HideInInspector] public bool canOpen;

    void Start()
    {
        if (objectType == ObjectType.Door) { canOpen = false; interactText = "Press E to Open Door"; }
        else if (objectType == ObjectType.Ladder) { canOpen = true; interactText = "Press E to Get Ladder Down"; }
        transform.localPosition = closedPos;
        state = State.closed;
        inCoroutine = false;
    }

    IEnumerator Open(bool open)
    {
        float elapsedTime = 0;
        inCoroutine = true;

        if (open) state = State.open;
        else state = State.closed;

        while (elapsedTime < totalTime)
        {
            if (open) transform.localPosition = Vector3.Lerp(closedPos, openedPos, elapsedTime);
            else transform.localPosition = Vector3.Lerp(openedPos, closedPos, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        inCoroutine = false;
    }

    public void Interact()
    {
        bool hasKeycardInInventory = inventoryManager.isSameObjectInInventory(keycard);
        bool isPlayerInRange = Vector3.Distance(player.localPosition, transform.localPosition) < maxActivationDst;

        if (state == State.closed && inCoroutine == false && hasKeycardInInventory && isPlayerInRange && canOpen)
        {
            hasInteracted = true;
            StartCoroutine(Open(true));
            if (objectType == ObjectType.Ladder) interactText = "Press E to Climb Ladder";
        }
    }

    public bool CanInteract()
    {
        return !hasInteracted;
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
