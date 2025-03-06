using System.Collections.Generic;
using UnityEngine;
using JW.Grid;
using AStarGrid = JW.Grid.AStarGrid;
using UnityEngine.Experimental.GlobalIllumination;

public class AStar : MonoBehaviour
{
    public delegate void onPathFound();
    public onPathFound PathFound;
    bool pathWasFound = false;

    public delegate void onRestart();
    public onRestart restart;

    AStarGrid grid;

    List<Node> openList = new();
    public List<Node> finalPath = new();
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 endPosition;
    int version = 0;

    Node startNode;
    Node goalNode;
    Node currentNode;
    int loopCount = 0;

#if ASTAR_DEBUG
    [SerializeField] Color neighbourColor;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    [SerializeField] Color currentColor;
    [SerializeField] float sphereSize = 0.1f;

#endif

    void Start()
    {
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
        // TODO: Re-add to debug
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartAlgorythm();
        }
#if ASTAR_DEBUG

        if (Input.GetKeyDown(KeyCode.Backspace)) // Step by step looping through the allgorythm
#else
        while (openList.Count > 0) // continue with the algorythm as long as we have nodes to visit
#endif
        {
            if (pathWasFound) // we exit the loop if the path is already found
            {
                return;
            }

            openList.Sort(); // Sort the list to have the first element be the Node with the lowest F cost, which is the shortest path so far
            foreach (Node node in openList)
            {
                print($"F: {node.FCost} | H: {node.HCost}");
            }
            currentNode = openList[0];
            openList.Remove(currentNode); // We have looked at this node so take it out of the list of cells to visit
            currentNode.IsVisited = true;

#if ASTAR_DEBUG
            currentNode.NodeGO.GetComponent<Renderer>().material.color = currentColor;
#endif

            if (currentNode == goalNode || pathWasFound) // are we at the end or has a path been found
            {
                FindFinalPath(goalNode); // Trace the path
                print("Path Found!");
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
                if (rightNode.IsWalkable) neighbours.Add(rightNode); // only add the neighbour if it is walkable
            }

            // Left neighbour
            Vector3Int leftNodeGridPosition = currentNode.GridPosition + new Vector3Int(-1, 0, 0);
            if (leftNodeGridPosition.x >= 0)
            {
                Node leftNode = grid.GetNode(grid.GridToWorlPosition(currentNode.GridPosition + new Vector3Int(-1, 0, 0)));
                if (leftNode.IsWalkable) neighbours.Add(leftNode); // only add the neighbour if it is walkable
            }

            // Up neighbour
            Vector3Int upNodeGridPosition = currentNode.GridPosition + new Vector3Int(0, 0, 1);
            if (upNodeGridPosition.z < grid.gridCountY)
            {
                Node upNode = grid.GetNode(grid.GridToWorlPosition(currentNode.GridPosition + new Vector3Int(0, 0, 1)));
                if (upNode.IsWalkable) neighbours.Add(upNode); // only add the neighbour if it is walkable
            }

            // Down neighbour
            Vector3Int downNodeGridPosition = currentNode.GridPosition + new Vector3Int(0, 0, -1);
            if (downNodeGridPosition.z >= 0)
            {
                Node downNode = grid.GetNode(grid.GridToWorlPosition(currentNode.GridPosition + new Vector3Int(0, 0, -1)));
                if (downNode.IsWalkable) neighbours.Add(downNode); // only add the neighbour if it is walkable
            } 
            #endregion

            // Going through neighbours and calculate the next shortest path
            for (int i = 0; i < neighbours.Count; i++)
            {
#if ASTAR_DEBUG
                neighbours[i].NodeGO.GetComponent<Renderer>().material.color = neighbourColor;
#endif

                // Go to next neighbour imediatly if this one is unwalkable or has been visited before
                if ((!neighbours[i].IsWalkable || neighbours[i].IsVisited) && neighbours[i].version == version) 
                {
                    continue;
                }

                // Update neighbour as needed
                int newMovementPath = CalculateDistance(neighbours[i].GridPosition, currentNode.GridPosition) + currentNode.GCost; // The G cost of teh neighbour from the current tile. recallculated in case it was discovered more effeciently
                if (neighbours[i].version != version) // we resstarted so ignore all previously assigned value and overwrite them
                {
                    neighbours[i].version = version; // Put the neighbour on the same version as the current itteration

                    // Update cost values
                    neighbours[i].GCost = newMovementPath;
                    int hCost = CalculateDistance(neighbours[i].GridPosition, goalNode.GridPosition);
                    neighbours[i].HCost = hCost;

                    neighbours[i].Parent = currentNode; // For tracing back the path

                    if (!openList.Contains(neighbours[i])) openList.Add(neighbours[i]); // Only add the neighbour to the list of nodes to visit if it isn't already on the list
                }
                else // We are again on the same version, so follow normal procedures
                {
                    // If the new G cost is better or the node is not in the list of nodes to visit yet
                    if (newMovementPath < neighbours[i].GCost || !openList.Contains(neighbours[i])) 
                    {
                        neighbours[i].version = version; // Bring the node up to speed

                        // Update cost values
                        neighbours[i].GCost = newMovementPath;
                        int hCost = CalculateDistance(neighbours[i].GridPosition, goalNode.GridPosition);
                        neighbours[i].HCost = hCost;

                        neighbours[i].Parent = currentNode; // For tracing back the path

                        // Only add it if it isn't already
                        if (!openList.Contains(neighbours[i])) openList.Add(neighbours[i]);
                    }
                }

                if (neighbours[i] == goalNode)
                {
                    Debug.LogError("Goal is a neighbour");
                    goalNode.Parent = currentNode;
                    currentNode = goalNode;
                    pathWasFound = true;
                    FindFinalPath(goalNode);
                    finalPath.Reverse();
                    PathFound();
                    break;
                }
            }

#if ASTAR_DEBUG
            //print($"Current node grid pos: {currentNode.GridPosition}");
            //print($"goal node grid pos: {goalNode.GridPosition}");
            print($"is goal in openList: {openList.Contains(goalNode)}");
#endif
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
#if ASTAR_DEBUG
            node.NodeGO.GetComponent<Renderer>().material.color = Color.white;
#endif
            if (node.Parent != null && !finalPath.Contains(node.Parent)) // Recursively call until either the parent is no longer there (goal shouldn't have a parent OR we loop back into the list
            {
                print($"{node.WorldPosition} -> {node.Parent.WorldPosition}");
                FindFinalPath(node.Parent);
            }
        }
    }

    void RestartAlgorythm()
    {
        print("Restarting algorythm");
        restart(); // Signal things that need to that we have restarted
        version++; 
        openList = new();  // Clear the list of nodes to visit
        finalPath = new(); // We don't know the new shortest path
        pathWasFound = false; // Continue the loop again

#if ASTAR_DEBUG
        grid.RestartGrid();
#endif

        // Resetting start and goal nodes
        startPosition = currentNode.WorldPosition; // Start where you are now
        startNode = grid.GetNode(startPosition);
        goalNode = grid.GetNode(endPosition);
        goalNode.Parent = null;
        startNode.version = version;
        goalNode.version = version;

        // Re-add the starting node to the list of nodes to visit. this is to restart the loop again
        openList.Add(startNode);
    }

#if ASTAR_DEBUG
    private void OnDrawGizmos()
    {
        Gizmos.color = startColor;
        Gizmos.DrawSphere(startPosition, sphereSize);

        Gizmos.color = endColor;
        Gizmos.DrawSphere(endPosition, sphereSize);

        Gizmos.color = currentColor;
        if (currentNode != null) Gizmos.DrawSphere(currentNode.WorldPosition, sphereSize);
    } 
#endif
}