using System.Linq;
using JW.Grid.GOAP;
using JW.Grid.GOAP.Goals;
using UnityEditor;

[CustomEditor(typeof(GOAPPlanner))]
public class PlannerEditor : Editor
{
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
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Current Action: {goap.currentAction.GetType().Name}");
        }

        if (goap.goals != null && goap.goals.Length > 0)
        {
            EditorGUILayout.Space(10);
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
            EditorGUILayout.LabelField("Available Goals:");
            foreach (GoalBase runableGoal in goap.goals.Where(g => g.CanRun()))
            {
                EditorGUILayout.LabelField($"  Goal Name: {runableGoal.GetType().Name}");
                EditorGUILayout.LabelField($"  Goal Priority: {runableGoal.CalculatePriority()}");
            }
        }

        // TODO: Show the actions and their priorities, goals, etc.
    }
}