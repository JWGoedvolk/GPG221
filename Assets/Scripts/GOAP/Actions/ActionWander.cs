using System;
using System.Collections.Generic;
using JW.Grid.GOAP.Goals;
using UnityEngine;

namespace JW.Grid.GOAP.Actions
{
    public class ActionWander : ActionBase
    {
        [SerializeField] private int wanderRadius = 5;
        private bool isGoalActivated;
        private List<Type> supportedGoals = new List<Type>() { typeof(GoalWander) };

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
        }

        public override List<Type> GetSupportedGoals()
        {
            return supportedGoals;
        }

        public override void OnActivated()
        {
            if (!isGoalActivated) // Only activate it if it isn't already
            {
                Agent.PickWanderLocation(wanderRadius);
                LinkedGoal.isGoalActivated = true;
                isGoalActivated = true;
            }
        }

        public override void OnDeactivated()
        {
            LinkedGoal.isGoalActivated = false;
            isGoalActivated = false;
        }

        public override void OnTick(float dt)
        {
            if (Agent.isAtGoal && !Agent.astar.CalculatingPath)
            {
                Agent.PickWanderLocation(wanderRadius);
            }
        }
    }
}