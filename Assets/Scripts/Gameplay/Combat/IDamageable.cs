public interface IDamageable
{
    void TakeDamage(int amount);
    bool IsAlive { get; }
}
