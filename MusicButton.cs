using UnityEngine;
using UnityEngine.UI;

public class MusicButton : MonoBehaviour
{
    public AudioSource bgmSource;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private bool isMuted = false;
    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        UpdateIcon();
    }

    public void ToggleMusic()
    {
        isMuted = !isMuted;
        bgmSource.mute = isMuted;
        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (isMuted)
            buttonImage.sprite = musicOffSprite;
        else
            buttonImage.sprite = musicOnSprite;
    }
}
