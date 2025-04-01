using System;
using UnityEngine;

namespace JW.Grid.GOAP
{
    [System.Serializable]
    public class Prereq
    {
        public  string Name { get; } // This is only a getter as we don't want other scripts to be able to somehow change the prereq name
        Func<bool> condition = () => false; // This does not hold the actual value but rather whether the condition is met is evaluated
        Func<Vector3> location = () => Vector3.zero; // This will be the location linked to this prereq
        
        public Vector3 Location => location(); // This allows other scripts to actualy use the location for this prereq
        public bool Evaluate() => condition(); // This will actualy tell the thing checking the conditions whether it is met or not
        
        public Prereq(string name)
        {
            Name = name;
        }

        public class Builder
        {
            readonly Prereq prereq;

            public Builder(string name)
            {
                prereq = new Prereq(name);
            }

            public Builder WithCondition(Func<bool> condition)
            {
                prereq.condition = condition;
                return this;
            }

            public Builder WithLocation(Func<Vector3> location)
            {
                prereq.location = location;
                return this;
            }

            public Prereq Build()
            {
                return prereq;
            }
        }
    }
}