using TMPro;
using UnityEngine;

public class AI : MonoBehaviour
{
    public delegate void OnGoalReached();
    public static OnGoalReached OnGoalReachedEvent;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float distanceThreshold = 0.1f;
    public bool IsRunning;
    public bool isAtGoal;
    private int nodeIndex = -1;
    private bool pathIsFound;

    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private float stamina;
    [SerializeField] private float staminaPerNode = 0.2f;

    public AStar astar { get; private set; }
    public float Stamina
    {
        get
        {
            return stamina;
        }
        set
        {
            stamina -= value;
            staminaText.text = stamina.ToString();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        astar = FindFirstObjectByType<AStar>();
        astar.PathFound += OnPathFound; // Subscribe to the delegate so we can run the AI as soon as we find a path
        astar.restart += OnRestart; // Subscribe to delegate for restarting
    }

    // Update is called once per frame
    private void Update()
    {
        if (!pathIsFound)
        {
            return;
        }

        if (IsRunning)
        {
            Vector3 nodePosition = astar.finalPath[nodeIndex].WorldPosition;
            nodePosition.y = 1f;
            float distanceToGoal = Vector3.Distance(nodePosition, transform.position); // Distance to next node
            if (distanceToGoal <= distanceThreshold) // If we get within a certain range of the node, we go to the next one if possible
            {
                if (nodeIndex < astar.finalPath.Count - 1)
                {
                    nodeIndex++;
                    Stamina -= staminaPerNode;
                }
                else
                {
                    GoalReached();
                }
            }

            Vector3 direction = (astar.finalPath[nodeIndex].WorldPosition - transform.position).normalized; // Direction to the next node in the list
            transform.position += direction * (speed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        }
    }

    private void OnPathFound()
    {
        pathIsFound = true;
        transform.position = astar.finalPath[0].WorldPosition;
        nodeIndex = 0;
        IsRunning = true;
    }

    private void GoalReached()
    {
        isAtGoal = true;
        IsRunning = false;
    }

    private void OnRestart()
    {
        pathIsFound = false;
        IsRunning = false;
    }

    public void PickWanderLocation(int radius)
    {
        if (astar.CalculatingPath && !astar.pathWasFound && !astar.ShouldRun) return; // Only allow a wandering location to be picked if we are not still calculating a path

        // Please tell me there's another way that doesn't feel absolutely scuffed XD
        PickLocationStart:
        Vector3 location = transform.position;
        location.x += Random.Range(-radius, radius);
        location.y = 0;
        location.z += Random.Range(-radius, radius);
        int result = astar.NewGoalDestination(location);

        switch (result)
        {
            case -2:
                Debug.LogWarning("Path is being calculated");
                goto PickLocationEnd;

            case -1:
                Debug.LogWarning("AI is already moving");
                goto PickLocationEnd;

            case 0:
                Debug.LogWarning("Node was out of bounds or null");
                goto PickLocationStart;

            case 1:
                Debug.LogWarning("Location picked Successfully");
                goto PickLocationEnd;
        }

        PickLocationEnd:
        Debug.Log("Good node picked");
    }
}