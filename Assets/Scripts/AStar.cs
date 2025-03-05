using System.Collections.Generic;
using UnityEngine;
using JW.Grid;
using AStarGrid = JW.Grid.AStarGrid;

public class AStar : MonoBehaviour
{
    // TODO: optimize by removing close list
    // TODO: optimize by checking if the neighbour is the goal node

    public delegate void onPathFound();
    public onPathFound PathFound;
    AStarGrid grid;

    List<Node> openList = new();
    List<Node> closeList = new();
    public List<Node> finalPath = new();
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 endPosition;

    Node startNode;
    Node goalNode;
    Node currentNode;

#if ASTAR_DEBUG
    [SerializeField] Color neighbourColor;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    [SerializeField] Color currentColor;
#endif
    [SerializeField] float sphereSize = 0.1f;

    void Start()
    {
        // Set up
        grid = GetComponent<AStarGrid>();

        startNode = grid.GetNode(startPosition);
        goalNode = grid.GetNode(endPosition);

#if ASTAR_DEBUG
        startNode.NodeGO.GetComponent<Renderer>().material.color = Color.blue;
        goalNode.NodeGO.GetComponent<Renderer>().material.color = Color.white;
#endif

        openList.Add(startNode); // Add the starting node to the open list so we can start finding a path
    }

    private void Update()
    {
#if ASTAR_DEBUG
        if (Input.GetKeyDown(KeyCode.Space))
#endif
        {
            openList.Sort(); // Sort the list to have the first element be the Node with the lowest F cost, which is the shortest path so far
            currentNode = openList[0];
            openList.Remove(currentNode); // We have looked at this node so take it out of the list of cells to visit
            closeList.Add(currentNode);   // and put it in the list of visited nodes

            if (currentNode == goalNode) // Have we reached the end yet?
            {
                FindFinalPath(goalNode); // Trace the path
                print("Path Found!");

                // Debug draw the nodes in the path as white
#if ASTAR_DEBUG
                for (int i = 0; i < finalPath.Count; i++)
                {
                    finalPath[i].NodeGO.GetComponent<Renderer>().material.color = Color.white;
                }
#endif

                finalPath.Reverse(); // Reverse the found path as it traces back from the goal to the start. we want it from the start to the goal
                PathFound(); // Dellegaate to let the AI know it can start moving

                return;
            }

            #region Finding Neighbours
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
            #endregion

            // Going through neighbours and calculate the next shortest path
            for (int i = 0; i < neighbours.Count; i++)
            {
#if ASTAR_DEBUG
                neighbours[i].NodeGO.GetComponent<Renderer>().material.color = Color.red;
#endif
                if (!neighbours[i].IsWalkable || closeList.Contains(neighbours[i])) // Go to next neighbour imediatly if this one is unwalkable or has been visited before
                {
                    continue;
                }

                int newMovementPath = CalculateDistance(neighbours[i].GridPosition, currentNode.GridPosition) + currentNode.GCost;
                if (newMovementPath < neighbours[i].GCost || !openList.Contains(neighbours[i])) // Update G cost if the new G cost is less or it has not been visited yet
                {
                    neighbours[i].GCost = newMovementPath;
                    int hCost = CalculateDistance(neighbours[i].GridPosition, goalNode.GridPosition);
                    neighbours[i].HCost = hCost;

                    neighbours[i].Parent = currentNode; // For tracing back the path

                    if (!openList.Contains(neighbours[i]))
                    {
                        openList.Add(neighbours[i]);
                    }
                }
            }
        }
    }

    int CalculateDistance(Vector3Int posA, Vector3Int posB)
    {
        return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y) + Mathf.Abs(posA.z - posB.z);
    }

    void FindFinalPath(Node node)
    {
        if (node != null)
        {
            finalPath.Add(node);
            if (node.Parent != null)
            {
                FindFinalPath(node.Parent);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPosition, sphereSize);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPosition, sphereSize);

        Gizmos.color = Color.magenta;
        if (currentNode != null) Gizmos.DrawSphere(currentNode.WorldPosition, sphereSize);
    }
}