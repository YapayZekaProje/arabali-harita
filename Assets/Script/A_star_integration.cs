using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform seeker, target;
    Player player;
    private bool requestSent = false; // KELVÝNDEN ALINDI

    // CSV variables
    private int totalNodesVisited = 0; // Total visited nodes
    public static int totalCost = 0; // Total path cost
    public static float realDistanceToTarget = 0; // Real distance to target
    private bool isLogged = false; // Tek seferlik loglama kontrolü

    private void Awake()
    {
        grid = GetComponent<Grid>();
        player = FindObjectOfType<Player>();  // Player'ý bul


        // KELVÝNDEN ALINDI
        Debug.Log($"Astar is : {player.isAstar}");
        grid.Start(); // Grid'in oluþturulduðundan emin olun
        PrintGridInfoToFile(); // Grid detaylarýný bir dosyaya yazdýrýn

    }


    private void Update()
    {
        // KELVÝNDEN ALINDI
        if (player.isAstar)
        {
            Debug.Log("Update metodu çaðrýldý.");
            RequestPathFromServer(seeker.position, target.position);
        }
    }


    // KELVÝNDEN ALINDI
    // Grid bilgisini bir dosyaya yazdýran metod
    void PrintGridInfoToFile()
    {
        if (grid.grid == null)
        {
            Debug.LogError("Grid henüz baþlatýlmadý.");
            return;
        }

        string filePath = Path.Combine(Application.dataPath, "Script", "GridInfo.json");// Chemin vers le fichier JSON
        List<Dictionary<string, object>> gridInfoList = new List<Dictionary<string, object>>();

        float nodeRadius = grid.NodeRadius; // Node yarýçapýný alýn

        // Grid üzerindeki her bir düðümü dolaþarak bilgilerini kaydedin
        for (int x = 0; x < grid.grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.grid.GetLength(1); y++)
            {
                Node node = grid.grid[x, y];
                var nodeInfo = new Dictionary<string, object>
                {
                    { "gridX", node.gridX },
                    { "gridY", node.gridY },
                    { "worldX", node.WorldPosition.x },
                    { "worldZ", node.WorldPosition.z },
                    { "walkable", node.Walkable },
                    { "right", node.right },
                    { "left", node.left },
                    { "kavsak", node.kavsak }
                };
                gridInfoList.Add(nodeInfo);
            }
        }

        // JSON yapýsýný oluþturun ve dosyaya yazdýrýn
        var finalJsonStructure = new
        {
            nodeRadius = nodeRadius,
            gridSizeX = grid.grid.GetLength(0),
            gridSizeY = grid.grid.GetLength(1),
            nodes = gridInfoList
        };

        string json = JsonConvert.SerializeObject(finalJsonStructure, Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.Log($"Grid bilgisi þu dosyaya kaydedildi: {filePath}");
    }

    // Server'dan yol isteði gönderen metod
    void RequestPathFromServer(Vector3 startPoz, Vector3 targetPoz)
    {
        if (requestSent)
        {
            Debug.Log("Ýstek zaten gönderildi, yanýt bekleniyor...");
            return;
        }

        string serverIP = "127.0.0.1"; // Sunucunun IP adresi
        int port = 8089; // Python sunucusuyla eþleþen port numarasý

        try
        {
            using (TcpClient client = new TcpClient(serverIP, port))
            using (NetworkStream stream = client.GetStream())
            {
                //CSV
                realDistanceToTarget = Vector3.Distance(startPoz, targetPoz); // Gerçek mesafeyi hesapla

                // Baþlangýç ve hedef pozisyonlarýný gönderin
                string request = $"{startPoz.x},{startPoz.y},{startPoz.z};{targetPoz.x},{targetPoz.y},{targetPoz.z}";
                Debug.Log($"Sunucuya istek gönderiliyor: {request}");
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                // Yanýt alýn
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Debug.Log($"Sunucudan gelen yanýt: {response}");

                ParsePathResponse(response);
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError($"SocketException: {ex.Message}");
        }

        requestSent = true; // Yeni istek gönderilmesini engelle
    }

    // Sunucudan gelen yolu çözümleyen metod
    void ParsePathResponse(string response)
    {
        string[] pathNodes = response.Split(';');
        List<Node> path = new List<Node>();

        foreach (string nodeData in pathNodes)
        {
            if (string.IsNullOrEmpty(nodeData)) continue;

            string[] nodeCoordinates = nodeData.Split(',');
            if (nodeCoordinates.Length == 3)
            {
                // Grid koordinatlarýný kullanarak düðümü bulun
                int gridX = int.Parse(nodeCoordinates[0]);
                int gridY = int.Parse(nodeCoordinates[1]);

                Node node = grid.grid[gridX, gridY]; // Grid üzerinden düðümü alýn
                if (node != null)
                {
                    path.Add(node);

                    //CSV
                    totalNodesVisited++; // Ziyaret edilen düðüm sayýsýný artýr
                }
            }
        }

        // Oyuncuya yolu ayarla
        player.SetPath(path);

        // Yolu yazdýr ve CSV'ye kaydet
        LogAlgorithmResultsOnce(totalNodesVisited, totalCost, realDistanceToTarget);

    }
    // Algoritma bittiðinde sonuçlarý yalnýzca bir kez kaydet
    private void LogAlgorithmResultsOnce(int totalNodesVisited, int totalCost, float realDistanceToTarget)
    {
        if (!isLogged) // Eðer henüz loglanmadýysa
        {
            isLogged = true; // Loglama durumunu iþaretle
            CsvLogger.Log("", "", 0, 0, "", "AStar", totalNodesVisited, totalCost, realDistanceToTarget); // Gerçek mesafeyi ekleyin
        }
    }

}
