using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private int numX = 10;
    [SerializeField] private int numY = 10;

    private void Awake()
    {
        numX = MazeSettings.numX;
        numY = MazeSettings.numY;
    }

    private Room[,] rooms;
    private float roomWidth;
    private float roomHeight;

    private class Edge
    {
        public Vector2Int A;
        public Vector2Int B;
        public float weight;
        public Room.Directions dirFromA;

        public Edge(Vector2Int a, Vector2Int b, Room.Directions dir, float w)
        {
            A = a;
            B = b;
            dirFromA = dir;
            weight = w;
        }
    }

    private class DisjointSet
    {
        private int[] parent;
        private int[] rank;

        public DisjointSet(int size)
        {
            parent = new int[size];
            rank = new int[size];
            for (int i = 0; i < size; i++) parent[i] = i;
        }

        private int Find(int x)
        {
            if (parent[x] != x) parent[x] = Find(parent[x]);
            return parent[x];
        }

        public bool Union(int a, int b)
        {
            int rootA = Find(a);
            int rootB = Find(b);
            if (rootA == rootB) return false;

            if (rank[rootA] < rank[rootB]) parent[rootA] = rootB;
            else if (rank[rootA] > rank[rootB]) parent[rootB] = rootA;
            else { parent[rootB] = rootA; rank[rootA]++; }

            return true;
        }

        public bool Connected(int a, int b) => Find(a) == Find(b);
    }

    private void Start()
    {
        InitializeRooms();
        AddExitTrigger(); 
        SetCamera();
    }

    private void InitializeRooms()
    {
        rooms = new Room[numX, numY];
        GetRoomSize();

        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numY; j++)
            {
                GameObject room = Instantiate(
                    roomPrefab,
                    new Vector3(i * roomWidth, j * roomHeight, 0),
                    Quaternion.identity
                );
                room.name = $"Room_{i}_{j}";
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);
            }
        }
    }

    private void GetRoomSize()
    {
        SpriteRenderer[] spriteRenderers = roomPrefab.GetComponentsInChildren<SpriteRenderer>();
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (SpriteRenderer ren in spriteRenderers)
        {
            minBounds = Vector3.Min(minBounds, ren.bounds.min);
            maxBounds = Vector3.Max(maxBounds, ren.bounds.max);
        }

        roomWidth = maxBounds.x - minBounds.x;
        roomHeight = maxBounds.y - minBounds.y;
    }

    private void SetCamera()
    {
        Camera.main.transform.position = new Vector3(
            numX * (roomWidth - 1) / 2,
            numY * (roomHeight - 1) / 2,
            -100.0f);
        float min_value = Mathf.Min(numX * (roomWidth - 1), numY * (roomHeight - 1));
        Camera.main.orthographicSize = min_value * 0.75f;
    }

    private int GetIndex(int x, int y) => y * numX + x;

    private void ResetRooms()
    {
        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numY; j++)
            {
                rooms[i, j].SetDirFlag(Room.Directions.TOP, true);
                rooms[i, j].SetDirFlag(Room.Directions.RIGHT, true);
                rooms[i, j].SetDirFlag(Room.Directions.BOTTOM, true);
                rooms[i, j].SetDirFlag(Room.Directions.LEFT, true);
            }
        }
    }

    public void CreateMaze()
    {
        ResetRooms();
        GenerateMazeWithKruskal();
    }

    private void GenerateMazeWithKruskal()
    {
        List<Edge> edges = new List<Edge>();

        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                Vector2Int current = new Vector2Int(x, y);
                if (x < numX - 1)
                    edges.Add(new Edge(current, new Vector2Int(x + 1, y), Room.Directions.RIGHT, UnityEngine.Random.value));
                if (y < numY - 1)
                    edges.Add(new Edge(current, new Vector2Int(x, y + 1), Room.Directions.TOP, UnityEngine.Random.value));
            }
        }

        edges.Sort((a, b) => a.weight.CompareTo(b.weight));

        DisjointSet ds = new DisjointSet(numX * numY);

        foreach (var e in edges)
        {
            int indexA = GetIndex(e.A.x, e.A.y);
            int indexB = GetIndex(e.B.x, e.B.y);

            if (!ds.Connected(indexA, indexB))
            {
                ds.Union(indexA, indexB);
                RemoveWall(e.A, e.dirFromA);
            }
        }

        RemoveWall(new Vector2Int(0, 0), Room.Directions.BOTTOM);
        RemoveWall(new Vector2Int(numX - 1, numY - 1), Room.Directions.RIGHT);
    }

    private void RemoveWall(Vector2Int pos, Room.Directions dir)
    {
        rooms[pos.x, pos.y].SetDirFlag(dir, false);
        Vector2Int other = pos;

        Room.Directions opposite = Room.Directions.NONE;
        switch (dir)
        {
            case Room.Directions.TOP:
                other += Vector2Int.up;
                opposite = Room.Directions.BOTTOM;
                break;
            case Room.Directions.RIGHT:
                other += Vector2Int.right;
                opposite = Room.Directions.LEFT;
                break;
            case Room.Directions.BOTTOM:
                other += Vector2Int.down;
                opposite = Room.Directions.TOP;
                break;
            case Room.Directions.LEFT:
                other += Vector2Int.left;
                opposite = Room.Directions.RIGHT;
                break;
        }

        if (other.x >= 0 && other.x < numX && other.y >= 0 && other.y < numY)
        {
            rooms[other.x, other.y].SetDirFlag(opposite, false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateMaze();
        }
    }

    private void AddExitTrigger()
    {
        GameObject exit = new GameObject("ExitTrigger");
        BoxCollider2D col = exit.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        exit.transform.position = rooms[numX - 1, numY - 1].transform.position;
        exit.transform.localScale = new Vector3(roomWidth * 1.2f, roomHeight * 1.2f, 1); 

        exit.AddComponent<ExitTrigger>(); 
        Debug.Log("ExitTrigger created at: " + exit.transform.position);
    }
}
