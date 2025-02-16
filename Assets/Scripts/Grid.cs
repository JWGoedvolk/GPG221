using UnityEngine;

public class Grid : MonoBehaviour
{
    Node[] grid;

    [SerializeField] int gridCountX;
    [SerializeField] int gridCountY;
    [SerializeField] int cellSizeX;
    [SerializeField] int cellSizeY;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
