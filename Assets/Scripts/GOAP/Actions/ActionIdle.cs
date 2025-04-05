using System;
using System.Collections.Generic;
using JW.Grid.GOAP.Goals;
using UnityEngine;

namespace JW.Grid.GOAP.Actions
{
    public class ActionIdle : ActionBase
    {
        private List<Type> supportedGoals = new List<Type>() { typeof(GoalIdle) };
        
        public override List<Type> GetSupportedGoals()
        {
            return supportedGoals;
        }
    }
}