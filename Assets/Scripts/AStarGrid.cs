using System;
using UnityEngine;

namespace JW.Grid
{
    public class AStarGrid : MonoBehaviour
    {
        private Node[] grid;

        public int gridCountX;
        public int gridCountY;
        public int cellSizeX;
        public int cellSizeY;
        public float HalfExtentHeight;
        private int totalNodes
        {
            get
            {
                return gridCountX * gridCountY;
            }
        }
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private LayerMask obstacleLayer;

        public GameObject NodePrefab { get { return nodePrefab; } }
        public LayerMask ObstacleLayer { get { return obstacleLayer; } }

#if ASTAR_DEBUG
        public bool isGridSpawned;
#endif

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            SpawnGrid();
        }

        public void SpawnGrid()
        {
            grid = new Node[totalNodes];

            for (int y = 0; y < gridCountY; y++)
            {
                for (int x = 0; x < gridCountX; x++)
                {
                    int i = y * gridCountX + x;

                    // Calculate positions for world and grid space
                    Vector3 halfPoint = new Vector3(cellSizeX / 2f, HalfExtentHeight / 2f, cellSizeY / 2f);
                    Vector3 worldPosition = new Vector3(x * cellSizeX + halfPoint.x, 0, y * cellSizeY + halfPoint.z);
                    Vector3Int gridPosition = new Vector3Int(x, 0, y);

                    // Check and set node is walkable or not
                    bool isWalkable = !Physics.CheckBox(worldPosition, halfPoint, nodePrefab.transform.localRotation, obstacleLayer);

                    grid[i] = new Node(worldPosition, gridPosition, isWalkable);
#if ASTAR_DEBUG
                    GameObject node = Instantiate(nodePrefab, worldPosition, nodePrefab.transform.rotation);
                    node.transform.localScale = new Vector3(cellSizeX, 1, cellSizeY);
                    node.transform.parent = transform; // This makes the spawned node object a child of the grid object, helping the hierarchy stay clean
                    grid[i].NodeGO = node;
                    grid[i].Background.color = isWalkable ? Color.black : Color.red;
#endif
                    grid[i].GCost = 0;
                    grid[i].HCost = 0;
                }
            }
#if ASTAR_DEBUG
            isGridSpawned = true;
#endif
        }
#if ASTAR_DEBUG
        public void RemoveGrid()
        {
            for (int i = grid.Length - 1; i >= 0; i--)
            {
                GameObject node = grid[i].NodeGO;
                DestroyImmediate(node); // Destroy the debug game object in the scene
            }
            grid = Array.Empty<Node>(); // Clear the grid completely
            isGridSpawned = false;
        }
#endif

        public Node GetNode(Vector3 worldPosition)
        {
            Vector3Int gridPosition = new Vector3Int((int)(worldPosition.x / cellSizeX), 0, (int)(worldPosition.z / cellSizeY));

            if (gridPosition.x < 0 || gridPosition.x >= gridCountX || gridPosition.z < 0 || gridPosition.z >= gridCountY)
            {
                Debug.LogWarning("Attempted to get node outside of grid bounds");
                return null;
            }

            int i = gridPosition.x + gridPosition.z * gridCountX;
            if (i < 0 || i >= totalNodes)
            {
                Debug.LogWarning("Attempted to get node outside the grid");
                return null;
            }
            return grid[i];
        }

        public Vector3Int WorldToGridPosition(Vector3 worldPosition)
        {
            return new Vector3Int((int)(worldPosition.x / cellSizeX), 0, (int)(worldPosition.z / cellSizeY));
        }

        public Vector3 GridToWorlPosition(Vector3Int gridPosition)
        {
            return new Vector3(gridPosition.x * cellSizeX, 0, gridPosition.z * cellSizeY);
        }

#if !ASTAR_DEBUG
        private void OnDrawGizmos()
        {
            Vector3 gridBounds = new Vector3(cellSizeX * gridCountX, 1f, cellSizeY * gridCountY);
            Vector3 offset = new Vector3(gridBounds.x / 2, 0, gridBounds.z / 2);
            Gizmos.DrawWireCube(transform.position + offset, gridBounds);
        }
#endif

#if ASTAR_DEBUG
        public void RestartGrid()
        {
            for (int y = 0; y < gridCountY; y++)
            {
                for (int x = 0; x < gridCountX; x++)
                {
                    int i = y * gridCountX + x;

                    if (grid[i].IsWalkable) grid[i].Background.color = Color.black;
                    else grid[i].Background.color = Color.red;
                }
            }
        }
#endif
    }
}