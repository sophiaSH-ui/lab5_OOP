using lab5_HorseRacing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace lab5_HorseRacing
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static readonly Color[] AllAllowedColors =
        {
            Colors.Pink, Colors.Red, Colors.Green, Colors.Blue, Colors.Black,
            Colors.Orange, Colors.Purple, Colors.Cyan, Colors.Yellow, Colors.Brown,
            Colors.Magenta, Colors.Lime, Colors.Teal, Colors.Navy, Colors.Gold,
            Colors.Coral, Colors.Aqua, Colors.Violet, Colors.Salmon, Colors.Crimson,
            Colors.Indigo, Colors.Khaki, Colors.Olive, Colors.Plum, Colors.Silver, Colors.Tomato
        };

        private static readonly string[] AllNames =
        {
            "Lucky", "Ranger", "Willow", "Tucker", "Shadow", "Apollo",
            "Blaze", "Comet", "Dash", "Eclipse", "Flash", "Ghost", "Hunter", "Storm"
        };

        private double _balance;
        private Horse _selectedHorse;
        private double _betAmount;
        private bool _isRaceFinished;
        private string _resultTitle;
        private string _resultSubtitle;
        private bool _isRaceRunning;
        private bool _isRaceStarted;

        public ObservableCollection<Horse> Horses { get; set; }
        public List<BitmapImage> HorseFrames { get; set; }
        public BitmapImage SaddleImage { get; set; }
        public BitmapImage JockeyImage { get; set; }

        public RelayCommand PlaceBetCommand { get; }
        public RelayCommand AddHorseCommand { get; }
        public RelayCommand RemoveHorseCommand { get; }
        public RelayCommand ChangeHorseColorCommand { get; }

        public bool IsRaceRunning
        {
            get => _isRaceRunning;
            set { _isRaceRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanReset)); }
        }

        public bool IsRaceStarted
        {
            get => _isRaceStarted;
            set { _isRaceStarted = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanReset)); }
        }

        public double Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        public Horse SelectedHorse
        {
            get => _selectedHorse;
            set { _selectedHorse = value; OnPropertyChanged(); }
        }

        public double BetAmount
        {
            get => _betAmount;
            set { _betAmount = value; OnPropertyChanged(); }
        }

        public bool IsRaceFinished
        {
            get => _isRaceFinished;
            set { _isRaceFinished = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanReset)); }
        }

        public string ResultTitle
        {
            get => _resultTitle;
            set { _resultTitle = value; OnPropertyChanged(); }
        }

        public string ResultSubtitle
        {
            get => _resultSubtitle;
            set { _resultSubtitle = value; OnPropertyChanged(); }
        }

        public bool CanReset => IsRaceStarted && (IsRaceFinished || !IsRaceRunning);

        public MainViewModel()
        {
            Balance = 250;
            BetAmount = 20;

            HorseFrames = new List<BitmapImage>();
            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    string uriString = $"pack://application:,,,/Images/horse_{i}.png";
                    BitmapImage frame = new BitmapImage(new Uri(uriString, UriKind.Absolute));
                    HorseFrames.Add(frame);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load horse_{i}.png: {ex.Message}");
                }
            }

            try
            {
                SaddleImage = new BitmapImage(new Uri("pack://application:,,,/Images/saddle.png", UriKind.Absolute));
                JockeyImage = new BitmapImage(new Uri("pack://application:,,,/Images/jockey.png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load saddle/jockey: {ex.Message}");
            }

            Horses = new ObservableCollection<Horse>
            {
                new Horse("Lucky", Colors.Pink, 1.25),
                new Horse("Ranger", Colors.Red, 1.50),
                new Horse("Willow", Colors.Green, 2.00),
                new Horse("Tucker", Colors.Blue, 1.75)
            };

            ICollectionView view = CollectionViewSource.GetDefaultView(Horses);
            if (view is ICollectionViewLiveShaping liveView)
            {
                liveView.IsLiveSorting = true;
                liveView.LiveSortingProperties.Add(nameof(Horse.PositionX));
            }
            view.SortDescriptions.Add(new SortDescription(nameof(Horse.PositionX), ListSortDirection.Descending));

            PlaceBetCommand = new RelayCommand(PlaceBet, CanPlaceBet);
            AddHorseCommand = new RelayCommand(AddHorse, CanAddHorse);
            RemoveHorseCommand = new RelayCommand(RemoveHorse, CanRemoveHorse);
            ChangeHorseColorCommand = new RelayCommand(ChangeHorseColor);
        }

        private bool CanPlaceBet(object obj)
        {
            if (SelectedHorse == null || Balance < BetAmount || Horses.Any(h => h.PositionX > 0))
                return false;

            var alreadyBettedHorse = Horses.FirstOrDefault(h => h.MoneyBet > 0);
            if (alreadyBettedHorse != null && alreadyBettedHorse != SelectedHorse)
                return false;

            return true;
        }

        private void PlaceBet(object obj)
        {
            Balance -= BetAmount;
            SelectedHorse.MoneyBet += BetAmount;
            OnPropertyChanged(nameof(CanReset));
        }

        private bool CanAddHorse(object obj) => Horses.Count < 14 && Horses.All(h => h.PositionX == 0);

        private void AddHorse(object obj)
        {
            var usedColors = Horses.Select(h => h.HorseColor).ToList();
            var unusedColor = AllAllowedColors.First(c => !usedColors.Contains(c));

            var usedNames = Horses.Select(h => h.Name).ToList();
            var unusedName = AllNames.First(n => !usedNames.Contains(n));

            Horses.Add(new Horse(unusedName, unusedColor, 2.00));
        }

        private bool CanRemoveHorse(object obj) => Horses.Count > 2 && Horses.All(h => h.PositionX == 0);

        private void RemoveHorse(object obj)
        {
            Horses.Remove(Horses.Last());
            if (SelectedHorse != null && !Horses.Contains(SelectedHorse))
                SelectedHorse = null;
        }

        private void ChangeHorseColor(object obj)
        {
            if (obj is Horse horse && Horses.All(h => h.PositionX == 0))
            {
                var usedColors = Horses.Select(h => h.HorseColor).ToList();
                var availableColors = AllAllowedColors.Where(c => !usedColors.Contains(c) || c == horse.HorseColor).ToList();

                int currentIndex = availableColors.IndexOf(horse.HorseColor);
                int nextIndex = (currentIndex + 1) % availableColors.Count;

                horse.HorseColor = availableColors[nextIndex];
            }
        }

        public void ResetRace()
        {
            foreach (var horse in Horses)
                horse.Reset();

            ResultTitle = string.Empty;
            ResultSubtitle = string.Empty;
            IsRaceFinished = false;
            IsRaceStarted = false; 
            IsRaceRunning = false; 
        }

        public void ProcessResults()
        {
            var winners = Horses.Where(h => h.IsFinished).OrderBy(h => h.RaceTime).ToList();

            if (winners.Count > 0)
            {
                var firstPlace = winners.First();
                var bettedHorse = Horses.FirstOrDefault(h => h.MoneyBet > 0);

                if (bettedHorse != null)
                {
                    if (bettedHorse == firstPlace)
                    {
                        double payout = bettedHorse.MoneyBet * bettedHorse.Coefficient;
                        Balance += payout;
                        ResultTitle = $"🏆  {bettedHorse.Name} WON!";
                        ResultSubtitle = $"Payout:  +{payout:F2} $  →  Balance: {Balance:F2} $";
                    }
                    else
                    {
                        ResultTitle = $"Your horse {bettedHorse.Name} lost";
                        ResultSubtitle = $"Winner:  {firstPlace.Name}";
                    }
                }
                else
                {
                    ResultTitle = $"🏁  Race Finished";
                    ResultSubtitle = $"Winner:  {firstPlace.Name}";
                }

                double baseCoef = 1.10;
                for (int i = 0; i < winners.Count; i++)
                    winners[i].Coefficient = Math.Round(baseCoef + (i * 0.4), 2);
            }

            foreach (var horse in Horses)
                horse.MoneyBet = 0;

            IsRaceFinished = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}