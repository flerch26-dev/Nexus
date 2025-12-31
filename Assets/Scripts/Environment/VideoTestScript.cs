using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoTestScript : MonoBehaviour, IInteractable
{
    public string interactText = "Press E to Turn Screen On";

    public VideoPlayer videoPlayer;
    public Transform player;
    public Transform playerCamera;

    bool hasAlreadyPlayed = false;

    private void Start()
    {
        //videoPlayer = transform.GetComponent<VideoPlayer>();
        videoPlayer.targetTexture.Release();
        //StartCoroutine(WaitForSeconds(3));
    }

    void PlayVideo()
    {
        if (!videoPlayer.isPlaying && !hasAlreadyPlayed)
        {
            hasAlreadyPlayed = true;
            videoPlayer.Play();
        }
    }

    public void Interact()
    {
        PlayVideo();
    }

    public bool CanInteract()
    {
        interactText = "";
        return !hasAlreadyPlayed;
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
