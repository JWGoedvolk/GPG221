using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JW.Grid;
using AStarGrid = JW.Grid.AStarGrid;

public class AStar : MonoBehaviour
{
    [Header("States")] public bool pathWasFound = false;
    public delegate void onPathFound();
    public onPathFound PathFound;
    public delegate void onRestart();
    public onRestart restart;
    bool isAIMoving = false;

#if ASTAR_DEBUG
    bool autoRun = false;
#endif

    [SerializeField] AStarGrid grid;

    [Header("Positions")] 
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 endPosition;
    #if ASTAR_DEBUG
    [SerializeField]
    #endif
    List<Node> openList = new();
    public List<Node> finalPath = new();
    int version = 0;

    Node startNode;
    Node goalNode;
    Node currentNode;

    [SerializeField] LayerMask rayLayerMask;
    [SerializeField] float rayDistance = 100f;
    [SerializeField] GameObject hitMarker;

#if ASTAR_DEBUG
    [Header("Debug")] [SerializeField] Color neighbourColor;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    [SerializeField] Color currentColor;
    [SerializeField] Color unwalkableColor;
    [SerializeField] bool isGridDrawn = false;
    [SerializeField] private int numCycles = 0;
    [SerializeField] private float halfExtentHeight = 1f;
#endif

    public Node GoalNode => goalNode;

    void Awake()
    {
        grid = GetComponent<AStarGrid>();

        startNode = grid.GetNode(startPosition);
        goalNode = grid.GetNode(endPosition);

#if ASTAR_DEBUG
        startNode.Background.color = startColor;
        goalNode.Background.color = endColor;
#endif

        openList.Add(startNode); // Add the starting node to the open list so we can start finding a path
    }

    void Start()
    {
        AI.OnGoalReachedEvent += AIReachedGoal;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Camera cam = Camera.main;
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, rayDistance, rayLayerMask))
            {
                Vector3 hitPoint = hit.point;
                hitPoint.y = 0f;
                NewGoalDestination(hitPoint); // Set the hit point's node as the new goal
            }
        }

#if ASTAR_DEBUG
        if (Input.GetKeyDown(KeyCode.V))
        {
            autoRun = !autoRun;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && openList.Count > 0 || (autoRun && openList.Count > 0)) // Step by step looping through the allgorythm
#else
        while (openList.Count > 0 && !pathWasFound) // continue with the algorythm as long as we have nodes to visit
