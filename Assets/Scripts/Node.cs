using UnityEngine;
using TMPro;
using System;

public class Node : IComparable
{
    public Node Parent { get; set; }
    public Vector3 WorldPosition { get; set; }
    public Vector3Int GridPosition { get; set; }
    public bool IsWalkable { get; private set; }
    public bool IsVisited;

#if ASTAR_DEBUG
    private GameObject nodeGO;
    public TMP_Text gCostText;
    public TMP_Text hCostText;
    public TMP_Text fCostText;

    [SerializeField] Color gColor = new Color(1, 0, 0);
    [SerializeField] Color hColor = new Color(0, 1, 0);
    [SerializeField] Color fColor = new Color(0, 0, 1);

    public GameObject NodeGO
    {
        get { return nodeGO; }
        set 
        { 
            nodeGO = value; 
            
            gCostText = nodeGO.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            gCostText.color = gColor;

            hCostText = nodeGO.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
            hCostText.color = hColor;

            fCostText = nodeGO.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>();
            fCostText.color = fColor;
        }
    }
#endif

    int gCost;
    public int GCost
    {
        get { return gCost; }
        set
        {
            gCost = value;
#if ASTAR_DEBUG
            gCostText.text = value.ToString();
            fCostText.text = FCost.ToString();
#endif
        }
    }

    int hCost;
    public int HCost
    {
        get { return hCost; }
        set
        {
            hCost = value;
#if ASTAR_DEBUG
            hCostText.text = value.ToString();
            fCostText.text = FCost.ToString();
#endif
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
