using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotUI : MonoBehaviour
{
    //Defining the slot positions
    public Vector3 slot0Pos = new Vector3(9, 240, 0);
    public Vector3 slot1Pos = new Vector3(6, 0, 0);
    public Vector3 slot2Pos = new Vector3(0, -240, 0);

    //Method that is called from the InventoryManager script
    //Positions the icon in the right slot
    public void PositionItem(int slot)
    {
        if (slot == 0) transform.localPosition = slot0Pos;
        if (slot == 1) transform.localPosition = slot1Pos;
        if (slot == 2) transform.localPosition = slot2Pos;
    }

    //Method that is called from the InventoryManager script
    //Activates or deactivates the icon based on wether or not the player has the object in his inventory
    public void ShowItem(bool active)
    {
        transform.gameObject.SetActive(active);
    }
}