using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    public Transform player;
    public LayerMask UnwalkableMask;
    public Vector2 GridWroldSize;            //harita boyut
    public float NodeRadius;                 //grid yaricap
    public Node[,] grid;

    float NodeDinameter;                    //grid cap
    int GridSizeX, GridSizeY;                //grid boyut

    public List<Node> path1 = new List<Node>();

    public bool yoluCiz = false; // ÖNCEKİ DOSYA ÜZERİNE EKLENDİ

    public void Start()
    {
        NodeDinameter = NodeRadius * 2;
        GridSizeX = Mathf.RoundToInt(GridWroldSize.x / NodeDinameter);
        GridSizeY = Mathf.RoundToInt(GridWroldSize.y / NodeDinameter);
        CreateGrid();
    }

    private void Update()
    {
        CreateGrid();
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + GridWroldSize.x / 2) / GridWroldSize.x;
        float percentY = (worldPosition.z + GridWroldSize.y / 2) / GridWroldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((GridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((GridSizeY - 1) * percentY);

        return grid[x, y];
    }



    private void CreateGrid()
    {
        grid = new Node[GridSizeX, GridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * GridWroldSize.x / 2 - Vector3.forward * GridWroldSize.y / 2; // haritanın en sol alt köşesine git

        for (int i = 0; i < GridSizeX; i++)
        {
            for (int j = 0; j < GridSizeY; j++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (NodeRadius + i * NodeDinameter) + Vector3.forward * (NodeRadius + j * NodeDinameter);
                bool walkable = !(Physics.CheckSphere(worldPoint, NodeRadius, UnwalkableMask));

                // Yeni düğümü oluştur
                grid[i, j] = new Node(walkable, worldPoint, i, j);

                // Sağdaki düğümü kontrol et
                if (i + 1 < GridSizeX) // Eğer sağındaki Node grid sınırları içindeyse
                {
                    Vector3 rightNodeWorldPoint = worldBottomLeft + Vector3.right * (NodeRadius + (i + 1) * NodeDinameter) + Vector3.forward * (NodeRadius + j * NodeDinameter);
                    bool rightNodeWalkable = !(Physics.CheckSphere(rightNodeWorldPoint, NodeRadius, UnwalkableMask));
                    if (!rightNodeWalkable)
                    {
                        grid[i, j].right = true;
                    }
                }

                // Aşağıdaki Node'u kontrol et
                if (j - 1 >= 0) // Eğer aşağıdaki Node grid sınırları içindeyse
                {
                    Vector3 belowNodeWorldPoint = worldBottomLeft + Vector3.right * (NodeRadius + i * NodeDinameter) + Vector3.forward * (NodeRadius + (j - 1) * NodeDinameter);
                    bool belowNodeWalkable = !(Physics.CheckSphere(belowNodeWorldPoint, NodeRadius, UnwalkableMask));
                    if (!belowNodeWalkable)
                    {
                        grid[i, j].right = true;
                    }
                }

                // Soldaki Node'u kontrol et
                if (i - 1 >= 0) // Eğer solundaki Node grid sınırları içindeyse
                {
                    Vector3 leftNodeWorldPoint = worldBottomLeft + Vector3.right * (NodeRadius + (i - 1) * NodeDinameter) + Vector3.forward * (NodeRadius + j * NodeDinameter);
                    bool leftNodeWalkable = !(Physics.CheckSphere(leftNodeWorldPoint, NodeRadius, UnwalkableMask));
                    if (!leftNodeWalkable)
                    {
                        grid[i, j].left = true;
                    }
                }

                // Yukarıdaki Node'u kontrol et
                if (j + 1 < GridSizeY) // Eğer yukarısındaki Node grid sınırları içindeyse
                {
                    Vector3 aboveNodeWorldPoint = worldBottomLeft + Vector3.right * (NodeRadius + i * NodeDinameter) + Vector3.forward * (NodeRadius + (j + 1) * NodeDinameter);
                    bool aboveNodeWalkable = !(Physics.CheckSphere(aboveNodeWorldPoint, NodeRadius, UnwalkableMask));
                    if (!aboveNodeWalkable)
                    {
                        grid[i, j].left = true;
                    }
                }

                // Kavşak kontrolü (Layer 11 ile temas ediyorsa)
                if (Physics.CheckSphere(worldPoint, NodeRadius, 1 << 11)) // Layer 11 kontrolü
                {
                    grid[i, j].kavsak = true; // Kavşak olarak işaretle
                    grid[i, j].right = false;
                    grid[i, j].left = false;
                }
            }
        }
    }




    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                if (x == -1 && y == -1)
                    continue;
                if (x == 1 && y == 1)
                    continue;
                if (x == 1 && y == -1)
                    continue;
                if (x == -1 && y == 1)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < GridSizeX && checkY >= 0 && checkY < GridSizeY)     //mapin icinde olup olmadigini kontrol ediyor
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }




    void OnDrawGizmos()
    {

        if (yoluCiz)
        {

            DrawOnlyPath();
        }
        else
        {
            DrawPath();
        }
    }

    void DrawPath()
    {
        if (grid != null)
        {
     //       Node PlayerNode = NodeFromWorldPoint(player.position);



            // 2. Adım: Walkable düğümleri yeşil renkle çiz
            foreach (Node node in grid)
            {
                if (node.Walkable)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
                }
            }

            // 3. Adım: right == true olan düğümleri mor renkle çiz
            foreach (Node node in grid)
            {
                if (node.right)
                {
                    Gizmos.color = new Color(0.5f, 0f, 0.5f); // Mor renk
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
                }
            }
            // 3. Adım: left == true olan düğümleri mor renkle çiz
            foreach (Node node in grid)
            {
                if (node.left)
                {
                    Gizmos.color = new Color(0f, 0.5f, 1f); // Parlak mavi
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
                }
            }
            // 1. Adım: Unwalkable düğümleri kırmızı renkle çiz
            foreach (Node node in grid)
            {
                if (!node.Walkable)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
                }
            }

            foreach (Node node in grid)
            {
                if (node.kavsak)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
                }
            }

            //// 4. Adım: Oyuncunun bulunduğu düğüm (cyan renkte çizilir)
            //if (PlayerNode != null)
            //{
            //    Gizmos.color = Color.cyan;
            //    Gizmos.DrawCube(PlayerNode.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
            //}
        }

        //// 5. Adım: Hedefe giden yolu siyah renkte çiz
        //if (path1 != null && path1.Count > 0)
        //{
        //    foreach (Node node in path1)
        //    {
        //        Gizmos.color = Color.black;
        //        Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
        //    }
        //}
    }



    void DrawOnlyPath()
    {
        Debug.Log("calisti");
        if (path1 != null && path1.Count > 0)
        {
            foreach (Node node in path1)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
            }
        }
    }

    public bool GridCheckWalkable(Vector3 nodePoz)
    {
        Node node = NodeFromWorldPoint(nodePoz);
        
        if (node == null)
        {
            return false; 
        }

        return node.Walkable;
    }

   public Vector3 GetRandomWalkablePosition()
    {
        List<Node> walkableNodes = new List<Node>();

        foreach (Node node in grid)
        {
            if (node.Walkable)
            {
                walkableNodes.Add(node);
            }
        }

        if (walkableNodes.Count > 0)
        {
            Node randomNode = walkableNodes[UnityEngine.Random.Range(0, walkableNodes.Count)];
            return randomNode.WorldPosition;
        }

        // Hiçbir Walkable düğüm yoksa varsayılan pozisyon döndür
        Debug.LogWarning("No walkable nodes found!");
        return Vector3.zero;
    }


}