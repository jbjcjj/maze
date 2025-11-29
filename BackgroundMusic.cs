using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip bgm;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.clip = bgm;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        audioSource.Play();
    }
}
