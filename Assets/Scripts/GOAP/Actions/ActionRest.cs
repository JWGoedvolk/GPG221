using System;
using System.Collections.Generic;
using JW.Grid.GOAP.Goals;
using UnityEngine;

namespace JW.Grid.GOAP.Actions
{
    public class ActionRest : ActionBase
    {
        private List<Type> supportedGoals = new List<Type>() { typeof(GoalRest) };
        
        [SerializeField] private float restAmount;
        [SerializeField] private Transform restTransform;
        
        public override List<Type> GetSupportedGoals()
        {
            return supportedGoals;
        }
        
        public override void OnActivated()
        {
            Agent.MoveTo(restTransform.position);
        }
    }
}