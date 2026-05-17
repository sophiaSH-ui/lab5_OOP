using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;


namespace lab5_HorseRacing
{
    public class Horse : INotifyPropertyChanged
    {
        private static readonly Random Rnd = new Random();
        private static readonly object RndLock = new object();

        private double _speed;

        private string _name;
        private Color _horseColor;
        private TimeSpan _raceTime;
        private double _positionX;
        private double _coefficient;
        private double _moneyBet;
        private bool _isFinished;
        private int _animationIndex;

        public double Acceleration { get; private set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public Color HorseColor
        {
            get => _horseColor;
            set { _horseColor = value; OnPropertyChanged(); }
        }

        public TimeSpan RaceTime
        {
            get => _raceTime;
            set { _raceTime = value; OnPropertyChanged(); }
        }

        public double PositionX
        {
            get => _positionX;
            set { _positionX = value; OnPropertyChanged(); }
        }

        public double Coefficient
        {
            get => _coefficient;
            set { _coefficient = value; OnPropertyChanged(); }
        }

        public double MoneyBet
        {
            get => _moneyBet;
            set { _moneyBet = value; OnPropertyChanged(); }
        }

        public bool IsFinished
        {
            get => _isFinished;
            set { _isFinished = value; OnPropertyChanged(); }
        }

        public int AnimationIndex
        {
            get => _animationIndex;
            set { _animationIndex = value; OnPropertyChanged(); }
        }

        public Horse(string name, Color color, double coefficient)
        {
            Name = name;
            HorseColor = color;
            Coefficient = coefficient;

            lock (RndLock)
            {
                _speed = Rnd.Next(5, 15);
            }

            PositionX = 0;
            IsFinished = false;
            AnimationIndex = 0; 
        }

        public async Task ChangeAcceleration()
        {
            await Task.Run(() =>
            {
                double multiplier;
                lock (RndLock)
                {
                    multiplier = Rnd.NextDouble() * (1.5 - 0.3) + 0.3;
                }
                Acceleration = _speed * multiplier;
            });
        }

        public void UpdateAnimation(int totalFrames)
        {
            if (totalFrames > 0)
            {
                AnimationIndex = (AnimationIndex + 1) % totalFrames;
            }
        }

        public void Reset()
        {
            PositionX = 0;
            IsFinished = false;
            RaceTime = TimeSpan.Zero;
            MoneyBet = 0;
            AnimationIndex = 0; 

            lock (RndLock)
            {
                _speed = Rnd.Next(5, 15);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}