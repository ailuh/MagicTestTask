using VContainer.Unity;

public class CombatSystem : ITickable
{
    private readonly PlayerController _playerController;
    
    public void Tick()
    {
    }
    
    public void ApplyDamageToMonster(Monster monster, int damage)
    {
        monster.TakeDamage(damage);
    }
}