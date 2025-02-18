using UnityEngine;

public class Grid : MonoBehaviour
{
    Node[] grid;

    [SerializeField] public int gridCountX;
    [SerializeField] public int gridCountY;
    [SerializeField] public int cellSizeX;
    [SerializeField] public int cellSizeY;
    [SerializeField] GameObject nodePrefab;
    int totalNodes;

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

                Vector3Int gridPosition = new Vector3Int(x, 0, y);
                Vector3 worldPosition = new Vector3(x * cellSizeX - cellSizeX/2, 0, y * cellSizeY - cellSizeY/2);
                grid[i] = new Node(worldPosition, gridPosition, true);
                var node = Instantiate(nodePrefab, worldPosition, nodePrefab.transform.rotation);
                node.transform.localScale = new Vector3(cellSizeX, 1, cellSizeY);
#if ASTAR_DEBUG
                grid[i].NodeGO = node;
#endif
                grid[i].GCost = 0;
                grid[i].HCost = 0;

            }
        }
    }

    public Node GetNode(Vector3 worldPosition)
    {
        Vector3Int gridPosition = new Vector3Int((int)(worldPosition.x / cellSizeX), 0, (int)(worldPosition.y / cellSizeY));
        int i = gridPosition.x + gridPosition.z * gridCountX;
        return grid[i];
    }

    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector3Int((int)(worldPosition.x / cellSizeX), 0, (int)(worldPosition.y / cellSizeY));
    }

    public Vector3Int GridToWorlPosition(Vector3Int gridPosition)
    {
        return new Vector3Int((int)(gridPosition.x * cellSizeX), 0, (int)(gridPosition.y * cellSizeY));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
