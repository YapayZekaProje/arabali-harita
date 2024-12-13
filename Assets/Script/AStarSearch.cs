// A* Arama Yol Bulma Harita 1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarSearch : MonoBehaviour
{
    Grid grid; // Önceden oluşturulmuş Grid sınıfına referans
    Player player; // Önceden oluşturulmuş Player sınıfına referans
    public Transform seeker, target; // Hareket eden oyuncu (seeker) ve hedefin Transform referansları
    public bool driveable = true; // Oyuncunun hareket edip edemeyeceğini belirler

    private void Awake()
    {
        grid = GetComponent<Grid>(); // Bu nesnedeki Grid bileşenini al
        player = FindObjectOfType<Player>(); // Sahnedeki Player nesnesini bul
    }

    private void Update()
    {
        FindPath(seeker.position, target.position); // Seeker ve hedef arasındaki yolu bul
        GoToTarget(); // Hedefe doğru hareket et
    }

    void GoToTarget()
    {
        // Eğer bir yol varsa ve oyuncu hareket edebiliyorsa
        if (grid.path1 != null && grid.path1.Count > 0 && driveable) // path1 grid sınıfında List<Node> olarak tanımlanmış (yol temsil ediyor)
        {
            Vector3 hedefNokta = grid.path1[0].WorldPosition; // Yolun ilk düğümünün dünya pozisyonunu al
            player.LookToTarget(hedefNokta); // Oyuncuyu hedefe doğru döndür
            player.GidilcekYer(hedefNokta); // Oyuncuyu hedef pozisyona hareket ettir
        }
    }

    void FindPath(Vector3 startPoz, Vector3 targetPoz)
    {
        Node startNode = grid.NodeFromWorldPoint(startPoz); // Başlangıç düğümünü bul
        Node targetNode = grid.NodeFromWorldPoint(targetPoz); // Hedef düğümünü bul

        List<Node> openSet = new List<Node>(); // Değerlendirilecek düğümler
        HashSet<Node> closedSet = new HashSet<Node>(); // Değerlendirilmiş düğümler

        openSet.Add(startNode); // Başlangıç düğümünü açık sete ekle

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            // En düşük fCost'a sahip düğümü bul (eşitlik durumunda en düşük hCost'a bakılır)
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode); // Şu anki düğümü açık listeden çıkar
            closedSet.Add(currentNode); // Şu anki düğümü kapalı listeye ekle

            // Eğer hedefe ulaşıldıysa yolu geri izleyerek oluştur
            if (currentNode == targetNode) // Yol bulundu
            {
                RetracePath(startNode, targetNode);
                return;
            }

            // Şu anki düğümün tüm komşularını kontrol et
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue; // Yürünebilir değilse veya zaten kapalı setteyse devam et
                }

                // Kavşakta değilken doğru hareket kurallarını uygula
                if (!currentNode.kavsak && !neighbour.kavsak)
                {
                    // Yön kontrolü
                    if (currentNode.gridY < neighbour.gridY && !currentNode.right) // Yukarı hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX < neighbour.gridX && !currentNode.right) // Sağa hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridY > neighbour.gridY && !currentNode.left) // Aşağı hareket (left == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX > neighbour.gridX && !currentNode.left) // Sola hareket (left == true)
                    {
                        continue;
                    }
                }

                // Kavşaklar dışında sağdan sola veya soldan sağa geçiş yapılmasını engelle
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

                // Kavşakta esneklik sağla
                if (currentNode.kavsak)
                {
                    // Kavşak özel hareket mantığı
                    if (currentNode.right && neighbour.left)
                    {
                        continue;
                    }
                    if (currentNode.left && neighbour.right)
                    {
                        continue;
                    }
                }

                // Komşu düğüm için heuristic maliyetini GetDistance fonk. kullanarak hesapla
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                // Eğer komşu düğüm açık listede değilse, ekle
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
            path.Add(currentNode); // Yol düğümlerini geri izleyerek listeye ekle
            currentNode = currentNode.parent;
        }

        path.Reverse(); // Yolun doğru sırasını elde etmek için ters çevir
        grid.path1 = path; // Bulunan yolu grid'in path1 değişkenine ata
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX); // X eksenindeki farkı al
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY); // Y eksenindeki farkı al

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY); // Çapraz hareket maliyeti + düz hareket maliyeti
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
