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

    [Header("Start area (reuse roomPrefab)")]
    [SerializeField] private int startAreaSize = 5;   
    [SerializeField] private int startPathLength = 5;  

    private Room[,] rooms;
    private float roomWidth;
    private float roomHeight;

    private List<Room> startPathRooms = new List<Room>();

    private class Edge
    {
        public Vector2Int A;
        public Vector2Int B;
        public float weight;
        public Room.Directions dirFromA;

        public Edge(Vector2Int a, Vector2Int b, Room.Directions dir, float w)
        {
            A = a; B = b; dirFromA = dir; weight = w;
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

    private void Awake()
    {
        numX = MazeSettings.numX;
        numY = MazeSettings.numY;
    }

    private void Start()
    {
        GetRoomSize();
        SpawnStartArea();
        SpawnStartPath();
        InitializeRooms();
        CreateMaze();
        AddExitTrigger();
        SetCamera();
        MovePlayerToStartCenter();
    }
    private void SpawnStartArea()
    {
        float originX = 0f;
        int half = startAreaSize / 2;
        float originY = -half * roomHeight;

        for (int sx = 0; sx < startAreaSize; sx++)
        {
            for (int sy = 0; sy < startAreaSize; sy++)
            {
                Vector3 pos = new Vector3(
                    originX + sx * roomWidth,
                    originY + sy * roomHeight,
                    0f
                );

                GameObject obj = Instantiate(roomPrefab, pos, Quaternion.identity);
                obj.name = $"Start_{sx}_{sy}";
                Room r = obj.GetComponent<Room>();
                r.Index = new Vector2Int(-1000 + sx, -1000 + sy);
                if (sx > 0) r.SetDirFlag(Room.Directions.LEFT, false);
                if (sx < startAreaSize - 1) r.SetDirFlag(Room.Directions.RIGHT, false);
                if (sy > 0) r.SetDirFlag(Room.Directions.BOTTOM, false);
                if (sy < startAreaSize - 1) r.SetDirFlag(Room.Directions.TOP, false);
            }
        }
    }
    private void SpawnStartPath()
    {
        float pathY = 0f;

        Room prev = null;

        for (int i = 0; i < startPathLength; i++)
        {
            float x = (startAreaSize + i) * roomWidth;
            Vector3 pos = new Vector3(x, pathY, 0f);
            GameObject obj = Instantiate(roomPrefab, pos, Quaternion.identity);
            obj.name = $"Path_{i}";
            Room room = obj.GetComponent<Room>();
            room.Index = new Vector2Int(-200, i);

            if (prev != null)
            {
                prev.SetDirFlag(Room.Directions.RIGHT, false);
                room.SetDirFlag(Room.Directions.LEFT, false);
            }
            else
            {
                int mid = startAreaSize / 2;
                string startCellName = $"Start_{startAreaSize - 1}_{mid}";
                GameObject startCellObj = GameObject.Find(startCellName);
                if (startCellObj != null)
                {
                    Room startCellRoom = startCellObj.GetComponent<Room>();
                    if (startCellRoom != null)
                    {
                        startCellRoom.SetDirFlag(Room.Directions.RIGHT, false);
                        room.SetDirFlag(Room.Directions.LEFT, false);
                    }
                }
                else
                {
                    room.SetDirFlag(Room.Directions.LEFT, false);
                }
            }

            prev = room;
            startPathRooms.Add(room);
        }
    }

    private void InitializeRooms()
    {
        float offsetX = (startAreaSize + startPathLength) * roomWidth;

        rooms = new Room[numX, numY];

        for (int i = 0; i < numX; i++)
        {
            for (int j = 0; j < numY; j++)
            {
                GameObject room = Instantiate(
                    roomPrefab,
                    new Vector3(offsetX + i * roomWidth, j * roomHeight, 0),
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
        float centerX = (startAreaSize + startPathLength) * roomWidth + (numX * roomWidth) / 2f - roomWidth / 2f;
        float centerY = (numY * roomHeight) / 2f - roomHeight / 2f;

        Camera.main.transform.position = new Vector3(centerX, centerY, -100.0f);

        float min_value = Mathf.Min((startAreaSize + startPathLength + numX) * roomWidth, numY * roomHeight);
        Camera.main.orthographicSize = min_value * 0.5f;
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
        if (startPathRooms.Count > 0)
            startPathRooms[startPathRooms.Count - 1].SetDirFlag(Room.Directions.RIGHT, false);
        rooms[0, 0].SetDirFlag(Room.Directions.LEFT, false);
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

    private void MovePlayerToStartCenter()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        float centerX = ((startAreaSize - 1) / 2f) * roomWidth;
        float centerY = 0f;

        player.transform.position = new Vector3(centerX, centerY, player.transform.position.z);
    }

    private void AddExitTrigger()
    {
        GameObject exit = new GameObject("ExitTrigger");
        BoxCollider2D col = exit.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Vector3 lastRoomPos = rooms[numX - 1, numY - 1].transform.position;
        exit.transform.position = lastRoomPos + new Vector3(roomWidth, 0f, 0f);
        exit.transform.localScale = new Vector3(roomWidth * 1.2f, roomHeight * 1.2f, 1);

        exit.AddComponent<ExitTrigger>();
        Debug.Log("ExitTrigger created at: " + exit.transform.position);
    }
}
