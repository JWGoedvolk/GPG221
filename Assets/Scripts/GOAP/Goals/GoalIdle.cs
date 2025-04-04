using UnityEngine;

namespace JW.Grid.GOAP.Goals
{
    public class GoalIdle : GoalBase
    {
        [SerializeField] private int priority = 10;

        public override void OnGoalActivated()
        {
            isGoalActivated = true;
        }

        public override void OnGoalDeactivated()
        {
            isGoalActivated = false;
        }

        public override int CalculatePriority()
        {
            return priority;
        }

        public override bool CanRun()
        {
            GoalCanRun = true;
            return GoalCanRun;
        }

        public override void OnGoalTick()
        {
        }
    }
}