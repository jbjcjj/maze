using UnityEngine;  

// Yêu cầu GameObject phải có Rigidbody2D và Collider2D
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;  // Tốc độ di chuyển của người chơi

    private Rigidbody2D rb;          // Rigidbody2D để điều khiển vật lý 2D
    private SpriteRenderer spriteRenderer;  // Renderer để lật sprite khi di chuyển
    private Animator animator;       // Animator để điều khiển animation
    private Vector2 moveInput;       // Vector lưu hướng di chuyển người chơi

    private void Awake()
    {
        // Lấy các component khi object được tạo
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Lấy input theo trục Horizontal (A/D hoặc ←/→) và Vertical (W/S hoặc ↑/↓)
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveInput = moveInput.normalized; // Chuẩn hóa vector để di chuyển đều tốc độ chéo và ngang/dọc

        // Lật sprite khi di chuyển trái/phải
        if (moveInput.x < 0)
            spriteRenderer.flipX = true;   // Nếu di chuyển sang trái, lật sprite
        else if (moveInput.x > 0)
            spriteRenderer.flipX = false;  // Nếu di chuyển sang phải, để sprite bình thường

        // Cập nhật animation
        if (animator != null)
            animator.SetBool("isRun", moveInput != Vector2.zero); // bật animation chạy nếu có di chuyển
    }

    private void FixedUpdate()
    {
        // Di chuyển người chơi theo Rigidbody2D, giữ tính vật lý
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // In ra thông tin khi va chạm với một object khác
        Debug.Log($"Player collided with: {collision.gameObject.name} (layer {LayerMask.LayerToName(collision.gameObject.layer)})");
    }
}
