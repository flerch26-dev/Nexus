using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DialogueManager : ObtainGraphData
{
    //Reference to the text component
    public DialogueContainer dialogueContainer;
    public TextMeshProUGUI dialogueText;

    //Speed that the individual characters will appear at
    public float textSpeed;

    //Array of all the buttons for the choices
    public Transform[] buttons;

    //Test variables for the dialogue tag system
    [HideInInspector] public string username = "seo";
    [HideInInspector] public int coins = 100;

    //List of all the tags
    public List<string> tags = new List<string>();

    //Variable representing a blank html tag to hide some of the characters
    private const string HTML_ALPHA = "<color=#00000000>";

    private void Start()
    {
        targetNodeGuid = dialogueContainer.nodeLinks[0].targetNodeGuid;
        ObtainNodeData(dialogueContainer);//, startNodeGuid);
        DisplayText();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            dialogueText.text = dialogue;
            ShowButtons();
        }
    }

    void DisplayText()
    {
        HideButtons();

        if (dialogue != "")
        {
            CheckForTag();
            /*if (dialogue.Contains("{username}"))
            {
                string[] tempDialogue = dialogue.Split("{username}");
                dialogue = tempDialogue[0] + username + tempDialogue[1];
            }*/

            StartCoroutine(TypeLine(dialogue));
        }
        else ShowButtons();

        for (int i = 0; i < choices.Count; i++)
        {
            //if (choices.ElementAt(i).Key == " ") buttons[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = "Next";
            //else
            buttons[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = choices.ElementAt(i).Key;
        }
    }

    void CheckForTag()
    {
        int numOpenBrackets = dialogue.Split("{").Length;
        int numClosedBrackets = dialogue.Split("}").Length;

        if (numOpenBrackets != 0 && numClosedBrackets != 0)
        {
            //We split the string into multiple parts while removing the brackets
            string[] dialogueParts = dialogue.Split(new char[] {'{', '}'}, numOpenBrackets + numClosedBrackets);
            dialogue = "";
            for (int i = 0; i < dialogueParts.Length; i++)
            {
                //if (i % 2 != 0)
                if (tags.Contains(dialogueParts[i])) dialogue = dialogue + GetType().GetField(dialogueParts[i]).GetValue(this);
                else dialogue = dialogue + dialogueParts[i];
            }
        }
    }

    public void OnClick(int buttonIndex)
    {
        string choice = buttons[buttonIndex].GetChild(0).GetComponent<TextMeshProUGUI>().text;
        targetNodeGuid = choices[choice];
        choices.Clear();
        ObtainNodeData(dialogueContainer);
        DisplayText();
    }

    //Coroutine to show the text character per character
    /*IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";

        //We achive this by looping over every character in the current text and adding the character to the TMP text
        //We then wait a bit (defined by the text speed variable)
        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        ShowButtons();
    }*/

    void HideButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
    }

    void ShowButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i >= choices.Count) buttons[i].gameObject.SetActive(false);
            else buttons[i].gameObject.SetActive(true);
        }
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";

        string originalText = line;
        string displayedText = "";
        int alphaIndex = 0;

        foreach (char c in line.ToCharArray())
        {
            alphaIndex++;
            dialogueText.text = originalText;

            displayedText = dialogueText.text.Insert(alphaIndex, HTML_ALPHA);
            dialogueText.text = displayedText;

            yield return new WaitForSeconds(textSpeed);
        }
    }
}
