using UnityEngine;
using TMPro;
using System;

public class Node : IComparable
{
    public Node Parent { get; set; }
    public Vector3 WorldPosition { get; set; }
    public Vector3Int GridPosition { get; set; }
    public bool IsWalkable { get; private set; }

#if ASTAR_DEBUG 
    private GameObject nodeGO;
    public TMP_Text gCostText;
    public TMP_Text hCostText;
    public TMP_Text fCostText;

    public GameObject NodeGO
    {
        get { return nodeGO; }
        set 
        { 
            nodeGO = value; 
            gCostText = nodeGO.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            hCostText = nodeGO.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
            fCostText = nodeGO.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>();

        }
    }
#endif

    int gCost;
    public int GCost
    {
        get { return gCost; }
        set
        {
#if ASTAR_DEBUG
            gCostText.text = value.ToString();
            fCostText.text = FCost.ToString();
#endif
            gCost = value;
        }
    }

    int hCost;
    public int HCost
    {
        get { return hCost; }
        set
        {
#if ASTAR_DEBUG
            hCostText.text = value.ToString();
            fCostText.text = FCost.ToString();
#endif
            hCost = value;
        }
    }

    public int FCost
    {
        get { return gCost + hCost; }
    }

    public Node(Vector3 worldPosition, Vector3Int gridPosition, bool isWalkable)
    {
        WorldPosition = worldPosition;
        GridPosition = gridPosition;
        IsWalkable = isWalkable;
    }

    public int CompareTo(object obj)
    {
        Node node = (Node)obj;

        if (node.FCost > FCost)
        {
            return -1;
        }
        else if (node.FCost < FCost)
        {
            return 1;
        }

        return 0;
    }
}
