using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    //Reference to the slot parent
    Slot slot;
    //Variable to keep track of the amount
    int amount;
    //References to the text and placeholder prefab
    public TMPro.TextMeshProUGUI counter;
    public Transform placeHolderPrefab;

    void Start()
    {
        //Get a reference to the Slot component of the parent
        slot = transform.parent.GetComponent<Slot>();
        //Set the text to empty so it doesn't show
        counter.text = "";
    }

    void Update()
    {
        //We check if the slot is empty
        if (slot.itemType == placeHolderPrefab)
        {
            //if it is we just hide the text by setting the amount to zero and setting the text to blank
            amount = 0;
            counter.text = "";
        }
        else
        {
            //If the slot has objects, we set the amount to the amount of objects in the slot and set the text to that amount
            amount = slot.amount;
            counter.text = amount.ToString();
        }
    }
}
