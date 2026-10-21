using EnemyEditor;

namespace Clicker
{
    public class CClickerGame
    {
        private readonly EnemySpawner _spawner;

        public CPlayer Player { get; private set; }
        public CEnemy CurrentEnemy { get; private set; }
        public int Stage { get; private set; } 

        public CClickerGame(CEnemyTemplateList templates)
        {
            _spawner = new EnemySpawner(templates);
            Player = new CPlayer();
            Stage = 0;
            SpawnNext();
        }

        public void Click()
        {
            if (CurrentEnemy.TakeDamage(Player.Damage))
            {
                Player.AddGold(CurrentEnemy.Reward);
                Stage++;
                SpawnNext();
            }
        }

        public bool TryUpgradeDamage()
        {
            return Player.TryUpgrade();
        }

        private void SpawnNext()
        {
            CurrentEnemy = new CEnemy(_spawner.NextTemplate(), Stage);
        }
    }
}
