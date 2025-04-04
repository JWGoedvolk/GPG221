using UnityEngine;

namespace JW.Grid.GOAP.Goals
{
    public class GoalWander : GoalBase
    {
        [SerializeField] private int minPriority;
        [SerializeField] private int maxPriority = 20;
        [SerializeField] private int startingPriority = 10;

        [SerializeField] private float priorityBuildRate = 1f;
        [SerializeField] private float priorityDecayRate = .5f;
        [SerializeField] private float currentPriority;

        public override void Awake()
        {
            base.Awake();
            currentPriority = startingPriority;
        }

        public override void OnGoalActivated()
        {   
            currentPriority = maxPriority;
            isGoalActivated = true;
        }

        public override void OnGoalDeactivated()
        {
            currentPriority = minPriority;
            isGoalActivated = false;
        }

        public override void OnGoalTick()
        {
            if (!isGoalActivated)
            {
                currentPriority += priorityBuildRate;
            }
            else
            {
                currentPriority -= priorityDecayRate;
            }

            currentPriority = Mathf.Clamp(currentPriority, 0, maxPriority);
        }

        public override int CalculatePriority()
        {
            return Mathf.FloorToInt(currentPriority);
        }

        public override bool CanRun()
        {
            // TODO: Change the logic to be so the AI can wander if it is not currently moving and stats stuff
            bool canRun = (!Agent.IsRunning && !Agent.astar.pathWasFound && !Agent.astar.CalculatingPath) || isGoalActivated;
            GoalCanRun = canRun;
            return canRun;
        }
    }
}