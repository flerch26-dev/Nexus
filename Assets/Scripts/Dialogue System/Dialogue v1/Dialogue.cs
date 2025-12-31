using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent; //Reference to the text mesh pro component (TMP for short)
    public string[] lines; //Text that will pop up
    public float textSpeed; //Speed that the individual characters will appear at

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        //Reset the text box to empty and start a new dialogue
        textComponent.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        //We check if we have clicked the right mouse button
        if (Input.GetMouseButtonDown(0))
        {
            //If we have fully shown the text of the current line we move on to the next line
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            //Otherwise we skip to the end of the text to show it completly without the animations
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    //Set the line index to zero to type the first lone
    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    //Coroutine to show the text character per character
    IEnumerator TypeLine()
    {
        //We achive this by looping over every character in the current text and adding the character to the TMP text
        //We then wait a bit (defined by the text speed variable)
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    //This is a function we call when we want to move to the next text
    void NextLine()
    {
        //We check if we are at the end of the lines array, meaning that there is no more text to say
        if (index < lines.Length - 1)
        {
            //If we arent, we move on to the next line by changing the index, resetting the string
            //and then we start typing the characters again
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            //If we are at the end of the array, we hide the text box
            gameObject.SetActive(false);
        }
    }
}
