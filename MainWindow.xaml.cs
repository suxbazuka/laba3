using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using EnemyEditor;
using Microsoft.Win32;

namespace Clicker
{
    public partial class MainWindow : Window
    {
        private CClickerGame _game;
        private string _iconsFolder;
        private readonly Dictionary<string, BitmapImage> _iconCache = new Dictionary<string, BitmapImage>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Шаблоны из редактора (Лаба 1)
            OpenFileDialog jsonDialog = new OpenFileDialog
            {
                Title = "Выберите файл с шаблонами противников",
                FileName = "enemies",
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json"
            };

            if (jsonDialog.ShowDialog() != true)
            {
                Close();
                return;
            }

            try
            {
                CEnemyTemplateList templates = new CEnemyTemplateList();
                templates.LoadFromJson(jsonDialog.FileName);
                _game = new CClickerGame(templates);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить шаблоны: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // 2. Папка с иконками (в JSON хранятся только имена файлов)
            OpenFolderDialog folderDialog = new OpenFolderDialog
            {
                Title = "Выберите папку с иконками противников"
            };
            if (folderDialog.ShowDialog() == true)
                _iconsFolder = folderDialog.FolderName;

            RefreshView();
        }

        private void EnemyArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_game == null) return;
            _game.Click();
            RefreshView();
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_game == null) return;
            _game.TryUpgradeDamage();
            RefreshView();
        }

        private void RefreshView()
        {
            CEnemy enemy = _game.CurrentEnemy;
            CPlayer player = _game.Player;

            EnemyNameText.Text = enemy.Name;
            HealthText.Text = "Здоровье: " + enemy.CurrentHealth + " / " + enemy.MaxHealth
                              + "\nНаграда: " + enemy.Reward;
            HealthBar.Value = enemy.HealthPercent;

            BitmapImage icon = LoadIcon(enemy.IconName);
            EnemyImage.Source = icon;
            NoIconText.Visibility = icon == null ? Visibility.Visible : Visibility.Collapsed;

            GoldText.Text = "Золото: " + player.Gold;
            DamageText.Text = "Урон за клик: " + player.Damage + " (ур. " + player.DamageLevel + ")";
            StageText.Text = "Побеждено противников: " + _game.Stage;

            UpgradeButton.Content = "Улучшить урон (цена: " + player.UpgradeCost + ")";
            UpgradeButton.IsEnabled = player.CanUpgrade;
        }

        private BitmapImage LoadIcon(string iconName)
        {
            if (string.IsNullOrEmpty(_iconsFolder) || string.IsNullOrEmpty(iconName))
                return null;

            BitmapImage cached;
            if (_iconCache.TryGetValue(iconName, out cached))
                return cached;

            BitmapImage image = LoadImageSafe(Path.Combine(_iconsFolder, iconName));
            _iconCache[iconName] = image;
            return image;
        }

        private BitmapImage LoadImageSafe(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            try
            {
                BitmapImage bitmap = new BitmapImage();
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
