using System.Collections.Generic;
using UnityEngine;

public class Greedy : MonoBehaviour
{
    Grid grid;
    Player player;
    public Transform seeker, target;
    public bool driveable = true;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        player = FindObjectOfType<Player>();  // Find the Player object
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
        GoToTarget();
    }

    void GoToTarget()
    {
        if (grid.path1 != null && grid.path1.Count > 0 && driveable)
        {
            Vector3 hedefNokta = grid.path1[0].WorldPosition;  // First node in path
            player.LookToTarget(hedefNokta);
            player.GidilcekYer(hedefNokta);  // Send target position to Player
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

            // Find the node with the lowest heuristic (hCost)
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // If we reached the target, retrace the path
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            // Check each neighbor of the current node
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                // Ensure proper movement rules apply when not at a junction
                if (!currentNode.kavsak && !neighbour.kavsak)
                {
                    // Direction control
                    if (currentNode.gridY < neighbour.gridY && !currentNode.right) // Upward movement (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX < neighbour.gridX && !currentNode.right) // Rightward movement (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridY > neighbour.gridY && !currentNode.left) // Downward movement (left == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX > neighbour.gridX && !currentNode.left) // Leftward movement (left == true)
                    {
                        continue;
                    }
                }

                // Ensure no direct switching between right and left lanes outside junctions
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

                // Allow flexibility when at a junction
                if (currentNode.kavsak)
                {
                    // Junction-specific movement logic
                    if (currentNode.right && neighbour.left)
                    {
                        continue;
                    }
                    if (currentNode.left && neighbour.right)
                    {
                        continue;
                    }
                }

                // Calculate heuristic cost for the neighbor
                neighbour.hCost = GetDistance(neighbour, targetNode);

                // If the neighbor is not in the open set, add it
                if (!openSet.Contains(neighbour))
                {
                    neighbour.parent = currentNode;  // Set parent to retrace path later
                    openSet.Add(neighbour);
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
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.path1 = path;  // Set the computed path to grid
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return 10 * (dstX + dstY);  // Manhattan distance (suitable for grid-based paths)
    }
}
