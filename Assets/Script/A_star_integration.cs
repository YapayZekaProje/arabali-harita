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
    private bool requestSent = false; // KELV�NDEN ALINDI

    // CSV variables
    private int totalNodesVisited = 0; // Total visited nodes
    public static int totalCost = 0; // Total path cost
    public static float realDistanceToTarget = 0; // Real distance to target
    private bool isLogged = false; // Tek seferlik loglama kontrol�

    private void Awake()
    {
        grid = GetComponent<Grid>();
        player = FindObjectOfType<Player>();  // Player'� bul


        // KELV�NDEN ALINDI
        Debug.Log($"Astar is : {player.isAstar}");
        grid.Start(); // Grid'in olu�turuldu�undan emin olun
        PrintGridInfoToFile(); // Grid detaylar�n� bir dosyaya�yazd�r�n

    }


    private void Update()
    {
        // KELV�NDEN ALINDI
        if (player.isAstar)
        {
            Debug.Log("Update metodu �a�r�ld�.");
            RequestPathFromServer(seeker.position, target.position);
        }
    }


    // KELV�NDEN ALINDI
    // Grid bilgisini bir dosyaya yazd�ran metod
    void PrintGridInfoToFile()
    {
        if (grid.grid == null)
        {
            Debug.LogError("Grid hen�z ba�lat�lmad�.");
            return;
        }

        string filePath = Path.Combine(Application.dataPath, "Script", "GridInfo.json");// Chemin vers le fichier JSON
        List<Dictionary<string, object>> gridInfoList = new List<Dictionary<string, object>>();

        float nodeRadius = grid.NodeRadius; // Node yar��ap�n� al�n

        // Grid �zerindeki her bir d���m� dola�arak bilgilerini kaydedin
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

        // JSON yap�s�n� olu�turun ve dosyaya yazd�r�n
        var finalJsonStructure = new
        {
            nodeRadius = nodeRadius,
            gridSizeX = grid.grid.GetLength(0),
            gridSizeY = grid.grid.GetLength(1),
            nodes = gridInfoList
        };

        string json = JsonConvert.SerializeObject(finalJsonStructure, Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.Log($"Grid bilgisi �u dosyaya kaydedildi: {filePath}");
    }

    // Server'dan yol iste�i g�nderen metod
    void RequestPathFromServer(Vector3 startPoz, Vector3 targetPoz)
    {
        if (requestSent)
        {
            Debug.Log("�stek zaten g�nderildi, yan�t bekleniyor...");
            return;
        }

        string serverIP = "127.0.0.1"; // Sunucunun IP adresi
        int port = 8089; // Python sunucusuyla e�le�en port numaras�

        try
        {
            using (TcpClient client = new TcpClient(serverIP, port))
            using (NetworkStream stream = client.GetStream())
            {
                //CSV
                realDistanceToTarget = Vector3.Distance(startPoz, targetPoz); // Ger�ek mesafeyi hesapla

                // Ba�lang�� ve hedef pozisyonlar�n� g�nderin
                string request = $"{startPoz.x},{startPoz.y},{startPoz.z};{targetPoz.x},{targetPoz.y},{targetPoz.z}";
                Debug.Log($"Sunucuya istek g�nderiliyor: {request}");
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                // Yan�t al�n
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Debug.Log($"Sunucudan gelen yan�t: {response}");

                ParsePathResponse(response);
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError($"SocketException: {ex.Message}");
        }

        requestSent = true; // Yeni istek g�nderilmesini engelle
    }

    // Sunucudan gelen yolu ��z�mleyen metod
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
                // Grid koordinatlar�n� kullanarak d���m� bulun
                int gridX = int.Parse(nodeCoordinates[0]);
                int gridY = int.Parse(nodeCoordinates[1]);

                Node node = grid.grid[gridX, gridY]; // Grid �zerinden d���m� al�n
                if (node != null)
                {
                    path.Add(node);

                    //CSV
                    totalNodesVisited++; // Ziyaret edilen d���m say�s�n� art�r
                }
            }
        }

        // Oyuncuya yolu ayarla
        player.SetPath(path);

        // Yolu yazd�r ve CSV'ye kaydet
        LogAlgorithmResultsOnce(totalNodesVisited, totalCost, realDistanceToTarget);

    }
    // Algoritma bitti�inde sonu�lar� yaln�zca bir kez kaydet
    private void LogAlgorithmResultsOnce(int totalNodesVisited, int totalCost, float realDistanceToTarget)
    {
        if (!isLogged) // E�er hen�z loglanmad�ysa
        {
            isLogged = true; // Loglama durumunu i�aretle
            CsvLogger.Log("", "", 0, 0, "", "AStar", totalNodesVisited, totalCost, realDistanceToTarget); // Ger�ek mesafeyi ekleyin
        }
    }

}
