using System;
using System.Collections.Generic;
using UnityEngine;

namespace JW.Grid.GOAP
{
    public class PrereqFactory
    {
        readonly GOAP goap;
        readonly Dictionary<string, Prereq> prereqs;

        public PrereqFactory(GOAP goap, Dictionary<string, Prereq> prereqs)
        {
            this.goap = goap;
            this.prereqs = prereqs;
        }

        #region Prereq Adders
        public void AddPrereq(string name, Func<bool> condition)
        {
            prereqs.Add(name, new Prereq.Builder(name)
                .WithCondition(condition)
                .Build());
        }

        public void AddSensorPrereq(string name, Sensor sensor)
        {
            prereqs.Add(name,  new Prereq.Builder(name)
                .WithCondition(() => sensor.IsTargetInRange)
                .WithLocation(() => sensor.TargetPosition)
                .Build());
        }

        public void AddLocationPrereq(string name, Transform location, float range)
        {
            AddLocationPrereq(name, location.position, range);
        }
        
        public void AddLocationPrereq(string name, Vector3 location, float range)
        {
            prereqs.Add(name, new Prereq.Builder(name)
                .WithCondition(() => InRangeOf(location, range)) // We need to be within a certain range to have the prereq
                .WithLocation(() => location) // The location in question
                .Build());
        }
        #endregion
        
        /// <summary>
        /// Whether the GOAP object is in within the given range
        /// </summary>
        /// <param name="pos">position to check for</param>
        /// <param name="range">the range to check out to</param>
        /// <returns></returns>
        bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(goap.transform.position, pos) <= range; 
    }
}