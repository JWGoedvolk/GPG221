using System;
using System.Collections.Generic;
using JW.Grid.GOAP.Goals;
using UnityEngine;

namespace JW.Grid.GOAP.Actions
{
    public class ActionWander : ActionBase
    {
        [Header("Wander Action")]
        [SerializeField] private int wanderRadius = 5;
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
            Agent.PickWanderLocation(wanderRadius);
        }

        public override void OnTick(float dt)
        {
            if (Agent.isAtGoal) // If we reach the goal
            {
                OnActivated(); // Then start again
            }
        }
    }
}