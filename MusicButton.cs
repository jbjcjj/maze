using UnityEngine;          
using UnityEngine.UI;      
public class MusicButton : MonoBehaviour
{
    public AudioSource bgmSource;       // Nguồn âm thanh nền (AudioSource phát nhạc)
    public Sprite musicOnSprite;        // Icon khi nhạc đang bật
    public Sprite musicOffSprite;       // Icon khi nhạc đang tắt
    private bool isMuted = false;       // Trạng thái tắt/mở nhạc (false = đang bật)
    private Image buttonImage;          // Hình ảnh hiển thị trên nút bấm
    void Start()
    {
        buttonImage = GetComponent<Image>();  // Lấy component Image trên nút
        UpdateIcon();                         // Cập nhật icon khi khởi động game
    }
    public void ToggleMusic()
    {
        isMuted = !isMuted;             // Đảo trạng thái: bật ↔ tắt
        bgmSource.mute = isMuted;       // Tắt hoặc bật âm thanh trên AudioSource
        UpdateIcon();                   // Cập nhật icon theo trạng thái mới
    }

    void UpdateIcon()
    {
        if (isMuted)                               // Nếu nhạc đang tắt
            buttonImage.sprite = musicOffSprite;   // Đổi icon thành loa tắt
        else                                       // Nếu nhạc đang bật
            buttonImage.sprite = musicOnSprite;    // Đổi icon thành loa bật
    }
}
