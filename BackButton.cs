using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Dùng để chuyển scene

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class BackToMenuButton : MonoBehaviour
{
    [Header("Ảnh của nút")]
    public Sprite buttonSprite; // Ảnh dùng làm hình của nút

    private Button button;// Biến lưu component Button
    private Image image;// Biến lưu component Image

    private void Awake()
    {
        button = GetComponent<Button>();// Lấy component Button gắn trên GameObject
        image = GetComponent<Image>();// Lấy component Image gắn trên GameObject

        if (buttonSprite != null)// Nếu người dùng đã gắn sprite trong Inspector
        {
            image.sprite = buttonSprite;// Đổi hình của nút sang sprite đã chọn
            image.type = Image.Type.Sliced; 
        }

        button.onClick.AddListener(GoBackToMainMenu);// Khi nút được nhấn → gọi hàm GoBackToMainMenu
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");// Chuyển sang scene có tên "Main Menu"
    }
}

