using System.Linq;
using JW.Grid.GOAP.Actions;
using JW.Grid.GOAP.Goals;
using UnityEngine;

namespace JW.Grid.GOAP
{
    public class GOAPPlanner : MonoBehaviour
    {
        public GoalBase currentGoal;
        public ActionBase currentAction;

        public ActionBase[] actions;
        public GoalBase[] goals;
        [SerializeField] private float currentTime;

        [SerializeField] private float planInterval = 5f;

        private void Awake()
        {
            goals = GetComponents<GoalBase>();
            actions = GetComponents<ActionBase>();
        }

        private void FixedUpdate()
        {
            currentTime += Time.fixedDeltaTime;
            if (currentTime >= planInterval)
            {
                currentTime = 0;

                // Get the best goal and its actions to achieve it
                GoalBase bestGoal = null;
                ActionBase bestAction = null;

                // Updates all this AI's goals he has access to
                foreach (GoalBase goal in goals)
                {
                    goal.OnGoalTick();

                    // Skip goals that can't run
                    if (!goal.CanRun())
                    {
                        continue;
                    }

                    // If this goal is worse than the current best, then skip it
                    if (!(bestGoal == null || goal.CalculatePriority() > bestGoal.CalculatePriority()))
                    {
                        continue;
                    }

                    if (bestGoal == null)
                    {
                        bestGoal = goal;
                    }

                    // Go through all our actions
                    ActionBase candidateAction = null;
                    foreach (ActionBase action in actions)
                    {
                        if (!action.GetSupportedGoals().Contains(goal.GetType()))
                        {
                            continue;
                        }

                        // Check if the candidate action is better than the current one
                        if (candidateAction == null || action.GetCost() < candidateAction.GetCost())
                        {
                            candidateAction = action;
                        }
                    }

                    // Update the best goal and action
                    if (candidateAction != null)
                    {
                        bestGoal = goal;
                        bestAction = candidateAction;
                    }
                }

                // If we don't have a goal and we found one
                if (currentGoal == null)
                {
                    // Set our goal and action
                    currentGoal = bestGoal;
                    currentAction = bestAction;

                    // Activate them if they are not null
                    if (currentGoal != null)
                    {
                        currentGoal.OnGoalActivated();
                    }
                    if (currentAction != null)
                    {
                        currentAction.OnActivated();
                    }
                }
                else if (currentGoal == bestGoal) // If the goal did not change
                {
                    if (currentAction != bestAction) // If the action changed
                    {
                        currentAction.OnDeactivated(); // Deactivate

                        currentAction = bestAction; // Set the new action

                        if (currentAction != null) currentAction.OnActivated(); // And activate if there is an action
                    }
                }
                else if (currentGoal != bestGoal) // If the goal did change
                {
                    // Deactivate our current goal and action
                    currentGoal.OnGoalDeactivated();
                    currentAction.OnDeactivated();

                    // Set the new ones
                    currentGoal = bestGoal;
                    currentAction = bestAction;

                    // Activate them if we have them
                    if (currentGoal != null)
                    {
                        currentGoal.OnGoalActivated();
                    }
                    if (currentAction != null)
                    {
                        currentAction.OnActivated();
                    }
                }
            }
            
            // Update the current action if we have one
            if (currentAction != null)
            {
                currentAction.OnTick(Time.fixedDeltaTime);
            }
        }
    }
}