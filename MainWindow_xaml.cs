using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EnemyEditor;
using Microsoft.Win32;

namespace Clicker
{
    public partial class MainWindow : Window
    {
        private CClickerGame _game;
        private string _iconsFolder;
        private readonly Dictionary<string, BitmapImage> _iconCache = new Dictionary<string, BitmapImage>();
        private readonly Dictionary<CCollectable, FrameworkElement> _sphereViews = new Dictionary<CCollectable, FrameworkElement>();
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        private DateTime _lastTick;

        public MainWindow()
        {
            InitializeComponent();
            _timer.Tick += Timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            _game.Spheres.SphereSpawned += OnSphereSpawned;
            _game.Spheres.SphereRemoved += OnSphereRemoved;

            OpenFolderDialog folderDialog = new OpenFolderDialog
            {
                Title = "Выберите папку с иконками противников"
            };
            if (folderDialog.ShowDialog() == true)
                _iconsFolder = folderDialog.FolderName;

            _lastTick = DateTime.Now;
            _timer.Start();
            RefreshView();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_game == null) return;

            DateTime now = DateTime.Now;
            double dt = (now - _lastTick).TotalSeconds;
            _lastTick = now;

            _game.Tick(dt);
            UpdateSphereViews();
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

        private void CooldownUpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_game == null) return;
            _game.TryUpgradeCooldown();
            RefreshView();
        }


        private void OnSphereSpawned(CCollectable sphere)
        {
            double size = sphere.Radius * 2;
            Grid view = new Grid
            {
                Width = size,
                Height = size,
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand
            };
            view.Children.Add(new System.Windows.Shapes.Ellipse
            {
                Fill = ToBrush(sphere.Color),
                Stroke = Brushes.White,
                StrokeThickness = 2
            });
            view.Children.Add(new TextBlock
            {
                Text = sphere.Label,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            view.MouseLeftButtonDown += (s, args) =>
            {
                _game.TryCollectSphere(sphere);
                args.Handled = true;
            };

            SphereLayer.Children.Add(view);
            _sphereViews[sphere] = view;
        }

        private void OnSphereRemoved(CCollectable sphere)
        {
            FrameworkElement view;
            if (_sphereViews.TryGetValue(sphere, out view))
            {
                SphereLayer.Children.Remove(view);
                _sphereViews.Remove(sphere);
            }
        }


        private void UpdateSphereViews()
        {
            foreach (KeyValuePair<CCollectable, FrameworkElement> pair in _sphereViews)
            {
                CCollectable sphere = pair.Key;
                FrameworkElement view = pair.Value;

                Canvas.SetLeft(view, sphere.X * SphereLayer.ActualWidth - sphere.Radius);
                Canvas.SetTop(view, sphere.Y * SphereLayer.ActualHeight - sphere.Radius);
                view.Opacity = Math.Min(1.0, sphere.TimeLeft);
            }
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
            DamageText.Text = "Урон за атаку: " + player.Damage + " (ур. " + player.DamageLevel + ")";

            CClickCooldown cooldown = player.AttackCooldown;
            CooldownText.Text = "Перезарядка атаки: "
                + (cooldown.IsReady ? "готова" : cooldown.Remaining.ToString("F1") + " с")
                + " (база " + player.AttackCooldownBase.ToString("F2") + " с)";
            EffectsText.Text = BuildEffectsText();
            StageText.Text = "Побеждено противников: " + _game.Stage;

            UpgradeButton.Content = "Улучшить урон (цена: " + player.UpgradeCost + ")";
            UpgradeButton.IsEnabled = player.CanUpgrade;

            CooldownUpgradeButton.Content = "Ускорить атаку, -10% перезарядки (цена: " + player.CooldownUpgradeCost + ")";
            CooldownUpgradeButton.IsEnabled = player.CanUpgradeCooldown;
        }

        private string BuildEffectsText()
        {
            if (_game.Effects.Count == 0) return "Эффекты: нет";

            StringBuilder sb = new StringBuilder("Эффекты: ");
            for (int i = 0; i < _game.Effects.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                CEffect effect = _game.Effects[i];
                sb.Append(effect.Title).Append(" (").Append(effect.Remaining.ToString("F1")).Append(" с)");
            }
            return sb.ToString();
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

        private static Brush ToBrush(string hex)
        {
            return (Brush)new BrushConverter().ConvertFromString(hex);
        }
    }
}
