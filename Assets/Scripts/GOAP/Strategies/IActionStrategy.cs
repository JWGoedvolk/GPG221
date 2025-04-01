namespace JW.Grid.GOAP
{
    public interface IActionStrategy
    {
        bool canPerform { get; }
        bool isCompleted { get; }

        void Start();
        void Update(float dt);
        void Stop();
    }
}