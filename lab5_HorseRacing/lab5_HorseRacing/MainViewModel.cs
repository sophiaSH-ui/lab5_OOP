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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace lab5_HorseRacing
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double _balance;
        private Horse _selectedHorse;
        private double _betAmount;

        public ObservableCollection<Horse> Horses { get; set; }
        public List<BitmapImage> HorseFrames { get; set; }

        public BitmapImage SaddleImage { get; set; }
        public BitmapImage JockeyImage { get; set; }

        public RelayCommand PlaceBetCommand { get; }

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
                new Horse("Tucker", Colors.Blue, 1.75),
                new Horse("Shadow", Colors.Black, 3.00)
            };

            PlaceBetCommand = new RelayCommand(PlaceBet, CanPlaceBet);
        }

        private bool CanPlaceBet(object obj)
        {
            return SelectedHorse != null && Balance >= BetAmount && Horses.All(h => h.PositionX == 0);
        }

        private void PlaceBet(object obj)
        {
            Balance -= BetAmount;
            SelectedHorse.MoneyBet += BetAmount;
        }

        public void ResetRace()
        {
            foreach (var horse in Horses)
            {
                horse.Reset();
            }
        }

        public void ProcessResults()
        {
            var winners = Horses.Where(h => h.IsFinished).OrderBy(h => h.RaceTime).ToList();
            if (winners.Count > 0)
            {
                var firstPlace = winners.First();
                if (firstPlace.MoneyBet > 0)
                {
                    Balance += firstPlace.MoneyBet * firstPlace.Coefficient;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}