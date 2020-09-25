public class StateMachine
{
    public State currentState;

    public void SetState(State newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        
        if (currentState != null)
            currentState.Enter();
    }
}
