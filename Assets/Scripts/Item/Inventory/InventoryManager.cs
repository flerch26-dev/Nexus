using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    //variable that contains the index of the selected item
    public int selectedSlot = 0;

    public Transform placeHolderPrefab;
    public Transform slotHolder;
    public int inventorySize = 3;
    public Slot[] inventory;


    //Make sure we have the correct item at the beggining
    void Awake()
    {
        //Setup the inventory
        inventory = new Slot[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            //Sets up every slot to have a placeholder in it
            inventory[i] = slotHolder.GetChild(i).GetComponent<Slot>();
            inventory[i].SetItemType(placeHolderPrefab);
            inventory[i].AddAmount(1, placeHolderPrefab);
        }
        //Makes sure we have the correct slot selected
        SelectSlot(selectedSlot);
    }

    void Update()
    {
        //Variable that helps keep track of which item we had selected
        int previousSelectedSlot = selectedSlot;

        //Changes the selected item based on the keyboard input
        if (Input.GetKeyDown(KeyCode.Alpha1))
            selectedSlot = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            selectedSlot = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            selectedSlot = 2;

        //Only call the select item method if we actually changed item
        if (previousSelectedSlot != selectedSlot)
        {
            SelectSlot(selectedSlot);
        }
    }

    //Method to select and show the corret item
    public void SelectSlot(int slotIndex)
    {
        selectedSlot = slotIndex;

        //Loops over every slot in the inventory
        for (int i = 0; i < inventory.Length; i++)
        {
            //Loops over every item in every slot
            for (int j = 0; j < inventory[i].itemList.Count; j++)
            {
                //checks if the object is the first one in the slot and if it is the correct slot
                if (j == 0 && i == selectedSlot)
                {
                    //shows the object
                    inventory[i].itemList[j].gameObject.SetActive(true);

                    //Show the icon and position it in the inventory UI
                    if (inventory[i].itemList[j].GetComponent<Item>() != null)
                    {
                        inventory[i].itemList[j].GetComponent<Item>().icon.GetComponent<SlotUI>().ShowItem(true);
                        inventory[i].itemList[j].GetComponent<Item>().icon.GetComponent<SlotUI>().PositionItem(i);
                    }
                }
                else
                {
                    //hides the object
                    inventory[i].itemList[j].gameObject.SetActive(false);
                }
            }
        }
    }

    //Main method to update the inventory every time we pick up or drop an item
    public void UpdateInventory(Transform GOtransform, bool pickUp, bool stackable)
    {
        //Quick check to see if we picked up the item or if we dropped it and execute the corresponding script
        //Pickup
        if (pickUp)
        {
            //We check if an item of this type already exists in the inventory
            //Yes
            if (isSameObjectInInventory(GOtransform) && stackable)
            {
                //We find the index of the object of the same type and then we add the GOtransform to the slot's item list
                //Select the correct slot so the item we picked up is now shown
                int index = IndexOfSameObject(GOtransform);
                inventory[index].AddAmount(1, GOtransform);
                SelectSlot(index);
            }
            //No
            else
            {
                //We find the index of the lowest free slot
                //We then remove the placeholder object and set the item type of the slot to the current object
                //We add the item to the item list
                //Select the correct slot so the item we picked up is now shown
                int index = LowestFreeIndex();
                inventory[index].RemoveGO(placeHolderPrefab);
                inventory[index].AddAmount(1, GOtransform);
                inventory[index].SetItemType(GOtransform);
                SelectSlot(index);
            }
        }
        //Drop
        else
        {
            //We loop over every slot in the invenetory and see if the list of objects contains this object
            //If it does we found the index of the object we want to remove
            int index = 0;
            for (int i = 0; i < inventorySize; i++)
            {
                if (inventory[i].itemList.Contains(GOtransform))
                    index = i;
            }

            //We remove the object from the slot's item list
            inventory[index].RemoveGO(GOtransform);

            //If the list is empty we add the placeholder to it and set the item type
            if (inventory[index].itemList.Count == 0)
            {
                inventory[index].SetItemType(placeHolderPrefab);
                inventory[index].AddAmount(1, placeHolderPrefab);
            }

            //Select the correct slot so the item we picked up is now shown
            SelectSlot(index);
        }
    }

    //Method to check if a transform has a similar object in the inventory
    public bool isSameObjectInInventory(Transform GO)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].itemType.name == GO.name)
            {
                return true;
            }
        }
        return false;
    }

    //Method that returns the slot index of the similar object
    public int IndexOfSameObject(Transform GO)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].itemType.name == GO.name)
            {
                return i;
            }
        }
        return inventory.Length;
    }

    //Method that returns the lowest free index where we can add an iten
    //Loops over all items in the inventory until we enconter a placeholder object
    public int LowestFreeIndex()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].itemType.tag == "Placeholder")
            {
                return i;
            }
        }
        return inventory.Length;
    }

    //Loops over every slot and returns true if there is a free space
    public bool isSpaceInInventory()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].itemType == placeHolderPrefab)
                return true;
        }
        return false;
    }
}