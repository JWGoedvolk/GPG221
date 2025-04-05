using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AStarGrid = JW.Grid.AStarGrid;

[RequireComponent(typeof(AI))]
public class AStar : MonoBehaviour
{
    public delegate void onPathFound();
    public delegate void onRestart();
    public onPathFound PathFound;
    public onRestart restart;
    [Header("States")] 
    public bool pathWasFound;
    public bool ShouldRun;
    public bool CalculatingPath;

    [SerializeField] private AStarGrid grid;

    [Header("Positions")] 
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
#if ASTAR_DEBUG
    [SerializeField]
#endif
    private List<Node> openList = new List<Node>();

    public List<Node> finalPath = new List<Node>();
    [SerializeField] private AI aiAgent;

#if ASTAR_DEBUG
    [SerializeField] private bool autoRun;
    public float halfExtentHeight = 1f;
#endif
    
#if ASTAR_DEBUG
    [Header("New Destination")]
    [SerializeField] private LayerMask rayLayerMask;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private GameObject hitMarker;
    [Header("Debug")] [SerializeField] private Color neighbourColor;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private Color currentColor;
    [SerializeField] private Color unwalkableColor;
    [SerializeField] private bool isGridDrawn;
    [SerializeField] private int numCycles;
#endif

    private Node currentNode;
    private Node startNode;
    private int version;
    private bool isAIMoving
    {
        get
        {
            return aiAgent.IsRunning;
        }
    }

    private Node GoalNode { get; set; }

    private void Awake()
    {
        aiAgent = GetComponent<AI>();

        startPosition = transform.position;
        startNode = grid.GetNode(startPosition);
        GoalNode = grid.GetNode(endPosition);

#if ASTAR_DEBUG
        startNode.Background.color = startColor;
        GoalNode.Background.color = endColor;
#endif

        openList.Add(startNode); // Add the starting node to the open list so we can start finding a path
    }

