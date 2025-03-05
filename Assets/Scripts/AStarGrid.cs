using UnityEngine;

namespace JW.Grid
{
    public class AStarGrid : MonoBehaviour
    {
        Node[] grid;

        [SerializeField] public int gridCountX;
        [SerializeField] public int gridCountY;
        [SerializeField] public int cellSizeX;
        [SerializeField] public int cellSizeY;
        [SerializeField] GameObject nodePrefab;
        [SerializeField] LayerMask obstacleLayer;
        int totalNodes;

#if ASTAR_DEBUG
        [SerializeField] Color unWalkable = Color.gray;
#endif

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
                    grid[i].NodeGO = node;

                    if (!isWalkable)
                    {
                        grid[i].NodeGO.GetComponent<Renderer>().material.color = unWalkable;
                    }
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
        private void OnDrawGizmosSelected()
        {

            for (int y = 0; y < gridCountY; y++)
            {
                for (int x = 0; x < gridCountX; x++)
                {
                    // Calculate position of the node
                    Vector3 halfPoint = new Vector3((float)cellSizeX / 2f, 0, (float)cellSizeY / 2f);
                    Vector3 worldPosition = new Vector3(x * cellSizeX + halfPoint.x, 0, y * cellSizeY + halfPoint.z);

                    // Draw in a wire cube for the node
                    Gizmos.DrawWireCube(worldPosition, new Vector3(cellSizeX, 1f, cellSizeY));
                }
            }
        }
#endif
    } 
}
