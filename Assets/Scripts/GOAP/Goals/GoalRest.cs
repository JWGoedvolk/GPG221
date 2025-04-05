using UnityEngine;

namespace JW.Grid.GOAP.Goals
{
    public class GoalRest : GoalBase
    {
        [Header("Priorities")]
        [SerializeField] private float restPriority;
        [SerializeField] private float currentPriority;
        [SerializeField] private float urgentPriority;
        [SerializeField] private float contentPriority;

        [Header("Stats")] 
        [SerializeField] private float urgent;
        [SerializeField] private float content;


        public override void OnGoalActivated()
        {   
            currentPriority = urgentPriority;
        }

        public override void OnGoalTick()
        {
            if (Agent.stamina <= urgent) // Low stamina
            {
                currentPriority = urgentPriority;
            }
            else if (Agent.stamina >= content) // we have enough stamina to not worry anymore
            {
                currentPriority = contentPriority;
            }
            else if (Agent.stamina >= urgent && Agent.stamina <= content) // We are in a sweet spot
            {
                currentPriority = restPriority;
            }
        }

        public override int CalculatePriority()
        {
            return Mathf.FloorToInt(currentPriority);
        }

        public override bool CanRun()
        {
            // TODO: Change the logic to be so the AI can wander if it is not currently moving and stats stuff
            GoalCanRun = Agent.stamina <= urgent;
            return GoalCanRun;
        }
    }
}