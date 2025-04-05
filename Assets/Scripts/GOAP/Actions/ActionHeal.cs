using System;
using System.Collections.Generic;
using JW.Grid.GOAP.Goals;
using UnityEngine;

namespace JW.Grid.GOAP.Actions
{
    public class ActionHeal : ActionBase
    {
        private List<Type> supportedGoals = new List<Type>() { typeof(GoalHeal) };
        
        [SerializeField] private float healAmount;
        [SerializeField] private Transform healTransform;

        public override List<Type> GetSupportedGoals()
        {
            return supportedGoals;
        }
        
        public override void OnActivated()
        {
            Agent.MoveTo(healTransform.position);
        }
    }
}