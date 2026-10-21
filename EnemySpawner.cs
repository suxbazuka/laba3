using System;
using System.Collections.Generic;
using EnemyEditor;

namespace Clicker
{
    /// <summary>Случайный выбор шаблона с учётом нормализованных шансов появления.</summary>
    public class EnemySpawner
    {
        private readonly List<CEnemyTemplate> _templates = new List<CEnemyTemplate>();
        private readonly List<double> _chances = new List<double>(); // сумма = 1
        private readonly Random _random = new Random();

        public EnemySpawner(CEnemyTemplateList list)
        {
            for (int i = 0; ; i++)
            {
                CEnemyTemplate t = list.GetEnemyByIndex(i);
                if (t == null) break;
                _templates.Add(t);
            }

            if (_templates.Count == 0)
                throw new ArgumentException("Список шаблонов противников пуст.");

            NormalizeChances();
        }

        public IReadOnlyList<double> NormalizedChances
        {
            get { return _chances; }
        }

        private void NormalizeChances()
        {
            double sum = 0;
            foreach (CEnemyTemplate t in _templates)
                sum += Math.Max(0, t.SpawnChance);

            foreach (CEnemyTemplate t in _templates)
            {
                if (sum > 0)
                    _chances.Add(Math.Max(0, t.SpawnChance) / sum);
                else
                    _chances.Add(1.0 / _templates.Count); // все шансы нулевые - выбираем равновероятно
            }
        }

        public CEnemyTemplate NextTemplate()
        {
            double roll = _random.NextDouble();
            double acc = 0;

            for (int i = 0; i < _templates.Count; i++)
            {
                acc += _chances[i];
                if (roll < acc) return _templates[i];
            }
            return _templates[_templates.Count - 1]; // страховка от погрешности double
        }
    }
}
