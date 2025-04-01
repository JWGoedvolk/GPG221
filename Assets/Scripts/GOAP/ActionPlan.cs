using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JW.Grid.GOAP
{
    public interface IGOAPPlanner
    {
        ActionPlan Plan(GOAP goapAgent, HashSet<Goal> goals, Goal mostRecentGoal = null);
    }

    public class GOAPPlanner : IGOAPPlanner
    {
        public ActionPlan Plan(GOAP goapAgent, HashSet<Goal> goals, Goal mostRecentGoal = null)
        {
             List<Goal> orderedGoals = goals
                 .Where(goal => goal.DesiredEffects.Any(prereq => !prereq.Evaluate())) // Get only the goal where any prerequisite has not been met yet
                 .OrderByDescending(goal => goal == mostRecentGoal ? goal.Priority - 0.01f : goal.Priority) // Order the goal in descending order of importance
                 .ToList();

             // Go through each goal and add the actions needed to complete them
             foreach (var goal in orderedGoals)
             {
                 Node goalNode = new Node(null, null, goal.DesiredEffects, 0);

                 // Recursively look for a path of actions to take
                 if (FindPath(goalNode, goapAgent.Actions))
                 { 
                     if (goalNode.IsChildDead) continue; // If the goal's children are already dead, then it has been satisfied already
                     
                     Stack<Action> actionStack = new Stack<Action>();
                     while (goalNode.Children.Count > 0)
                     {
                         var cheapestChild = goalNode.Children.OrderBy(child => child.Cost).First();
                         goalNode = cheapestChild;
                         actionStack.Push(cheapestChild.Action);
                     }
                     
                     return new ActionPlan(goal, actionStack, goalNode.Cost);
                 }
             }
             
             Debug.LogError("No path found for given goal node");
             return null;
        }

        bool FindPath(Node parent, HashSet<Action> actions)
        {
            foreach (var action in actions)
            {
                var requiredEffects = parent.RequiredEffects;
                requiredEffects.RemoveWhere(prereq => prereq.Evaluate()); // Remove effects that are already met as no actions need to be taken to perform them

                if (requiredEffects.Count == 0)
                {
                    return true; // All required effects are met so return succesful 
                }

                // If this action contains will give a desired effect then we should add it to the plann
                if (action.Effects.Any(requiredEffects.Contains))
                {
                    var newRequiredEffects = new HashSet<Prereq>(requiredEffects); // Set up the new hash table of required effects
                    newRequiredEffects.ExceptWith(action.Effects); // Remove ones we have already satisfied
                    newRequiredEffects.UnionWith(action.Prereqs); // Add this action's requirements to the list 
                    
                    // Create the new set of actions to be performed
                    var newAvailableActions = new HashSet<Action>(actions);
                    newAvailableActions.Remove(action);

                    // Create a node for this action
                    var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);

                    // Recursively search it for a path
                    if (FindPath(newNode, newAvailableActions))
                    {
                        // Add the node to the parent's children and remove any satisfied prereqs
                        parent.Children.Add(newNode);
                        newRequiredEffects.ExceptWith(newNode.Action.Prereqs);
                    }

                    // If there are no more actions to account for, we have reached the end
                    if (newRequiredEffects.Count == 0)
                    {
                        return true;
                    }
                }
            }
            return false; // If we make it here, then no path was found
        }
    }

    public class Node
    {
        public Node Parent { get; }
        public Action Action { get; }
        public HashSet<Prereq> RequiredEffects { get; }
        public List<Node> Children { get; }
        public float Cost { get; }
        
        public bool IsChildDead => Children.Count == 0 && Action == null; // A child node is considered dead if it does not have children and has no action to perform

        public Node(Node parent, Action action, HashSet<Prereq> effects, float cost)
        {
            Parent = parent;
            Action = action;
            RequiredEffects = new HashSet<Prereq>(effects);
            Children = new List<Node>();
            Cost = cost;
        }
    }
    
    public class ActionPlan
    {
        public Goal Goal { get; }
        public Stack<Action> Actions { get; }
        public float TotalCost { get; set; }

        public ActionPlan(Goal goal, Stack<Action> actions, float totalCost)
        {
            Goal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }
        
        
    }
}