#endif
        {
            if (pathWasFound) // we exit the loop if the path is already found
            {
                return;
            }

            if (!goalNode.IsWalkable)
            {
                return;
            }

            /* I Know this one works but only the fCost with no regard for h cost
            */
            openList.Sort(); // Sort the list to have the first element be the Node with the lowest F cost, which is the shortest path so far

            // Remove these if this doesn't seem to work
            //openList.OrderBy(n => n.FCost).ThenByDescending(n => n.HCost);
            int lowestFCost = openList[0].FCost;
            var lowestFCostNodes = openList.Where(n => n.FCost == lowestFCost).ToList();
            lowestFCostNodes = lowestFCostNodes.OrderBy(n => n.HCost).ToList();

#if ASTAR_DEBUG
            foreach (Node n in lowestFCostNodes)
            {
                Debug.Log($"{n.GridPosition}: F = {n.FCost} | H = {n.HCost}");
            }
#endif
            
            currentNode = lowestFCostNodes[0];
            openList.Remove(currentNode); // We have looked at this node so take it out of the list of cells to visit
            currentNode.IsVisited = true;
#if ASTAR_DEBUG
            currentNode.Background.color = Color.cyan;
            currentNode.Background.color = currentColor;
#endif

            if ((currentNode.GridPosition.x == goalNode.GridPosition.x && currentNode.GridPosition.z == goalNode.GridPosition.z) || pathWasFound) // are we at the end or has a path been found
            {
                startNode.Parent = null;
                FindFinalPath(goalNode); // Trace the path
                finalPath.Reverse(); // Reverse the found path as it traces back from the goal to the start. we want it from the start to the goal
                PathFound(); // Dellegaate to let the AI know it can start moving
                pathWasFound = true;
                isAIMoving = true;
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
                neighbours[i].Background.color = neighbourColor;
#endif
                if (neighbours[i].GridPosition == goalNode.GridPosition)
                {
                    goalNode.Parent = currentNode;
                    startNode.Parent = null;
                    FindFinalPath(goalNode); // Trace the path
                    finalPath.Reverse(); // Reverse the found path as it traces back from the goal to the start. we want it from the start to the goal
                    PathFound(); // Dellegaate to let the AI know it can start moving
                    pathWasFound = true;
                    return;
                }

                // Go to next neighbour imediatly if this one is has been visited before on this run
                if (neighbours[i].version == version && neighbours[i].IsVisited)
                {
                    continue;
                }

                // Update neighbour as needed
                int newMovementPath =
                    CalculateDistance(neighbours[i].GridPosition, currentNode.GridPosition) +
                    currentNode.GCost; // The G cost of teh neighbour from the current tile. recallculated in case it was discovered more effeciently
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
            }

#if ASTAR_DEBUG
            numCycles++;
            Vector3 currentPosition = currentNode.WorldPosition;
            currentPosition.y = 1f;
            if (currentNode != null && hitMarker != null) hitMarker.transform.position = currentPosition;
#endif

            if (pathWasFound)
            {
                return;
            }
        }
    }

    private void AIReachedGoal()
    {
        isAIMoving = false;
    }
    
    public bool NewGoalDestination(Vector3 goalPosition)
    {
        if (isAIMoving) return false; // Only get a new position if the AI is at the goal
        
        goalPosition.y = 0f; // Sanity check just in case

        Node newGoalNode = grid.GetNode(goalPosition); // Attempt to get the new node position
        if (newGoalNode != null) // If the node exists
        {
            if (newGoalNode.IsWalkable && newGoalNode != goalNode) // And it is walkable and not the current goal
            {
                // Then set it as the new goal and return success
                endPosition = goalPosition;
                RestartAlgorythm();
                return true;
            }
            else // Otherwise it is not valid so return failure
            {
                return false;
            }
        }
        else
        {
            return false;
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
            node.IsVisited = false;
#if ASTAR_DEBUG
            node.Background.color = Color.white;
#endif
            if (node.Parent != null &&
                !finalPath.Contains(node.Parent) &&
                node.Parent.version == version) // Recursively call until either the parent is no longer there (goal shouldn't have a parent OR we loop back into the list
            {
                FindFinalPath(node.Parent);
            }
        }
    }

    void RestartAlgorythm()
    {
#if ASTAR_DEBUG
        print("Restarting algorythm");
        numCycles = 0;
#endif
        restart(); // Signal things that need to that we have restarted
        version++;
        openList = new(); // Clear the list of nodes to visit
        finalPath = new(); // We don't know the new shortest path
        pathWasFound = false; // Continue the loop again

#if ASTAR_DEBUG
        grid.RestartGrid();
#endif

        // Resetting start and goal nodes
        startNode.Parent = null;
        startPosition = goalNode.WorldPosition; // Start where you are now
        startNode = grid.GetNode(startPosition);
        goalNode = grid.GetNode(endPosition);
        goalNode.Parent = null;
        startNode.version = version;
        goalNode.version = version;

#if ASTAR_DEBUG
        startNode.Background.color = startColor;
        goalNode.Background.color = endColor;
#endif

        // Re-add the starting node to the list of nodes to visit. this is to restart the loop again
        openList.Add(startNode);
    }

#if ASTAR_DEBUG
    private void OnPathFound()
    {
        Debug.Log($"path found in {numCycles} cycles");
    }
    private void OnDrawGizmos()
    {
        // We only draw the debug grid if we have the grid referenced, in edit mode, and it has not been drawn yet
        if (grid == null) return;
        if (Application.isPlaying) return;
        if (!isGridDrawn) return;

        Vector3Int startGridPosition = grid.WorldToGridPosition(startPosition);
        Vector3Int endGridPosition = grid.WorldToGridPosition(endPosition);

        for (int y = 0; y < grid.gridCountY; y++)
        {
            for (int x = 0; x < grid.gridCountX; x++)
            {
                int i = y * grid.gridCountX + x;

                // Calculate positions for world and grid space
                Vector3 halfPoint = new Vector3((float)grid.cellSizeX / 2f, halfExtentHeight, (float)grid.cellSizeY / 2f);
                Vector3 worldPosition = new Vector3(x * grid.cellSizeX + halfPoint.x, -1, y * grid.cellSizeY + halfPoint.z);

                bool isWalkable = !Physics.CheckBox(worldPosition, halfPoint, grid.NodePrefab.transform.localRotation, grid.ObstacleLayer);

                Gizmos.color = isWalkable ? Color.white : Color.red;

                if (x == startGridPosition.x && y == startGridPosition.z)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(worldPosition, new Vector3(grid.cellSizeX, grid.cellSizeY, grid.cellSizeY));
                }
                else if (x == endGridPosition.x && y == endGridPosition.z)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(worldPosition, new Vector3(grid.cellSizeX, grid.cellSizeY, grid.cellSizeY));
                }
                else
                {
                    Gizmos.DrawWireCube(worldPosition, new Vector3(grid.cellSizeX - 0.1f, 1, grid.cellSizeY - 0.1f));
                }

            }
        }
    }
#endif
}