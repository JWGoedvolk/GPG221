using UnityEngine;

namespace JW.Grid
{
    public class AStarGrid : MonoBehaviour
    {
        Node[] grid;

        public int gridCountX;
        public int gridCountY;
        public int cellSizeX;
        public int cellSizeY;
        int totalNodes;
        [SerializeField] GameObject nodePrefab;
        [SerializeField] LayerMask obstacleLayer;

        public GameObject NodePrefab { get { return nodePrefab; } }
        public LayerMask ObstacleLayer { get { return obstacleLayer; } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            totalNodes = gridCountX * gridCountY;

            grid = new Node[totalNodes];

            for (int y = 0; y < gridCountY; y++)
            {
                for (int x = 0; x < gridCountX; x++)
                {
                    int i = y * gridCountX + x;

                    // Calculate positions for world and grid space
                    Vector3 halfPoint = new Vector3((float)cellSizeX/2f, .5f, (float)cellSizeY/2f);
                    Vector3 worldPosition = new Vector3(x * cellSizeX + halfPoint.x, -1, y * cellSizeY + halfPoint.z);
                    Vector3Int gridPosition = new Vector3Int(x, 0, y);

                    // Check and set node is walkable or not
                    bool isWalkable = !Physics.CheckBox(worldPosition, halfPoint, nodePrefab.transform.localRotation, obstacleLayer);

                    grid[i] = new Node(worldPosition, gridPosition, isWalkable);
#if ASTAR_DEBUG
                    var node = Instantiate(nodePrefab, worldPosition, nodePrefab.transform.rotation);
                    node.transform.localScale = new Vector3(cellSizeX, 1, cellSizeY);
                    node.transform.parent = transform; // This makes the spawned node object a child of the grid object, helping the hierarchy stay clean
                    grid[i].NodeGO = node;
#endif
                    grid[i].GCost = 0;
                    grid[i].HCost = 0;
                }
            }

        }

        public Node GetNode(Vector3 worldPosition)
        {
            Vector3Int gridPosition = new Vector3Int((int)(worldPosition.x / cellSizeX), 0, (int)(worldPosition.z / cellSizeY));
            int i = gridPosition.x + gridPosition.z * gridCountX;
            if (i < 0 || i >= totalNodes)
            {
                Debug.LogError("Attempted to get node outside the grid");
                return null;
            }
            return grid[i];
        }

        public Vector3Int WorldToGridPosition(Vector3 worldPosition)
        {
            return new Vector3Int((int)(worldPosition.x / cellSizeX), 0, (int)(worldPosition.z / cellSizeY));
        }

        public Vector3Int GridToWorlPosition(Vector3Int gridPosition)
        {
            return new Vector3Int((int)(gridPosition.x * cellSizeX), 0, (int)(gridPosition.z * cellSizeY));
        }

#if ASTAR_DEBUG
        public void RestartGrid()
        {
            for (int y= 0; y < gridCountY; y++)
            {
                for (int x= 0; x < gridCountX; x++)
                {
                    int i = y * gridCountX + x;

                    grid[i].NodeGO.GetComponent<Renderer>().material.color = Color.black;
                }
            }
        }
#endif
    }
}
