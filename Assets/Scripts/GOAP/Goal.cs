using System.Collections.Generic;

namespace JW.Grid.GOAP
{
    [System.Serializable]
    public class Goal
    {
        public string Name { get; }
        public float Priority { get; private set; }
        public HashSet<Prereq> DesiredEffects { get; } = new();

        public Goal(string name)
        {
            Name = name;
        }

        #region Builder
        public class Builder
        {
            readonly Goal goal;

            public Builder(string name)
            {
                goal = new Goal(name);
            }

            public Builder WithPriority(float priority)
            {
                goal.Priority = priority;
                return this;
            }

            public Builder WithDesiredEffects(Prereq desiredEffect)
            {
                goal.DesiredEffects.Add(desiredEffect);
                return this;
            }

            public Goal Build()
            {
                return goal;
            }
        }
        #endregion
    }
}