    private void Update()
    {
        if (!ShouldRun) return; // Only continue if we should run
#if ASTAR_DEBUG
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

        if (Input.GetKeyDown(KeyCode.V))
        {
            autoRun = !autoRun;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && openList.Count > 0 || autoRun && openList.Count > 0) // Step by step looping through the algorithm
#else
        while (openList.Count > 0 && !pathWasFound) // continue with the algorithm as long as we have nodes to visit
#endif
        {
            if (pathWasFound) // we exit the loop if the path is already found
            {
                return;
            }

            if (!GoalNode.IsWalkable)
            {
                return;
            }

            CalculatingPath = true;

            // TODO: Check this later as I think we're sorting the list twice?!
            openList.Sort(); // Sort the list to have the first element be the Node with the lowest F cost, which is the shortest path so far
            int lowestFCost = openList[0].FCost; // So we only look at nodes with the lowest F Cost
            // Get all the nodes with the same lowest F cost into a list so we can use Linq on it
            var lowestFCostNodes = openList.Where(n => n.FCost == lowestFCost).ToList();
            lowestFCostNodes = lowestFCostNodes.OrderBy(n => n.HCost).ToList(); // Sort by lowest H cost to get the node closest to the goal node
            currentNode = lowestFCostNodes[0];
            openList.Remove(currentNode); // We have looked at this node so take it out of the list of cells to visit
            currentNode.IsVisited = true; // And mark it as visited
#if ASTAR_DEBUG
            currentNode.Background.color = Color.cyan;
            currentNode.Background.color = currentColor;
#endif

            if (currentNode.GridPosition.x == GoalNode.GridPosition.x && currentNode.GridPosition.z == GoalNode.GridPosition.z ||
                pathWasFound) // are we at the end or has a path been found
            {
                startNode.Parent = null;
                FindFinalPath(GoalNode); // Trace the path
                finalPath.Reverse(); // Reverse the found path as it traces back from the goal to the start. we want it from the start to the goal
                PathFound(); // Delegate to let the AI know it can start moving
                pathWasFound = true;
                return;
            }

            #region Finding Neighbours

            // Find neighbours
            var neighbours = new List<Node>();

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
            foreach (Node neighbour in neighbours)
            {
#if ASTAR_DEBUG
                neighbour.Background.color = neighbourColor;
#endif
                if (neighbour.GridPosition == GoalNode.GridPosition)
                {
                    GoalNode.Parent = currentNode;
                    startNode.Parent = null;
                    FindFinalPath(GoalNode); // Trace the path
                    finalPath.Reverse(); // Reverse the found path as it traces back from the goal to the start. we want it from the start to the goal
                    PathFound(); // Delegate to let the AI know it can start moving
                    pathWasFound = true;
                    return;
                }

                // Go to next neighbour immediately if this one has been visited before on this run
                if (neighbour.version == version && neighbour.IsVisited)
                {
                    continue;
                }

                // Update neighbour as needed
                int newMovementPath =
                    CalculateDistance(neighbour.GridPosition, currentNode.GridPosition) +
                    currentNode.GCost; // The G cost of teh neighbour from the current tile. recalculated in case it was discovered more efficiently
                if (neighbour.version != version) // we restarted so ignore all previously assigned value and overwrite them
                {
                    neighbour.version = version; // Put the neighbour on the same version as the current iteration

                    // Update cost values
                    neighbour.GCost = newMovementPath;
                    int hCost = CalculateDistance(neighbour.GridPosition, GoalNode.GridPosition);
                    neighbour.HCost = hCost;

                    neighbour.Parent = currentNode; // For tracing back the path

                    if (!openList.Contains(neighbour)) openList.Add(neighbour); // Only add the neighbour to the list of nodes to visit if it isn't already on the list
                }
                else // We are again on the same version, so follow normal procedures
                {
                    // If the new G cost is better or the node is not in the list of nodes to visit yet
                    if (newMovementPath < neighbour.GCost || !openList.Contains(neighbour))
                    {
                        neighbour.version = version; // Bring the node up to speed

                        // Update cost values
                        neighbour.GCost = newMovementPath;
                        int hCost = CalculateDistance(neighbour.GridPosition, GoalNode.GridPosition);
                        neighbour.HCost = hCost;

                        neighbour.Parent = currentNode; // For tracing back the path

                        // Only add it if it isn't already
                        if (!openList.Contains(neighbour)) openList.Add(neighbour);
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
                CalculatingPath = false;
            }
        }
    }

    public void TermintatePath(int nodeIndex)
    {
        currentNode = finalPath[nodeIndex];
        ShouldRun = false;
        CalculatingPath = false;
    }

    /// <summary>
    /// </summary>
    /// <param name="goalPosition"></param>
    /// <returns>-2: calculating path, -1: AI is moving, 0: node is invalid, 1: node is good</returns>
    public int NewGoalDestination(Vector3 goalPosition)
    {
        if (isAIMoving) return -1; // Only get a new position if the AI is at the goal
        if (CalculatingPath) return -2; // We can't make a new path if we are still working through this one

        goalPosition.y = 0f; // Sanity check just in case

        Node newGoalNode = grid.GetNode(goalPosition); // Attempt to get the new node position
        if (newGoalNode != null) // If the node exists
        {
            if (!newGoalNode.IsWalkable)
            {
                return 0;
            }
            if (newGoalNode.IsWalkable && newGoalNode != GoalNode || newGoalNode != currentNode) // And it is walkable and not the current goal
            {
#if ASTAR_DEBUG
                // Reset our cycles
                numCycles = 0;
#endif

                version++; // Increase version count

                // Clear our lists
                openList = new List<Node>();
                finalPath = new List<Node>();

                // Reset parents
                startNode.Parent = null;
                if (GoalNode != null) GoalNode.Parent = null;

                // Reset positions
                startPosition = transform.position;
                endPosition = goalPosition;

                // Get the new nodes
                startNode = grid.GetNode(startPosition);
                GoalNode = newGoalNode;
                GoalNode.Parent = null; // Just in case 😅
                openList.Add(startNode);

#if ASTAR_DEBUG
                grid.RestartGrid();
                startNode.Background.color = startColor;
                GoalNode.Background.color = endColor;
#endif

                pathWasFound = false; // Continue the loop again
                ShouldRun = true;
                CalculatingPath = true;

                restart();

                // Then set it as the new goal and return success
                //RestartAlgorythm();
                return 1;
            }
            // Otherwise it is not valid so return failure
            return 0;
        }
        return 0;
    }

    private int CalculateDistance(Vector3Int posA, Vector3Int posB)
    {
        return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y) + Mathf.Abs(posA.z - posB.z);
    }

    private void FindFinalPath(Node node)
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

            CalculatingPath = false;
        }
    }

    // ReSharper disable once IdentifierTypo
    private void RestartAlgorythm()
    {
#if ASTAR_DEBUG
        print("Restarting algorythm");
        numCycles = 0;
#endif
        restart(); // Signal things that need to that we have restarted
        version++;
        openList = new List<Node>(); // Clear the list of nodes to visit
        finalPath = new List<Node>(); // We don't know the new shortest path
        pathWasFound = false; // Continue the loop again
        ShouldRun = false;
        CalculatingPath = true;

        // Resetting start and goal nodes
        startNode.Parent = null;
        startPosition = GoalNode.WorldPosition; // Start where you are now
        startNode = grid.GetNode(startPosition);
        GoalNode = grid.GetNode(endPosition);
        GoalNode.Parent = null;
        startNode.version = version;
        GoalNode.version = version;

#if ASTAR_DEBUG
        grid.RestartGrid();
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
        //if (Application.isPlaying) return;
        if (!isGridDrawn) return;

        Vector3Int startGridPosition = grid.WorldToGridPosition(startPosition);
        Vector3Int endGridPosition = grid.WorldToGridPosition(endPosition);

        for (int y = 0; y < grid.gridCountY; y++)
        {
            for (int x = 0; x < grid.gridCountX; x++)
            {
                // Calculate positions for world and grid space
                Vector3 halfPoint = new Vector3(grid.cellSizeX / 2f, halfExtentHeight, grid.cellSizeY / 2f);
                Vector3 worldPosition = new Vector3(x * grid.cellSizeX + halfPoint.x, 0, y * grid.cellSizeY + halfPoint.z);

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