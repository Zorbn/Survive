namespace Game.Scripts
{
    public interface IHealth
    {
        public const int DefaultMaxHealth = 100;

        public void TakeDamage(int damage);
    }
}