using UnityEngine;

namespace JW.Grid.GOAP.Goals
{
    public class GoalIdle : GoalBase
    {
        [SerializeField] private int priority = 10;

        public override int CalculatePriority()
        {
            return priority;
        }

        public override bool CanRun()
        {
            GoalCanRun = true;
            return GoalCanRun;
        }
    }
}