using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    Grid grid;

    List<Node> openList = new();
    List<Node> closeList = new();
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 endPosition;

    Node startNode;
    Node goalNode;
    Node currentNode;

    void Start()
    {
        grid = GetComponent<Grid>();

        startNode = grid.GetNode(startPosition);
        goalNode = grid.GetNode(endPosition);

#if ASTAR_DEBUG
        startNode.NodeGO.GetComponent<Renderer>().material.color = Color.blue;
        goalNode.NodeGO.GetComponent<Renderer>().material.color = Color.white;
#endif

        openList.Add(startNode);

        openList.Sort();
        currentNode = openList[0];

        openList.Remove(currentNode);
        closeList.Add(currentNode);

        if (currentNode == goalNode)
        {
            return;
        }

        // Find neighbours
        List<Node> neighbours = new List<Node>();
        // Right neighbour
        Vector3Int rightNodeGridPosition = currentNode.GridPosition + new Vector3Int(1, 0, 0);
        if (rightNodeGridPosition.x < grid.gridCountX)
        {
            Node rightNode = grid.GetNode(grid.GridToWorlPosition(currentNode.GridPosition + new Vector3Int(1, 0, 0)));
            neighbours.Add(rightNode);
        }

        // Left neighbour
        Vector3Int leftNodeGridPosition = currentNode.GridPosition + new Vector3Int(-1, 0, 0);
        if (leftNodeGridPosition.x >= 0)
        {
            Node leftNode = grid.GetNode(grid.GridToWorlPosition(currentNode.GridPosition + new Vector3Int(-1, 0, 0)));
            neighbours.Add(leftNode);
        }

        // Up neighbour
        Vector3Int upNodeGridPosition = currentNode.GridPosition + new Vector3Int(0, 0, 1);
        if (upNodeGridPosition.z < grid.gridCountY)
        {
            Node upNode = grid.GetNode(grid.GridToWorlPosition(currentNode.GridPosition + new Vector3Int(0, 0, 1)));
            neighbours.Add(upNode);
        }

        // Down neighbour
        Vector3Int downNodeGridPosition = currentNode.GridPosition + new Vector3Int(0, 0, -1);
        if (downNodeGridPosition.z >= 0)
        {
            Node downNode = grid.GetNode(grid.GridToWorlPosition(currentNode.GridPosition + new Vector3Int(0, 0, -1)));
            neighbours.Add(downNode);
        }
#if ASTAR_DEBUG
        for (int i = 0; i < neighbours.Count; i++)
        {
            neighbours[i].NodeGO.GetComponent<Renderer>().material.color = Color.red;
        }
#endif
    }

    void Update()
    {
        
    }
}
