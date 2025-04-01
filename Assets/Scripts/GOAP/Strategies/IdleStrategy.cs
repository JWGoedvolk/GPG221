namespace JW.Grid.GOAP
{
    public class IdleStrategy : IActionStrategy
    {
        // IActionStrategy Variables
        public bool canPerform => true;
        public bool isCompleted { get; private set; }
        
        // Timer Variables
        float idleTime;

        public IdleStrategy(float idleTime)
        {
            this.idleTime = idleTime;
        }
        
        public void Start()
        {
        }
        public void Update(float dt)
        {
            idleTime -= dt;
            if (idleTime <= 0)
            {
                isCompleted = true;
            }
        }
        public void Stop()
        {
            isCompleted = true;
        }
    }
}