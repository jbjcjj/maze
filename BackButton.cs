using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class BackToMenuButton : MonoBehaviour
{
    [Header("Ảnh của nút")]
    public Sprite buttonSprite; 

    private Button button;
    private Image image;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        if (buttonSprite != null)
        {
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced; 
        }

        button.onClick.AddListener(GoBackToMainMenu);
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
