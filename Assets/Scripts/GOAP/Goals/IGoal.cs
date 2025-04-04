namespace JW.Grid.GOAP.Goals
{
    public interface IGoal
    {
        public int CalculatePriority()
        {
            return -1;
        }
        public bool CanRun()
        {
            return false;
        }
        public void OnGoalTick()
        {
        }
        public void OnGoalActivated()
        {
        }
        public void OnGoalDeactivated()
        {
        }
    }
}