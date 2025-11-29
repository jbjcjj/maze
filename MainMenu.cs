using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class MainMenu : MonoBehaviour
{
    [Header("Input để chọn kích thước mê cung")]
    public TMP_InputField inputX; 
    public TMP_InputField inputY; 

    public void PlayGame()
    {
        int x = 10;
        int y = 10;

        if (!int.TryParse(inputX.text, out x)) x = 10;
        if (!int.TryParse(inputY.text, out y)) y = 10;

        MazeSettings.numX = Mathf.Clamp(x, 5, 100);
        MazeSettings.numY = Mathf.Clamp(y, 5, 100);

        SceneManager.LoadSceneAsync(1);
    }
}
