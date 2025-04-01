using System.Collections.Generic;

namespace JW.Grid.GOAP
{
    [System.Serializable]
    public class Action
    {
        public string Name { get; }
        public int Cost { get; private set; }

        public HashSet<Prereq> Prereqs { get; } = new();
        public HashSet<Prereq> Effects { get; } = new();

        #region !ActionStrategyImplementation

        IActionStrategy strategy;
        public bool IsCompleted => strategy.isCompleted;
        public void Start() => strategy.Start();
        public void Update(float dt)
        {
            if (strategy.canPerform) // Only perform the strategy if it is possible
            {
                strategy.Update(dt);
            }
            
            if (!strategy.isCompleted) return; // Go back and keep running the action as it is not done yet

            foreach (Prereq effect in Effects)
            {
                effect.Evaluate(); // Recheck the effects we expect to change from this actions completing
            }
        }
        public void Stop() => strategy.Stop(); 

        #endregion

        public Action(string name)
        {
            Name = name;
        }

        #region Builder

        public class Builder
        {
            private readonly Action action;

            public Builder(string name)
            {
                action = new Action(name)
                {
                    Cost = 1
                };
            }

            public Builder WithCost(int cost)
            {
                action.Cost = cost;
                return this;
            }

            public Builder WithStrategy(IActionStrategy strategy)
            {
                action.strategy = strategy;
                return this;
            }

            /// <summary>
            /// Adds the given prerequisite to the list of this action's prereqs
            /// </summary>
            /// <param name="prereq">the Prereq to add</param>
            /// <returns></returns>
            public Builder WithPrereq(Prereq prereq)
            {
                action.Prereqs.Add(prereq);
                return this;
            }

            /// <summary>
            /// Adds the given Prereq as the expected effects this action will have when completed
            /// </summary>
            /// <param name="prereq">Prereq corresponding to the desired effect</param>
            /// <returns></returns>
            public Builder WithEffect(Prereq prereq)
            {
                action.Effects.Add(prereq);
                return this;
            }

            public Action Build()
            {
                return action;
            }
        }

        #endregion
    }
}