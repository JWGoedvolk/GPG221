using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

[System.Serializable]
public class Node : IComparable
{
    [SerializeField] public Node Parent { get; set; }
    [SerializeField] public Vector3 WorldPosition { get; set; }
    [SerializeField] public Vector3Int GridPosition { get; set; }
    public bool IsWalkable { get; private set; }
    public bool IsVisited;
    public int version = 0;

#if ASTAR_DEBUG
    private GameObject nodeGO;
    public Image Background;
    public TMP_Text gCostText;
    public TMP_Text hCostText;
    public TMP_Text fCostText;
    public TMP_Text versionText;

    Color gColor = new Color(.5f, 0, 0);
    Color hColor = new Color(0, 1, 0);
    Color fColor = new Color(0, 0, 1);
    Color vColor = new Color(1, 1, 1);

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
            
            versionText = nodeGO.transform.GetChild(0).GetChild(3).GetComponent<TMP_Text>();
            versionText.color = vColor;
            
            Background = nodeGO.transform.GetChild(1).GetChild(0).GetComponent<Image>();
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
            versionText.text = version.ToString();
            
            if (IsVisited)
            {
                Background.color = Color.cyan;
            }
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
            versionText.text = version.ToString();
            
            if (IsVisited)
            {
                Background.color = Color.cyan;
            }
#endif
        }
    }

    public int FCost
    {
        get
        {
#if ASTAR_DEBUG
            versionText.text = version.ToString();
#endif
            return gCost + hCost;
        }
    }

    public Node(Vector3 worldPosition, Vector3Int gridPosition, bool isWalkable)
    {
        WorldPosition = worldPosition;
        GridPosition = gridPosition;
        IsWalkable = isWalkable;
        version = 0;
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
