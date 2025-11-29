using UnityEngine;                 
using UnityEngine.SceneManagement; // Dùng để chuyển scene

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Log ra tên object vừa chạm vào trigger (để debug)
        Debug.Log("Trigger collided with: " + collision.name);

        // Kiểm tra xem object chạm vào có Tag là "Player" hay không
        if (collision.CompareTag("Player"))
        {
            // Log để biết người chơi đã đến điểm thoát
            Debug.Log("Player reached exit!");
            SceneManager.LoadScene(2);// Chuyển sang scene có index = 2 trong Build Settings
        }
    }
}
