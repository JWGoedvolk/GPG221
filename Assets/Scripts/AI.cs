using UnityEngine;

public class AI : MonoBehaviour
{
    public delegate void OnGoalReached();
    public static OnGoalReached OnGoalReachedEvent;
    
    public AStar astar {get; private set;}
    bool pathIsFound = false;
    int nodeIndex = -1;

    [SerializeField] float speed = 5f;
    bool isRunning = false;
    bool isAtGoal = false;
    
    public bool IsIdle => !isRunning;
    public bool IsAtGoal => isAtGoal;

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
        isRunning = true;
    }

    void GoalReached()
    {
        isAtGoal = true;
        isRunning = false;
    }

    void OnRestart()
    {
        pathIsFound = false;
        isRunning = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pathIsFound)
        {
            return;
        }

        if (isRunning)
        {
            Vector3 nodePosition = astar.finalPath[nodeIndex].WorldPosition;
            nodePosition.y = 1f;
            float distanceToGoal = Vector3.Distance(nodePosition, transform.position); // Distance to next node
            if (distanceToGoal <= 0.5f) // If we get within a certain range of the node, we go to the next one if possible
            {
                if (nodeIndex < astar.finalPath.Count - 1)
                {
                    nodeIndex++;
                }
                else
                {
                    GoalReached();
                }
            }

            Vector3 direction = (astar.finalPath[nodeIndex].WorldPosition - transform.position).normalized; // Direction to the next node in the list
            transform.position += direction * speed * Time.deltaTime; 
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        }
    }
}
