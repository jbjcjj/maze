using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum Directions { TOP, RIGHT, BOTTOM, LEFT, NONE }

    [Header("Walls")]
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject bottomWall;
    [SerializeField] private GameObject leftWall;

    [Header("Corners (always active)")]
    [SerializeField] private GameObject topLeftCorner;
    [SerializeField] private GameObject topRightCorner;
    [SerializeField] private GameObject bottomLeftCorner;
    [SerializeField] private GameObject bottomRightCorner;

    private Dictionary<Directions, GameObject> walls = new();
    private Dictionary<Directions, bool> dirFlags = new();

    public Vector2Int Index { get; set; }

    private void Awake()
    {
        walls[Directions.TOP] = topWall;
        walls[Directions.RIGHT] = rightWall;
        walls[Directions.BOTTOM] = bottomWall;
        walls[Directions.LEFT] = leftWall;

        foreach (var dir in walls.Keys)
            dirFlags[dir] = true;

        EnsureCollider(topWall);
        EnsureCollider(rightWall);
        EnsureCollider(bottomWall);
        EnsureCollider(leftWall);

        SetupCorner(topLeftCorner);
        SetupCorner(topRightCorner);
        SetupCorner(bottomLeftCorner);
        SetupCorner(bottomRightCorner);
    }

    private void EnsureCollider(GameObject go)
    {
        if (go == null) return;
        if (!go.TryGetComponent(out Collider2D col))
        {
            col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
        }
    }

    private void SetupCorner(GameObject go)
    {
        if (go == null) return;
        go.SetActive(true);
        if (!go.TryGetComponent(out Collider2D col))
            col = go.AddComponent<BoxCollider2D>();

        col.isTrigger = false;
        col.offset = Vector2.zero;
        col.transform.localScale = Vector3.one;
    }

    public void SetDirFlag(Directions dir, bool flag)
    {
        if (dir == Directions.NONE) return;
        dirFlags[dir] = flag;
        if (walls.TryGetValue(dir, out GameObject wall) && wall != null)
            wall.SetActive(flag);
    }
}
