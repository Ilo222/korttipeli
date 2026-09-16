using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSequencePlayer : MonoBehaviour
{
    [Serializable]
    public class SoundStep
    {
        public AudioClip clip;
        public float delayAfter = 1f;
    }

    [Header("Sound Clips")]
    public List<SoundStep> sequence = new List<SoundStep>();

    [Header("Playback")]
    public bool playOnStart = true;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(PlaySequence());
        }
    }

    public void PlaySequenceFromExternalCall()
    {
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        foreach (SoundStep step in sequence)
        {
            if (step.clip == null)
            {
                continue;
            }

            _audioSource.PlayOneShot(step.clip);
            // makes it so clips dont overlap
            float wait = Mathf.Max(step.delayAfter, step.clip.length);
            yield return new WaitForSeconds(wait);
        }
    }
}