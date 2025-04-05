using System.Collections.Generic;
using System.Linq;
using JW.Grid.GOAP;
using JW.Grid.GOAP.Goals;
using UnityEditor;

[CustomEditor(typeof(GOAPPlanner))]
public class PlannerEditor : Editor
{
    bool ShowGoals = false;
    bool ShowAvailableGoals = false;
    public override void OnInspectorGUI()
    {
        GOAPPlanner goap = (GOAPPlanner)target;

        base.OnInspectorGUI();

        // Show the current goal
        if (goap.currentGoal != null)
        {
            EditorGUILayout.LabelField($"Current Goal: {goap.currentGoal.GetType().Name}");
            EditorGUILayout.LabelField($"Goal Priority: {goap.currentGoal.CalculatePriority()}");
        }

        // Show the current actions
        if (goap.currentAction != null)
        {
            EditorGUILayout.LabelField($"Current Action: {goap.currentAction.GetType().Name}");
            EditorGUILayout.Space(10);
        }

        if (goap.goals != null && goap.goals.Length > 0)
        {
            ShowGoals = EditorGUILayout.Foldout(ShowGoals, "All Goals:");
            if (ShowGoals)
            {
                foreach (GoalBase goal in goap.goals)
                {
                    string goalName = goal.GetType().Name;
                    EditorGUILayout.LabelField($"Goal Name: {goalName}");
                    EditorGUILayout.LabelField($"  Goal Priority: {goal.CalculatePriority()}");
                    EditorGUILayout.LabelField($"  Goal Can Run: {goal.CanRun()}");
                    EditorGUILayout.LabelField($"  Goal Activated: {goal.isGoalActivated}");
                    EditorGUILayout.LabelField($"  Goal Complete: {goal.GoalCompleted}");
                }
                EditorGUILayout.Space(10);
            }
            
            ShowAvailableGoals = EditorGUILayout.Foldout(ShowAvailableGoals, "Available Goals:");
            if (ShowAvailableGoals)
            {
                foreach (GoalBase runableGoal in goap.goals.Where(g => g.CanRun()))
                {
                    EditorGUILayout.LabelField($"  Goal Name: {runableGoal.GetType().Name}");
                    EditorGUILayout.LabelField($"  Goal Priority: {runableGoal.CalculatePriority()}");
                }
            }
        }

#if ASTAR_DEBUG
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Goals History:");
        if (goap.GoalsHistory != null && goap.GoalsHistory.Count > 0)
        {
            foreach (var goal in goap.GoalsHistory)
            {
                EditorGUILayout.LabelField($"  Goal Name: {goal.GetType().Name}");
            }
        }
#endif

        // TODO: Show the actions and their priorities, goals, etc.
    }
}