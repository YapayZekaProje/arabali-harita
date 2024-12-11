using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool Walkable;
    public Vector3 WorldPosition;
    public Node parent;

    public int gridX, gridY;
    public int gCost, hCost;

    public bool right;
    public bool left;
    public bool kavsak;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }

    }


    public Node(bool walkable, Vector3 worldPosition, int _gridX, int _gridY)
    {
        gridX = _gridX;
        gridY = _gridY;
        Walkable = walkable;
        WorldPosition = worldPosition;
    }



}