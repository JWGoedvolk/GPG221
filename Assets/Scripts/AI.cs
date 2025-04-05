using JW.Grid.Sensors;
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

    [Header("UI")]
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] public float stamina;
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaPerNode = 0.2f;
    [SerializeField] public float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float healthPerNode = 0.2f;

    [Header("Sensors")]
    [SerializeField] private BaseSensor staminaSensor;
    [SerializeField] private BaseSensor healthSensor;
    
    public AStar astar { get; private set; }
    
    public float MaxHealth => maxHealth;
    public float MaxStamina => maxStamina;

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
        // If in range of heal or stamina providers, increase thee stats
        if (IsInHealthRange())
        {
            health += healthPerNode * Time.deltaTime;
        }
        if (IsInStaminaRange())
        {
            stamina += staminaPerNode * Time.deltaTime;
        }
        
        // Clamp the stats so they don't go crazy
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        health = Mathf.Clamp(health, 0f, maxHealth);
        
        if (!pathIsFound) // Don't bother starting until we have a path
        { 
            return;
        }

        if (IsRunning) // If we are running to the goal
        {
            Vector3 nodePosition = astar.finalPath[nodeIndex].WorldPosition;
            nodePosition.y = 0f;
            float distanceToGoal = Vector3.Distance(nodePosition, transform.position); // Distance to next node
            if (distanceToGoal <= distanceThreshold) // If we get within a certain range of the node, we go to the next one if possible
            {
                if (nodeIndex < astar.finalPath.Count - 1) // Check if we are on the node
                {
                    nodeIndex++; // Go to next node
                    
                    // Lose stats for traveling a node
                    stamina -= staminaPerNode;
                    health -= healthPerNode;
                }
                else // No more nodes means we are at our goal
                {
                    GoalReached();
                }
            }

            // Move us towards the node
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

    public void StopAgent()
    {
        IsRunning = false;
        isAtGoal = true;
        transform.position = astar.finalPath[nodeIndex].WorldPosition; // Set our stating position to the current node
    }

    public bool IsInStaminaRange()
    {
        return staminaSensor.IsTriggered;
    }

    public bool IsInHealthRange()
    {
        return healthSensor.IsTriggered;
    }

    public void MoveTo(Vector3 destination)
    {
        if (astar.CalculatingPath && !astar.pathWasFound && !astar.ShouldRun) return; // Only allow a wandering location to be picked if we are not still calculating a path
        destination.y = 0f;

        if (IsRunning || !isAtGoal)
        {
            StopAgent(); // Stop the agent if needed
        }


        int result = astar.NewGoalDestination(destination);
        
        print(result);
    }

    public void PickWanderLocation(int radius)
    {
        if (astar.CalculatingPath && !astar.pathWasFound && !astar.ShouldRun) return; // Only allow a wandering location to be picked if we are not still calculating a path

        if (IsRunning || !isAtGoal)
        {
            StopAgent(); // Stop the agent if needed
        }
        
        // Please tell me there's another way that doesn't feel absolutely scuffed XD
        PickLocationStart:
        Vector3 location = transform.position;
        location.x += Random.Range(-radius, radius) * 2;
        location.y = 0;
        location.z += Random.Range(-radius, radius) * 2;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanceThreshold);
    }
}