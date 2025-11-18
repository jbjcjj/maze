using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger collided with: " + collision.name);
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player reached exit!");
            SceneManager.LoadScene(2);
        }
    }
}
