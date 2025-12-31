using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    //Item type so the Inventory manger knows what object type this slot has
    public Transform itemType;
    //List of every transform of the same type the slot holds
    public List<Transform> itemList = new List<Transform>();
    //value that holds the amount of items in this slot
    public int amount;

    //Empty initializer to create new slots
    public Slot()
    {

    }

    //Method to set the type of item the slot holds
    public void SetItemType(Transform type)
    {
        itemType = type;
    }

    //Method to add a transform into the item list
    public void AddAmount(int value, Transform GO)
    {
        amount += value;
        itemList.Add(GO);
    }

    //Method to remove a transform from the item list
    public void RemoveGO(Transform GO)
    {
        amount -= 1;
        itemList.Remove(GO);
    }
}
