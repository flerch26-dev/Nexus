using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySwitcher : MonoBehaviour, IInteractable
{
    public PlayerMovement player;
    public string interactText = "Press E to Switch Gravity";

    public bool CanInteract()
    {
        return true;
    }

    public string GetInteractText()
    {
        return interactText;
    }

    public void Interact()
    {
        player.SwitchGravity();
    }
}
