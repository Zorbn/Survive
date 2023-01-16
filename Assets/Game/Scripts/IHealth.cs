namespace Game.Scripts
{
    public interface IHealth
    {
        public const int DefaultMaxHealth = 100;

        public bool TakeDamage(int damage);
        public int GetHealth();
    }
}