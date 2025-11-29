using System.Collections.Generic;   
using UnityEngine;                  

public class Room : MonoBehaviour
{
    // Enum các hướng của phòng (trên, phải, dưới, trái, không)
    public enum Directions { TOP, RIGHT, BOTTOM, LEFT, NONE }

    [Header("Walls")]
    [SerializeField] private GameObject topWall;       // Tường trên của phòng
    [SerializeField] private GameObject rightWall;     // Tường phải của phòng
    [SerializeField] private GameObject bottomWall;    // Tường dưới của phòng
    [SerializeField] private GameObject leftWall;      // Tường trái của phòng

    [Header("Corners (always active)")]
    [SerializeField] private GameObject topLeftCorner;     // Góc trên trái (luôn hiển thị)
    [SerializeField] private GameObject topRightCorner;    // Góc trên phải
    [SerializeField] private GameObject bottomLeftCorner;  // Góc dưới trái
    [SerializeField] private GameObject bottomRightCorner; // Góc dưới phải

    private Dictionary<Directions, GameObject> walls = new(); // Lưu các tường theo hướng
    private Dictionary<Directions, bool> dirFlags = new();    // Lưu trạng thái tường (true = hiển thị)

    public Vector2Int Index { get; set; }   // Vị trí phòng trong lưới (x, y)

    private void Awake()
    {
        // Gán từng tường vào dictionary walls
        walls[Directions.TOP] = topWall;
        walls[Directions.RIGHT] = rightWall;
        walls[Directions.BOTTOM] = bottomWall;
        walls[Directions.LEFT] = leftWall;

        // Khởi tạo tất cả dirFlags = true (ban đầu tất cả tường hiện)
        foreach (var dir in walls.Keys)
            dirFlags[dir] = true;

        // Đảm bảo các tường có Collider2D
        EnsureCollider(topWall);
        EnsureCollider(rightWall);
        EnsureCollider(bottomWall);
        EnsureCollider(leftWall);

        // Thiết lập các góc luôn hiển thị và có collider
        SetupCorner(topLeftCorner);
        SetupCorner(topRightCorner);
        SetupCorner(bottomLeftCorner);
        SetupCorner(bottomRightCorner);
    }

    // Hàm đảm bảo đối tượng có Collider2D
    private void EnsureCollider(GameObject go)
    {
        if (go == null) return;
        if (!go.TryGetComponent(out Collider2D col))
        {
            col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = false;  // Collider không phải trigger
        }
    }

    // Thiết lập các góc phòng
    private void SetupCorner(GameObject go)
    {
        if (go == null) return;
        go.SetActive(true);  // Góc luôn hiển thị
        if (!go.TryGetComponent(out Collider2D col))
            col = go.AddComponent<BoxCollider2D>();

        col.isTrigger = false;        // Không phải trigger
        col.offset = Vector2.zero;    // Đặt offset về 0
        col.transform.localScale = Vector3.one; // Scale chuẩn
    }

    // Cập nhật trạng thái tường theo hướng
    public void SetDirFlag(Directions dir, bool flag)
    {
        if (dir == Directions.NONE) return;  // Không làm gì nếu NONE

        dirFlags[dir] = flag;  // Cập nhật trạng thái tường trong dict

        // Nếu có tường thực tế, bật/tắt theo flag
        if (walls.TryGetValue(dir, out GameObject wall) && wall != null)
            wall.SetActive(flag);
    }
}
