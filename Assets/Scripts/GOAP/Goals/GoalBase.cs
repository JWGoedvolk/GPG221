using UnityEngine;

namespace JW.Grid.GOAP.Goals
{
    [RequireComponent(typeof(AI))]
    public class GoalBase : MonoBehaviour, IGoal
    {
        public bool GoalCanRun;
        public bool GoalCompleted;
        public bool isGoalActivated;

        protected AI Agent;
        // TODO: Create and add awareness system

        public virtual void Awake()
        {
            Agent = GetComponent<AI>();
        }

        public virtual int CalculatePriority()
        {
            return -1;
        }
        public virtual bool CanRun()
        {
            return false;
        }
        public virtual void OnGoalActivated()
        {
        }
        public virtual void OnGoalDeactivated()
        {
        }
        public virtual void OnGoalTick()
        {
        }

        public virtual void SetPriority(int priority)
        {
        }
    }
}