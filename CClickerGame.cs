using System;
using System.Collections.Generic;
using EnemyEditor;

namespace Clicker
{
    public class CClickerGame
    {
        private const double SphereSpawnInterval = 2.5;
        private const double SphereLifetime = 4.0;

        private readonly EnemySpawner _spawner;
        private readonly List<CEffect> _effects = new List<CEffect>();

        public CPlayer Player { get; private set; }
        public CEnemy CurrentEnemy { get; private set; }
        public int Stage { get; private set; }

        public CSphereField Spheres { get; private set; }

        public IReadOnlyList<CEffect> Effects { get { return _effects; } }

        public CClickerGame(CEnemyTemplateList templates)
        {
            _spawner = new EnemySpawner(templates);
            Player = new CPlayer();
            Spheres = new CSphereField(new CClickerSphereFactory(), new Random(),
                SphereSpawnInterval, SphereLifetime);
            Stage = 0;
            SpawnNext();
        }

        public bool Click()
        {
            if (!Player.AttackCooldown.IsReady) return false;

            BigNumber damage = GetEffectiveDamage();
            Player.AttackCooldown.Start(GetEffectiveCooldown());

            if (CurrentEnemy.TakeDamage(damage))
            {
                Player.AddGold(CurrentEnemy.Reward);
                Stage++;
                SpawnNext();
            }
            return true;
        }

        public bool TryCollectSphere(CCollectable sphere)
        {
            return Spheres.TryCollect(sphere, this);
        }

        public void Tick(double dt)
        {
            Player.AttackCooldown.Update(dt);

            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].Update(dt);
                if (_effects[i].IsExpired)
                    _effects.RemoveAt(i);
            }

            Spheres.Update(dt);
        }

        public void AddEffect(CEffect effect)
        {
            _effects.Add(effect);
        }

        public bool TryUpgradeDamage()
        {
            return Player.TryUpgrade();
        }

        public bool TryUpgradeCooldown()
        {
            return Player.TryUpgradeCooldown();
        }

        private BigNumber GetEffectiveDamage()
        {
            BigNumber damage = Player.Damage;
            foreach (CEffect effect in _effects)
                damage = effect.ModifyDamage(damage);
            return damage;
        }

        private double GetEffectiveCooldown()
        {
            double seconds = Player.AttackCooldownBase;
            foreach (CEffect effect in _effects)
                seconds = effect.ModifyCooldown(seconds);
            return seconds;
        }

        private void SpawnNext()
        {
            CurrentEnemy = new CEnemy(_spawner.NextTemplate(), Stage);
        }
    }
}
