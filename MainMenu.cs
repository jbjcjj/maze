using UnityEngine;                     
using UnityEngine.SceneManagement;     // Dùng để load scene
using TMPro;                           // Dùng cho TMP_InputField của TextMeshPro

public class MainMenu : MonoBehaviour
{
    [Header("Input để chọn kích thước mê cung")]
    public TMP_InputField inputX;      // Ô nhập số lượng phòng theo trục X
    public TMP_InputField inputY;      // Ô nhập số lượng phòng theo trục Y

    public void PlayGame()
    {
        int x = 10;    // Giá trị mặc định nếu người dùng nhập sai
        int y = 10;    // Giá trị mặc định nếu người dùng nhập sai

        // Thử chuyển chuỗi trong inputX thành số nguyên.
        // Nếu chuyển không được → giữ giá trị mặc định = 10
        if (!int.TryParse(inputX.text, out x)) x = 10;

        // Tương tự với inputY
        if (!int.TryParse(inputY.text, out y)) y = 10;

        // Gán vào MazeSettings và giới hạn trong khoảng 5 → 100
        MazeSettings.numX = Mathf.Clamp(x, 5, 100);
        MazeSettings.numY = Mathf.Clamp(y, 5, 100);

        // Load scene có index = 1 trong Build Settings
        SceneManager.LoadSceneAsync(1);
    }
}
