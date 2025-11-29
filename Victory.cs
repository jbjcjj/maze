using UnityEngine;                
using UnityEngine.SceneManagement; // Dùng để chuyển scene

public class VictoryUI : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);  // Chuyển sang scene có index = 0 trong Build Settings
    }
}
