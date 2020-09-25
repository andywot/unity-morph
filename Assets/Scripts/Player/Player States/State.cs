public abstract class State
{
    public State() { }

    public abstract void Execute();

    public virtual void Enter() { }
    public virtual void Exit() { }
}
