public abstract class PlayerState : State
{
    protected PlayerController player;
    protected PlayerInput input;

    public PlayerState(PlayerController player, PlayerInput input)
    {
        this.player = player;
        this.input = input;
    }
}
