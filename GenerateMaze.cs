using System;                              
using System.Collections;                   
using System.Collections.Generic;            
using UnityEngine;                          
using UnityEngine.SceneManagement;           // Để đổi scene

public class GenerateMaze : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;  // Prefab của mỗi ô phòng trong mê cung
    [SerializeField] private int numX = 10;          // Số phòng theo trục X (sẽ bị ghi đè bởi MazeSettings)
    [SerializeField] private int numY = 10;          // Số phòng theo trục Y

    [Header("Start area (reuse roomPrefab)")]
    [SerializeField] private int startAreaSize = 5;     // Kích thước hình vuông khu vực xuất phát
    [SerializeField] private int startPathLength = 5;   // Độ dài đường nối từ start area đến mê cung

    private Room[,] rooms;               // Mảng 2D chứa tất cả phòng mô phỏng mê cung
    private float roomWidth;             // Chiều rộng 1 phòng (dựa trên sprite bounds)
    private float roomHeight;            // Chiều cao 1 phòng

    private List<Room> startPathRooms = new List<Room>();  // Danh sách các phòng tạo thành đường nối từ khu start

    // Class Edge dùng trong thuật toán Kruskal
    private class Edge
    {
        public Vector2Int A;             // Vị trí phòng A
        public Vector2Int B;             // Vị trí phòng B
        public float weight;             // Trọng số để sort random (Random.value)
        public Room.Directions dirFromA; // Hướng từ A đi sang B

        public Edge(Vector2Int a, Vector2Int b, Room.Directions dir, float w)
        {
            A = a; 
            B = b; 
            dirFromA = dir; 
            weight = w;
        }
    }

    // Disjoint set (Union-Find) để phục vụ thuật toán Kruskal
    private class DisjointSet
    {
        private int[] parent;   // Mỗi phần tử lưu cha của nó
        private int[] rank;     // Độ sâu của cây (quan trọng trong union)

        public DisjointSet(int size)
        {
            parent = new int[size];
            rank = new int[size];

            // Khởi tạo mỗi node đều tự là cha của chính nó
            for (int i = 0; i < size; i++)
                parent[i] = i;
        }

        // Tìm đại diện của tập hợp (có nén đường dẫn)
        private int Find(int x)
        {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);  // Gộp đường dẫn
            return parent[x];
        }

        // Hợp nhất 2 tập — trả về false nếu đã cùng tập
        public bool Union(int a, int b)
        {
            int rootA = Find(a);
            int rootB = Find(b);

            if (rootA == rootB) return false; // Đã kết nối → không hợp nhất

            // Hợp nhất theo rank để tối ưu
            if (rank[rootA] < rank[rootB]) parent[rootA] = rootB;
            else if (rank[rootA] > rank[rootB]) parent[rootB] = rootA;
            else {
                parent[rootB] = rootA;
                rank[rootA]++;
            }

            return true;
        }

        // Kiểm tra 2 node có cùng tập không
        public bool Connected(int a, int b) => Find(a) == Find(b);
    }

    private void Awake()
    {
        numX = MazeSettings.numX;   // Lấy số lượng phòng X người chơi nhập
        numY = MazeSettings.numY;   // Lấy số lượng phòng Y người chơi nhập
    }

    private void Start()
    {
        GetRoomSize();               // Lấy chiều dài + chiều rộng của prefab
        SpawnStartArea();            // Tạo khu xuất phát hình vuông
        SpawnStartPath();            // Tạo đường nối từ start area vào mê cung
        InitializeRooms();           // Tạo toàn bộ phòng mê cung
        CreateMaze();                // Tạo mê cung bằng Kruskal
        AddExitTrigger();            // Thêm trigger chiến thắng
        SetCamera();                 // Căn camera sao cho bao trọn mê cung
        MovePlayerToStartCenter();   // Đưa người chơi vào giữa khu xuất phát
    }
    //TẠO KHU START
    private void SpawnStartArea()
    {
        float originX = 0f;                 // Gốc X của khu start
        int half = startAreaSize / 2;       // Tính để căn giữa
        float originY = -half * roomHeight; // Gốc Y

        // Tạo startAreaSize × startAreaSize phòng hình vuông
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

                // Gán index âm để phân biệt với phòng chính
                r.Index = new Vector2Int(-1000 + sx, -1000 + sy);

                // Tắt tường giữa các ô để tạo vùng trống to
                if (sx > 0) r.SetDirFlag(Room.Directions.LEFT, false);
                if (sx < startAreaSize - 1) r.SetDirFlag(Room.Directions.RIGHT, false);
                if (sy > 0) r.SetDirFlag(Room.Directions.BOTTOM, false);
                if (sy < startAreaSize - 1) r.SetDirFlag(Room.Directions.TOP, false);
            }
        }
    }
    //TẠO ĐƯỜNG DẪN TỪ START AREA → MÊ CUNG
    private void SpawnStartPath()
    {
        float pathY = 0f;   // Tạo 1 đường ngang, Y = 0
        Room prev = null;

        for (int i = 0; i < startPathLength; i++)
        {
            float x = (startAreaSize + i) * roomWidth;
            Vector3 pos = new Vector3(x, pathY, 0f);

            GameObject obj = Instantiate(roomPrefab, pos, Quaternion.identity);
            obj.name = $"Path_{i}";

            Room room = obj.GetComponent<Room>();
            room.Index = new Vector2Int(-200, i);

            // Nối phòng trước → phòng sau
            if (prev != null)
            {
                prev.SetDirFlag(Room.Directions.RIGHT, false);
                room.SetDirFlag(Room.Directions.LEFT, false);
            }
            else
            {
                // Nối phòng đầu tiên với cạnh phải của khu start
                int mid = startAreaSize / 2;
                string startCellName = $"Start_{startAreaSize - 1}_{mid}";
                GameObject startCellObj = GameObject.Find(startCellName);

                if (startCellObj != null)
                {
                    Room startCellRoom = startCellObj.GetComponent<Room>();

                    startCellRoom.SetDirFlag(Room.Directions.RIGHT, false);
                    room.SetDirFlag(Room.Directions.LEFT, false);
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
    //KHỞI TẠO MÊ CUNG CHÍNH (rooms[x,y])
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
    //  LẤY CHIỀU RỘNG + CAO CỦA PREFAB ROOM
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
    //  CĂN CAMERA CHO VỪA MÊ CUNG
    private void SetCamera()
    {
        float centerX =
            (startAreaSize + startPathLength) * roomWidth + (numX * roomWidth) / 2f - roomWidth / 2f;

        float centerY =
            (numY * roomHeight) / 2f - roomHeight / 2f;

        Camera.main.transform.position = new Vector3(centerX, centerY, -100.0f);

        float min_value = Mathf.Min(
            (startAreaSize + startPathLength + numX) * roomWidth,
            numY * roomHeight
        );

        Camera.main.orthographicSize = min_value * 0.5f;
    }
    // HÀM TẠO CHỈ SỐ LIÊN TIẾP CHO MA TRẬN
    private int GetIndex(int x, int y) => y * numX + x;
    //  RESET TẤT CẢ TƯỜNG TRƯỚC KHI TẠO MAZE
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
    //  GỌI TẠO MAZE
    public void CreateMaze()
    {
        ResetRooms();                // Bật tất cả tường
        GenerateMazeWithKruskal();   // Thuật toán Kruskal

        // Mở đường từ end của start path sang mê cung
        if (startPathRooms.Count > 0)
            startPathRooms[startPathRooms.Count - 1]
                .SetDirFlag(Room.Directions.RIGHT, false);

        // Mở tường trái phòng [0,0]
        rooms[0, 0].SetDirFlag(Room.Directions.LEFT, false);
    }
    // THUẬT TOÁN KRUSKAL → TẠO MÊ CUNG HOÀN CHỈNH
    private void GenerateMazeWithKruskal()
    {
        List<Edge> edges = new List<Edge>();

        // Tạo danh sách các cạnh – mỗi cạnh là tường có thể phá
        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                Vector2Int current = new Vector2Int(x, y);

                // Cạnh sang phải
                if (x < numX - 1)
                    edges.Add(new Edge(
                        current,
                        new Vector2Int(x + 1, y),
                        Room.Directions.RIGHT,
                        UnityEngine.Random.value
                    ));

                // Cạnh lên trên
                if (y < numY - 1)
                    edges.Add(new Edge(
                        current,
                        new Vector2Int(x, y + 1),
                        Room.Directions.TOP,
                        UnityEngine.Random.value
                    ));
            }
        }

        // Sort random
        edges.Sort((a, b) => a.weight.CompareTo(b.weight));

        DisjointSet ds = new DisjointSet(numX * numY);

        foreach (var e in edges)
        {
            int indexA = GetIndex(e.A.x, e.A.y);
            int indexB = GetIndex(e.B.x, e.B.y);

            // Nếu 2 phòng chưa được kết nối → Union và phá tường
            if (!ds.Connected(indexA, indexB))
            {
                ds.Union(indexA, indexB);
                RemoveWall(e.A, e.dirFromA);
            }
        }

        // Mở tường phải phòng cuối để đặt ExitTrigger
        RemoveWall(new Vector2Int(numX - 1, numY - 1), Room.Directions.RIGHT);
    }
    //  PHÁ TƯỜNG CỦA PHÒNG A VÀ PHÒNG B
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

        // Nếu phòng đối diện hợp lệ → phá tường đối diện
        if (other.x >= 0 && other.x < numX && other.y >= 0 && other.y < numY)
            rooms[other.x, other.y].SetDirFlag(opposite, false);
    }
    //  ĐƯA NGƯỜI CHƠI VỀ GIỮA VÙNG START
    private void MovePlayerToStartCenter()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float centerX = ((startAreaSize - 1) / 2f) * roomWidth;
        float centerY = 0f;

        player.transform.position = new Vector3(
            centerX, 
            centerY, 
            player.transform.position.z
        );
    }
    //  TẠO Ô EXIT (WIN GAME)
    private void AddExitTrigger()
    {
        GameObject exit = new GameObject("ExitTrigger");

        BoxCollider2D col = exit.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Vector3 lastRoomPos = rooms[numX - 1, numY - 1].transform.position;

        exit.transform.position =
            lastRoomPos + new Vector3(roomWidth, 0f, 0f);

        exit.transform.localScale =
            new Vector3(roomWidth * 1.2f, roomHeight * 1.2f, 1);

        exit.AddComponent<ExitTrigger>();

        Debug.Log("ExitTrigger created at: " + exit.transform.position);
    }
}

