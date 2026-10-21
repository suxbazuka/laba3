using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EnemyEditor
{
    public class CEnemyTemplateList
    {
        private List<CEnemyTemplate> enemies;

        public CEnemyTemplateList()
        {
            enemies = new List<CEnemyTemplate>();
        }

        public void AddEnemy(string name, string iconName, int baseLife,
            double lifeModifier, int baseGold, double goldModifier, double spawnChance)
        {
            enemies.Add(new CEnemyTemplate(name, iconName, baseLife, lifeModifier,
                baseGold, goldModifier, spawnChance));
        }

        public bool UpdateEnemy(string oldName, string newName, string iconName, int baseLife,
            double lifeModifier, int baseGold, double goldModifier, double spawnChance)
        {
            CEnemyTemplate enemy = GetEnemyByName(oldName);
            if (enemy == null) return false;

            enemy.Update(newName, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance);
            return true;
        }

        public CEnemyTemplate GetEnemyByName(string name)
        {
            return enemies.FirstOrDefault(en => en.Name == name);
        }

        public CEnemyTemplate GetEnemyByIndex(int id)
        {
            if (id < 0 || id >= enemies.Count)
                return null;
            return enemies[id];
        }

        public void DeleteEnemyByName(string name)
        {
            CEnemyTemplate enemy = GetEnemyByName(name);
            if (enemy != null)
                enemies.Remove(enemy);
        }

        public void DeleteEnemyByIndex(int id)
        {
            if (id >= 0 && id < enemies.Count)
                enemies.RemoveAt(id);
        }

        public List<string> GetListOfEnemyNames()
        {
            return enemies.Select(en => en.Name).ToList();
        }

        public void SaveToJson(string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(enemies, options);
            File.WriteAllText(path, jsonString);
        }

        public void LoadFromJson(string path)
        {
            enemies.Clear();
            string jsonFromFile = File.ReadAllText(path);
            JsonDocument doc = JsonDocument.Parse(jsonFromFile);

            foreach (JsonElement element in doc.RootElement.EnumerateArray())
            {
                string name = element.GetProperty("Name").GetString();
                string iconName = element.GetProperty("IconName").GetString();
                int baseLife = element.GetProperty("BaseLife").GetInt32();
                double lifeModifier = element.GetProperty("LifeModifier").GetDouble();
                int baseGold = element.GetProperty("BaseGold").GetInt32();
                double goldModifier = element.GetProperty("GoldModifier").GetDouble();
                double spawnChance = element.GetProperty("SpawnChance").GetDouble();

                AddEnemy(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance);
            }
        }
    }
}