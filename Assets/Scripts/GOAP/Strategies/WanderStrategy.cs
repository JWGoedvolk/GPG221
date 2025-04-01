using UnityEngine;

namespace JW.Grid.GOAP
{
    public class WanderStrategy : IActionStrategy
    {
        // Interface Variables
        public bool canPerform => !astar.pathWasFound;
        public bool isCompleted => agent.IsAtGoal;
        
        readonly AStar astar;
        readonly float wanderRadius;
        readonly AI agent;

        public WanderStrategy(AI agent, float wanderRadius)
        {
            this.agent = agent;
            astar = this.agent.astar;
            this.wanderRadius = wanderRadius;
        }
        
        // Interface Functions
        public void Start()
        {
            Vector3 currentGoalPosition = astar.GoalNode.WorldPosition;
            Vector3 newGoalPosition = Random.insideUnitSphere * wanderRadius;

            while (!astar.NewGoalDestination(newGoalPosition))
            {
                newGoalPosition = Random.insideUnitSphere * wanderRadius;
            } 
        }
        public void Update(float dt)
        {
            
        }
        public void Stop()
        {
            
        }
    }
}