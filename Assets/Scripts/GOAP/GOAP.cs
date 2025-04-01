using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace JW.Grid.GOAP
{
    [RequireComponent(typeof(AI))]
    public class GOAP : MonoBehaviour
    {
        [Header("Sensors")]
        [SerializeField] private Sensor chaseSensor;
        [SerializeField] private Sensor attackSensor;

        [Header("Locations")] 
        [SerializeField] private AI agent;
        [SerializeField] private AStar aStar;
        [SerializeField] private float wanderRadius;
        [SerializeField] Transform restingLocation;
        [SerializeField] Transform foodLocation;
        [SerializeField] Transform healLocation;
        [SerializeField] Transform restLocation;
         
        [Header("Stats")]
        [SerializeField] private float health;
        [SerializeField] private float stamina;
        [SerializeField] private float hunger;
        [SerializeField] private float statChangeInterval;
        private float currentChangeTimer;
         
        GameObject target;
        Vector3 destination;
         
        // Goals 
        Goal lastGoal;
        public Goal CurrentGoal;
        public ActionPlan Plan;
        public Action CurrentAction;
        public Dictionary<string, Prereq> Prereqs;
        public HashSet<Action> Actions;
        public HashSet<Goal> Goals;
        IGOAPPlanner planner;

        private void Awake()
        {
            agent = GetComponent<AI>();

            planner = new GOAPPlanner();
        }

        private void Start()
        {
            aStar = agent.astar; // TODO: Set up excecution order to have GOAP run after AI so we can grab this reference like this. Otherwise we can run into null errors
            // TODO: Add call set up functions here
        }

        private void OnEnable()
        {
            chaseSensor.OnDetected += HandleTargetChanged;
            attackSensor.OnDetected += HandleTargetChanged;
        }

        private void OnDisable()
        {
            chaseSensor.OnDetected -= HandleTargetChanged;
            attackSensor.OnDetected -= HandleTargetChanged;
        }

        void Update()
        {
            currentChangeTimer += Time.deltaTime;
            if (currentChangeTimer >= statChangeInterval)
            {
                if (InRangeOf(healLocation.position, 2f)) // Get health if close to the health spot
                {
                    health += 20f;
                }
                else // Otherwise lose health
                {
                    health -= 5f;
                }

                if (InRangeOf(foodLocation.position, 2f)) // Get hunger back if close to the food spot
                {
                    hunger += 20f;
                }
                else // Otherwise lose hunger
                {
                    hunger -= 5f;
                }

                if (InRangeOf(restingLocation.position, 2f)) // Get stamina if close to the rest spot
                {
                    stamina += 20f;
                }
                else // Otherwise lose stamina
                {
                    stamina -= 5f;
                }
                 
                // Clamp to make sure we don't go over or under our limits
                health = Mathf.Clamp(health, 0f, 100f);
                stamina = Mathf.Clamp(stamina, 0f, 100f);
                hunger = Mathf.Clamp(hunger, 0f, 100f);
            }

            if (CurrentAction == null)
            {
                Debug.Log("Making a new plan");
                CalculatePlan();

                if (Plan != null && Plan.Actions.Count > 0)
                {
                    CurrentGoal = Plan.Goal;
                    CurrentAction = Plan.Actions.Pop();
                    CurrentAction.Start();
                }
            }

            // Call our current action's update function if we have a plan and current action
            if (Plan != null && CurrentAction != null)
            {
                CurrentAction.Update(Time.deltaTime);

                // Check if we completed the action
                if (CurrentAction.IsCompleted)
                {
                    // Stop the action and reset it
                    CurrentAction.Stop();
                    CurrentAction = null;
                    
                    // If we are finished with the plan, then set everything up for the next plan to be made
                    if (Plan.Actions.Count == 0)
                    {
                        lastGoal = CurrentGoal;
                        CurrentGoal = null;
                    }
                }
            }
        }

        void CalculatePlan()
        {
            float priority = CurrentGoal?.Priority ?? 0f; // What is our current goal's priority so we know if we need to override it. Default to 0

            HashSet<Goal> goalsToCheck = Goals;

            if (CurrentGoal != null)
            {
                goalsToCheck = new HashSet<Goal>(Goals.Where(goal => goal.Priority > priority)); // Make a new hash set with goal with a higher priority than our current one
            }

            var potentialPlan = planner.Plan(this, goalsToCheck, lastGoal);
            if (potentialPlan != null)
            {
                Plan = potentialPlan;
            }
        }

        /// <summary>
        /// Returns whether we are within a given range from a position
        /// </summary>
        /// <param name="pos">position to go to</param>
        /// <param name="range">how far we need to be</param>
        /// <returns>true if within range, false if  not</returns>
        bool InRangeOf(Vector3 pos, float range)
        {
            return Vector3.Distance(transform.position, pos) <= range;
        }

        /// <summary>
        /// This function simply sets our current action and goal to null so we have to rethink our plan
        /// </summary>
        void HandleTargetChanged()
        {
            CurrentAction = null;
            CurrentGoal = null;
        }

        private void SetupPrereqs()
        {
            Prereqs = new Dictionary<string, Prereq>();
            PrereqFactory factory = new PrereqFactory(this, Prereqs);
             
            factory.AddPrereq("Nothing", () => false);
            
            factory.AddPrereq("Idle", () => agent.IsIdle);
            factory.AddPrereq("Moving", () => !agent.IsIdle);
        }

        private void SetupActions()
        {
             Actions = new HashSet<Action>();

             Actions.Add(new Action.Builder("Relax")
                 .WithStrategy(new IdleStrategy(5f))
                 .WithPrereq(Prereqs["Nothing"])
                 .Build());

             Actions.Add(new Action.Builder("Wandering")
                 .WithStrategy(new WanderStrategy(agent, wanderRadius))
                 .WithPrereq(Prereqs["Idle"])
                 .WithEffect(Prereqs["Moving"])
                 .Build());
        }

        private void SetupGoals()
        {
             Goals = new HashSet<Goal>();
             
             // Relax: Go to idle spot to regain stamina and idle
             Goals.Add(new Goal.Builder("Relax")
                 .WithPriority(1f)
                 .WithDesiredEffects(Prereqs["Nothing"])
                 .Build());

             // Wandering: Randomly picks a location and moves there
             Goals.Add(new Goal.Builder("Wandering")
                 .WithPriority(1f)
                 .WithDesiredEffects(Prereqs["Moving"])
                 .Build());
        }
    }
}