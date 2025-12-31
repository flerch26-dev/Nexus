using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that applies to any object that is able to take damage
public class Target : MonoBehaviour
{
    //Sets the health of the object
    public float health = 50f;

    //Function so the object takes damage
    public void TakeDamage (float amount)
    {
        //remove the desired amount from the object's health
        health -= amount;
        //checks if the object is dead (health is less or equal to 0)
        if (health <= 0f)
        {
            //Kill the object
            Die();
        }
    }

    //Kill script
    //Note : Add effects and stuff later on for game feel and player feedback
    public void Die()
    {
        //Destory the game object
        Destroy(gameObject);
    }
}
