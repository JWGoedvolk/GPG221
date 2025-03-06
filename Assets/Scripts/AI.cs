using UnityEngine;

public class AI : MonoBehaviour
{
    AStar astar;
    bool pathIsFound = false;
    int nodeIndex = -1;
    [SerializeField] float speed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        astar = FindFirstObjectByType<AStar>();
        astar.PathFound += OnPathFound; // Subsribe to the delegate so we can run the AI as soon as we find a path
        astar.restart += OnRestart; // Subscribe to delegate for restarting
    }

    void OnPathFound()
    {
        pathIsFound = true;
        transform.position = astar.finalPath[0].WorldPosition;
        nodeIndex = 0;
    }

    void OnRestart()
    {
        pathIsFound = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pathIsFound)
        {
            return;
        }

        float distanceToGoal = Vector3.Distance(astar.finalPath[nodeIndex].WorldPosition, transform.position); // Distance to next node
        if (distanceToGoal <= 0.5f) // If we get within a certain range of the node, we go to the next one if possible
        {
            if (nodeIndex < astar.finalPath.Count - 1)
            {
                nodeIndex++;
            }
        }

        Vector3 direction = (astar.finalPath[nodeIndex].WorldPosition - transform.position).normalized; // Direction to the next node in the list
        transform.position += direction * speed * Time.deltaTime;
    }
}
