using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtainGraphData : MonoBehaviour
{
    //Steps to follow :
    //In Node Links : Obtain guid from start node and also the option "next" (default option)
    //In Node Links : We then want to get the target node guid associated with the start guid
    //In Dialogue Node Data : We can then obtain the dialogue in the node with the guid we got from the previous step
    //In Node Links : We can then grab all choices associated with the guid
    //We get the input from the player to decide which option he wants, and based on that we can get the new base node guid
    //We can then repeat this process and using the player input to set the new base guid every time

    //string baseNodeGuid;
    [HideInInspector] public string targetNodeGuid;

    [HideInInspector] public string dialogue;
    [HideInInspector] public Dictionary<string, string> choices = new Dictionary<string, string>();

    public void ObtainNodeData(DialogueContainer dialogueContainer)//, string baseNodeGuid)
    {
        //baseNodeGuid = dialogueContainer.nodeLinks[1].baseNodeGuid;

        //Debug.Log(baseNodeGuid);
        //Debug.Log(targetNodeGuid);

        for (int i = 0; i < dialogueContainer.dialogueNodeData.Count; i++)
        {
            if (dialogueContainer.dialogueNodeData[i].Guid == targetNodeGuid)
            {
                dialogue = dialogueContainer.dialogueNodeData[i].dialogueText;
                //Debug.Log(dialogue);
                break;
            }
        }

        for (int i = 0; i < dialogueContainer.nodeLinks.Count; i++)
        {
            if (dialogueContainer.nodeLinks[i].baseNodeGuid == targetNodeGuid)
            {
                choices.Add(dialogueContainer.nodeLinks[i].portName, dialogueContainer.nodeLinks[i].targetNodeGuid);
                //Debug.Log(dialogueContainer.nodeLinks[i].portName);
            }
        }
    }
}
