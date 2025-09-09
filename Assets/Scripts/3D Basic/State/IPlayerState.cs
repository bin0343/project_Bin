public interface IPlayerState
{
    void Enter(Player_Action player);

    void Execute(Player_Action player);

    void Exit(Player_Action player);
}
