using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using TMPro;
using Unity.VisualScripting;


public class Bot : MonoBehaviour
{
    [SerializeField] private GameObject target;
    public GridBot grid;

    public float currentSpeed;
    private float moveSpeed = 5;
    public float maxSpeed;
    public float deceleration = 0.2f; // Yavaþlama hýzý
    public float acceleration = 0.5f;  //hizlanma hizi 
    public bool isSlowingDown = false;
    public bool isAccelerating = true;
    public bool canSkid;
    public bool carSlowDown;

    public float followDistance;

    Bot bot;

    private AudioSource gazAudioSource;

    public bool driveable = true;
    Vector3 baslangicKonumu;
    public float kusUcumuMesafe;

    private void Awake()
    {
        grid = FindObjectOfType<GridBot>();
        bot = FindObjectOfType<Bot>();

        baslangicKonumu = bot.transform.position;

    }

    private void Start()
    {
        target.transform.position = grid.GetRandomWalkablePosition();
    }

    private void Update()
    {
        GameKontrol();
        FindPath(transform.position, target.transform.position);
        GoToTarget();

    }

    public void GidilcekYer(Vector3 hedefNoktasi)
    {
        ++hedefNoktasi.y;

        // Önündeki araç kontrolü
        if (carSlowDown)
        {
            // Öndeki araç varsa yavaşla
            currentSpeed = 0;
            Debug.Log("Öndeki araç yakın, yavaşlıyorum.");
        }

        else if (isSlowingDown)
        {
            currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime);
            Debug.Log("Kırmızı ışık | Yaya");
        }
        else if (canSkid)
        {
            if (isAccelerating)
            {
                currentSpeed = Mathf.Max(maxSpeed / 2, currentSpeed - deceleration * Time.deltaTime);
                Debug.Log("Karlı yol yavaşla");
            }
            else if (isSlowingDown)
            {
                currentSpeed = Mathf.Min(maxSpeed / 2, currentSpeed + acceleration * Time.deltaTime);
                Debug.Log("Karlı yol hızlan");
            }
        }
        else if (isAccelerating)
        {
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
            Debug.Log("Sıkıntı yok devam et");
        }

        // Aracı hedef noktaya hareket ettir
        transform.position = Vector3.MoveTowards(transform.position, hedefNoktasi, currentSpeed * Time.deltaTime);

        LookToTarget(hedefNoktasi); // Hedefe doğru yavaşça dönecek
    }


    public void LookToTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        // Eðer hedefe çok yakýn deðilse
        if (direction.magnitude > 0.1f)
        {
            direction.y = 0;  // y ekseninde dönmeyi engelliyoruz

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Mevcut rotasyonu al ve yalnýzca Y eksenini ayarla
            Vector3 currentRotation = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(currentRotation.x, targetRotation.eulerAngles.y, currentRotation.z);

            // Arabayý sadece Y ekseninde hedefe bakacak þekilde yumuþakça döndür
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);
        }
    }

    private void GameKontrol()
    {
        //if (maxSpeed == 0)
        //{
        //    isSlowingDown = false;
        //}
        if (maxSpeed == currentSpeed)
        {
            isAccelerating = false;
        }
        if (maxSpeed != currentSpeed)
        {

            if (isSlowingDown)
            {
            }
            else
            {
                isAccelerating = true;
            }
        }

    }

    public void NewTarget()
    {
        target.transform.position = grid.GetRandomWalkablePosition();
        
    }

    void GoToTarget()
    {
        kusUcumuMesafe = Vector3.Distance(transform.position, target.transform.position);
        if (kusUcumuMesafe <= 40f)
        {
            target.transform.position = grid.GetRandomWalkablePosition();
            bot.transform.position = grid.GetRandomWalkablePosition();

        }
        if (grid.path1 != null && grid.path1.Count > 0 && driveable)
        {

            Vector3 hedefNokta = grid.path1[0].WorldPosition;  // Ýlk path noktasý 
            LookToTarget(hedefNokta);

            //     Debug.Log(Vector3.Distance(bot.transform.position, target.position));  // hedefle kus ucumu mesafe olcer 

            GidilcekYer(hedefNokta);  // Hedef noktayý Player'a gönder

        }
    }

    void FindPath(Vector3 startPoz, Vector3 targetPoz)
    {
        Node startNode = grid.NodeFromWorldPoint(startPoz);
        Node targetNode = grid.NodeFromWorldPoint(targetPoz);

        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (currentNode.fCost > openSet[i].fCost || currentNode.fCost == openSet[i].fCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                // Kavþak kontrolü
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

                // Right'tan direkt Left'e veya Left'ten direkt Right'a geçiþi engelle
                if (currentNode.right && neighbour.left && !neighbour.kavsak)
                {
                    continue;
                }
                if (currentNode.left && neighbour.right && !neighbour.kavsak)
                {
                    continue;
                }

                // Hareket maliyetini hesapla ve komþuyu ekle
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        // Yol bulunamadýysa hata mesajý
        Debug.LogWarning("Path not found!");
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        grid.path1 = path;
    }



    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstx = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dsty = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstx > dsty)
            return 14 * dsty + 10 * dstx;
        return 14 * dstx + 10 * (dsty - dstx);
    }

}