// A* Arama Yol Bulma Harita 1

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarSearch : MonoBehaviour
{
    Grid grid; // Önceden oluþturulmuþ Grid sýnýfýna referans
    Player player; // Önceden oluþturulmuþ Player sýnýfýna referans
    public Transform seeker, target; // Hareket eden oyuncu (seeker) ve hedefin Transform referanslarý
    public bool driveable = true; // Oyuncunun hareket edip edemeyeceðini belirler

    private int totalNodesVisited = 0; // Toplam ziyaret edilen düðüm sayýsý
    public static float totalPathCost = 0;      // Toplam maliyet
    public static float realDistanceToTarget = 0; // Gerçek mesafe
    private int totalCost = 0;  // Toplam maliyet fonksiyonu
    
    private void Awake()
    {
        grid = GetComponent<Grid>(); // Bu nesnedeki Grid bileþenini al
        player = FindObjectOfType<Player>(); // Sahnedeki Player nesnesini bul
    }

    private void Update()
    {
        FindPath(seeker.position, target.position); // Seeker ve hedef arasýndaki yolu bul
        GoToTarget(); // Hedefe doðru hareket et
    }

    void GoToTarget()
    {
        // Eðer bir yol varsa ve oyuncu hareket edebiliyorsa
        if (grid.path1 != null && grid.path1.Count > 0 && driveable) // path1 grid sýnýfýnda List<Node> olarak tanýmlanmýþ (yol temsil ediyor)
        {
            Vector3 hedefNokta = grid.path1[0].WorldPosition; // Yolun ilk düðümünün dünya pozisyonunu al
            player.LookToTarget(hedefNokta); // Oyuncuyu hedefe doðru döndür
            player.GidilcekYer(hedefNokta); // Oyuncuyu hedef pozisyona hareket ettir
        }
    }

    void FindPath(Vector3 startPoz, Vector3 targetPoz)
    {
        Node startNode = grid.NodeFromWorldPoint(startPoz); // Baþlangýç düðümünü bul
        Node targetNode = grid.NodeFromWorldPoint(targetPoz); // Hedef düðümünü bul

        List<Node> openSet = new List<Node>(); // Deðerlendirilecek düðümler
        HashSet<Node> closedSet = new HashSet<Node>(); // Deðerlendirilmiþ düðümler
        // CSV
        // Gerçek mesafeyi hesapla (Euclidean Distance)
        realDistanceToTarget = Vector3.Distance(startPoz, targetPoz);

        openSet.Add(startNode); // Baþlangýç düðümünü açýk sete ekle

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            // En düþük fCost'a sahip düðümü bul (eþitlik durumunda en düþük hCost'a bakýlýr)
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode); // Þu anki düðümü açýk listeden çýkar
            closedSet.Add(currentNode); // Þu anki düðümü kapalý listeye ekle

            // Eðer hedefe ulaþýldýysa yolu geri izleyerek oluþtur
            if (currentNode == targetNode) // Yol bulundu
            {
                
                
                RetracePath(startNode, targetNode);
                //CSV
                totalPathCost = totalCost; // Hedefe ulaþýldýðýnda toplam maliyeti kaydet
                LogAlgorithmResultsOnce(totalNodesVisited, (int)totalPathCost, realDistanceToTarget);
                return;
            }

            // Þu anki düðümün tüm komþularýný kontrol et
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue; // Yürünebilir deðilse veya zaten kapalý setteyse devam et
                }

                // Kavþakta deðilken doðru hareket kurallarýný uygula
                if (!currentNode.kavsak && !neighbour.kavsak)
                {
                    // Yön kontrolü
                    if (currentNode.gridY < neighbour.gridY && !currentNode.right) // Yukarý hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX < neighbour.gridX && !currentNode.right) // Saða hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridY > neighbour.gridY && !currentNode.left) // Aþaðý hareket (left == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX > neighbour.gridX && !currentNode.left) // Sola hareket (left == true)
                    {
                        continue;
                    }
                }

                // Kavþaklar dýþýnda saðdan sola veya soldan saða geçiþ yapýlmasýný engelle
                if (!currentNode.kavsak)
                {
                    if (currentNode.right && neighbour.left)
                    {
                        continue;
                    }
                    if (currentNode.left && neighbour.right)
                    {
                        continue;
                    }
                }

                // Kavþakta esneklik saðla
                if (currentNode.kavsak)
                {
                    // Kavþak özel hareket mantýðý
                    if (currentNode.right && neighbour.left)
                    {
                        continue;
                    }
                    if (currentNode.left && neighbour.right)
                    {
                        continue;
                    }
                }

                // Komþu düðüm için heuristic maliyetini GetDistance fonk. kullanarak hesapla
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                // Eðer komþu düðüm açýk listede deðilse, ekle
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode); // Yol düðümlerini geri izleyerek listeye ekle
            totalNodesVisited++; // Bir düðüm ziyaret edildi
            currentNode = currentNode.parent;
        }

        path.Reverse(); // Yolun doðru sýrasýný elde etmek için ters çevir
        grid.path1 = path; // Bulunan yolu grid'in path1 deðiþkenine ata
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX); // X eksenindeki farký al
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY); // Y eksenindeki farký al

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY); // Çapraz hareket maliyeti + düz hareket maliyeti
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }
    private bool isLogged = false; // Tek seferlik loglama kontrolü
    private void LogAlgorithmResultsOnce(int totalNodesVisited, int totalPathCost, float realDistanceToTarget)
    {
        if (!isLogged)
        {
            isLogged = true;

            CsvLogger.Log("", "", 0, 0, "", "AStarSearch", totalNodesVisited, totalPathCost, realDistanceToTarget);
        }
    }

    
}