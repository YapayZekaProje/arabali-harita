// Uniform Cost fath finding for map 1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniformCostSearch : MonoBehaviour
{
    Grid grid; // Grid bileşenine referans
    Player player; // Player bileşenine referans
    public Transform seeker, target; // Harita üzerindeki arayıcı (player) ve hedef
    public bool driveable = true; // Oyuncunun hareket edip edemeyeceği

    private void Awake()
    {
        grid = GetComponent<Grid>(); // Aynı nesneye bağlı olan Grid bileşenini al
        player = FindObjectOfType<Player>();  // Sahnedeki Player nesnesini bul
    }

    private void Update()
    {
        FindPath(seeker.position, target.position); // Arayıcıdan hedefe giden yolu bul
        GoToTarget(); // Oyuncuyu yol boyunca hedefe hareket ettir
    }

    void GoToTarget()
    {
        // Eğer bir yol varsa ve oyuncunun hareket etmesine izin veriliyorsa
        if (grid.path1 != null && grid.path1.Count > 0 && driveable)
        {
            Vector3 targetPoint = grid.path1[0].WorldPosition;  // Yolun ilk noktasını al
            player.LookToTarget(targetPoint); // Oyuncuyu hedefe yönlendir
            player.GidilcekYer(targetPoint);  // Oyuncuyu hedefe doğru hareket ettir
        }
    }

    void FindPath(Vector3 startPoz, Vector3 targetPoz)
    {
        Node startNode = grid.NodeFromWorldPoint(startPoz); // Başlangıç noktasından grid üzerindeki düğümü al
        Node targetNode = grid.NodeFromWorldPoint(targetPoz); // Hedef noktasından grid üzerindeki düğümü al

        PriorityQueue<Node> openSet = new PriorityQueue<Node>(); // Keşfedilecek düğümleri tutan açık küme (altta  PriorityQueue için bir sınıf vardır)
        HashSet<Node> closedSet = new HashSet<Node>(); // Keşfedilen düğümleri tutan kapalı küme

        startNode.gCost = 0; // Başlangıç düğümünün maliyetini 0 olarak ayarla
        openSet.Enqueue(startNode, startNode.gCost); // Başlangıç düğümünü açık kümeye ekle

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue(); // En düşük maliyete sahip düğümü al

            // Hedef düğümüne ulaşıldıysa, yolu geri takip et
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode); // Hedeften başlangıca kadar yolu geri takip et
                return;
            }



            closedSet.Add(currentNode); // Geçerli düğümü kapalı kümeye ekle

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                // Eğer komşu geçilebilir değilse veya zaten keşfedildiyse, atla
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

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

                // Right'tan direkt Left'e veya Left'ten direkt Right'a geçişi engelle
                if (currentNode.right && neighbour.left && !neighbour.kavsak)
                {
                    continue;
                }
                if (currentNode.left && neighbour.right && !neighbour.kavsak)
                {
                    continue;
                }

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour); // Komşuya olan maliyeti hesapla
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    // Komşu düğümünün maliyetini ve ebeveynini güncelle
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Enqueue(neighbour, neighbour.gCost); // Komşuyu açık kümeye ekle
                    }


                }


            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>(); // Nihai yolu tutacak liste
        Node currentNode = endNode;

        while (currentNode != startNode) // Hedef düğümünden başlangıç düğümüne kadar geri git
        {
            path.Add(currentNode); // Mevcut düğümü yola ekle
            currentNode = currentNode.parent; // Ebeveyn düğümüne geç
        }

        path.Reverse(); // Yolu ters çevir, çünkü yol başlangıçtan hedefe doğru olmalı
        grid.path1 = path; // Yolu grid'e ayarla
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX); // X koordinatları arasındaki farkı al
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY); // Y koordinatları arasındaki farkı al

        return 10 * (dstX + dstY);  // Manhattan mesafesini kullan (grid tabanlı hareket için uygun)
    }


}




// PriorityQueue class to store nodes by priority (lowest cost first)
public class PriorityQueue<T> where T : class
{
    private List<KeyValuePair<T, int>> elements = new List<KeyValuePair<T, int>>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add(new KeyValuePair<T, int>(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].Value < elements[bestIndex].Value)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Key;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool Contains(T item)
    {
        return elements.Exists(e => e.Key.Equals(item));
    }
}
