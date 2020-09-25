public abstract class PlayerState
{
    public abstract void Execute();

    public virtual void Enter() { }
    public virtual void Exit() { }
}
