using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayRandomAudio : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClips;
    [SerializeField] private float maxDelaySeconds;

    void Start()
    {
        StartCoroutine(PlaySoundAfterDelay());
    }

    private IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(Random.Range(0f, maxDelaySeconds));
        int choice = Random.Range(0, audioClips.Count);

        GetComponent<AudioSource>().PlayOneShot(audioClips[choice]);
    }
}
