using System;
using System.Collections.Generic;

namespace Clicker
{
    public class CSphereField
    {
        private readonly List<CCollectable> _spheres = new List<CCollectable>();
        private readonly CSphereFactory _factory;
        private readonly Random _random;
        private readonly double _spawnInterval;
        private readonly double _lifetime;
        private double _spawnTimer;

        public CSphereField(CSphereFactory factory, Random random, double spawnInterval, double lifetime)
        {
            _factory = factory;
            _random = random;
            _spawnInterval = spawnInterval;
            _lifetime = lifetime;
        }

        public IReadOnlyList<CCollectable> Spheres { get { return _spheres; } }

        public event Action<CCollectable> SphereSpawned;
        public event Action<CCollectable> SphereRemoved;

        public void Update(double dt)
        {
            _spawnTimer += dt;
            while (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer -= _spawnInterval;
                Spawn();
            }

            for (int i = _spheres.Count - 1; i >= 0; i--)
            {
                _spheres[i].Update(dt);
                if (!_spheres[i].IsAlive)
                    RemoveAt(i);
            }
        }

        public bool TryCollect(CCollectable sphere, CClickerGame game)
        {
            int index = _spheres.IndexOf(sphere);
            if (index < 0 || !sphere.Click(game)) return false;

            RemoveAt(index);
            return true;
        }

        private void Spawn()
        {
            double x = 0.1 + _random.NextDouble() * 0.8;
            double y = 0.1 + _random.NextDouble() * 0.8;

            CCollectable sphere = _factory.CreateRandom(_random, x, y, _lifetime);
            _spheres.Add(sphere);
            if (SphereSpawned != null) SphereSpawned(sphere);
        }

        private void RemoveAt(int index)
        {
            CCollectable sphere = _spheres[index];
            _spheres.RemoveAt(index);
            if (SphereRemoved != null) SphereRemoved(sphere);
        }
    }
}
