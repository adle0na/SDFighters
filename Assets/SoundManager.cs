using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public GameObject objMusic;

    private AudioSource audioSource;

    void Start()
    {
        objMusic = GameObject.FindWithTag("GameMusic");
        audioSource = objMusic.GetComponent<AudioSource>();
    }
}
