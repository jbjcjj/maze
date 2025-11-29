using UnityEngine;   

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource audioSource;   // AudioSource dùng để phát nhạc nền
    public AudioClip bgm;             // File nhạc nền được gán trong Inspector

    void Start()
    {
        if (audioSource == null)                     // Nếu người dùng chưa kéo thả AudioSource
            audioSource = GetComponent<AudioSource>(); // Thì script tự lấy AudioSource trên GameObject

        audioSource.clip = bgm;       // Gán nhạc nền (AudioClip) vào AudioSource
        audioSource.loop = true;      // Bật chế độ lặp vô hạn
        audioSource.playOnAwake = false; // Không tự phát khi scene vừa load

        audioSource.Play();           // Phát nhạc nền khi Start() được gọi
    }
}
