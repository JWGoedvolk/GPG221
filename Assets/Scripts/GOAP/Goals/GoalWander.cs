using UnityEngine;

namespace JW.Grid.GOAP.Goals
{
    public class GoalWander : GoalBase
    {
        [SerializeField] private float WanderPriority = 30f;
        [SerializeField] private float priorityBuildRate = 1f;
        [SerializeField] private float priorityDecayRate = .5f;
        private float currentPriority = 30f;

        public override void OnGoalActivated()
        {   
            currentPriority = WanderPriority;
        }

        public override void OnGoalTick()
        {
            if (Agent.IsRunning)
            {
                currentPriority -= priorityDecayRate * Time.deltaTime;
            }
            else
            {
                currentPriority += priorityBuildRate * Time.deltaTime;
            }
        }

        public override int CalculatePriority()
        {
            return Mathf.FloorToInt(currentPriority);
        }

        public override bool CanRun()
        {
            // TODO: Change the logic to be so the AI can wander if it is not currently moving and stats stuff
            GoalCanRun = true;
            return GoalCanRun;
        }
    }
}