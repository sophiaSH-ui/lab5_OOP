using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;

namespace lab5_HorseRacing
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _isRacing;
        private Stopwatch _stopwatch;

        private const double LogicalTrackLength = 1000;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            _stopwatch = new Stopwatch();

            foreach (var horse in _viewModel.Horses)
                horse.PropertyChanged += Horse_PropertyChanged;

            _viewModel.Horses.CollectionChanged += Horses_CollectionChanged;

            RenderImage.SizeChanged += (s, e) => { if (!_isRacing) RenderFrame(); };
            Loaded += (s, e) => RenderFrame();
        }

        private void Horses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Horse h in e.NewItems)
                    h.PropertyChanged += Horse_PropertyChanged;
            if (e.OldItems != null)
                foreach (Horse h in e.OldItems)
                    h.PropertyChanged -= Horse_PropertyChanged;
            if (!_isRacing)
                RenderFrame();
        }

        private void Horse_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_isRacing && e.PropertyName == nameof(Horse.HorseColor))
                RenderFrame();
        }

        private async void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            if (_isRacing || _viewModel.IsRaceFinished) return;

            _isRacing = true;
            _viewModel.IsRaceRunning = true;
            _viewModel.IsRaceStarted = true;

            _stopwatch.Start();

            while (_isRacing)
            {
                List<Task> tasks = new List<Task>();
                foreach (var horse in _viewModel.Horses)
                {
                    if (!horse.IsFinished)
                    {
                        tasks.Add(horse.ChangeAcceleration());
                    }
                }

                if (tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }

                RenderFrame();
                CheckFinishLine();

                await Task.Delay(30);
            }

            _stopwatch.Stop();
            _viewModel.IsRaceRunning = false;

            bool allFinished = _viewModel.Horses.All(h => h.IsFinished);

            if (allFinished && !_viewModel.IsRaceFinished)
            {
                Horse winner = _viewModel.Horses.Where(h => h.IsFinished).OrderBy(h => h.RaceTime).FirstOrDefault();

                var bettedHorses = _viewModel.Horses.Where(h => h.MoneyBet > 0).ToList();

                double totalPayout = 0;
                double totalLost = 0;

                if (winner != null)
                {
                    foreach (var h in bettedHorses)
                    {
                        if (h == winner)
                            totalPayout += h.MoneyBet * h.Coefficient;
                        else
                            totalLost += h.MoneyBet;
                    }
                }

                _viewModel.ProcessResults();

                string msg;
                if (bettedHorses.Any())
                {
                    if (totalPayout > 0)
                        msg = $"Winner is {winner?.Name}!\nYour winning payout: {totalPayout:F2} $\nYou lost on other bets: {totalLost:F2} $";
                    else
                        msg = $"Winner is {winner?.Name}.\nUnfortunately, all your bets lost (Total lost: {totalLost:F2} $).";
                }
                else
                {
                    msg = $"Race finished!\nWinner: {winner?.Name}";
                }

                MessageBox.Show(msg, "Race Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StopSimulation_Click(object sender, RoutedEventArgs e)
        {
            _isRacing = false;
            if (_viewModel != null)
            {
                _viewModel.IsRaceRunning = false;
            }
        }

        private async void ResetRace_Click(object sender, RoutedEventArgs e)
        {
            _isRacing = false;
            _stopwatch.Stop();
            _stopwatch.Reset();

            await Task.Delay(50);

            _viewModel.ResetRace();
            RenderFrame();
        }

        private void RenderFrame()
        {
            double width = RenderImage.ActualWidth > 0 ? RenderImage.ActualWidth : 1140;
            double height = RenderImage.ActualHeight > 0 ? RenderImage.ActualHeight : 500;

            var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();

            var sortedHorses = _viewModel.Horses.OrderByDescending(h => h.PositionX).ToList();
            for (int j = 0; j < sortedHorses.Count; j++)
            {
                sortedHorses[j].Place = j + 1;
            }

            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                double stripeWidth = 60;
                for (double x = 0; x < width; x += stripeWidth)
                {
                    var color = (int)(x / stripeWidth) % 2 == 0 ? Color.FromRgb(70, 160, 20) : Color.FromRgb(60, 145, 15);
                    dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(x, 0, stripeWidth, height));
                }

                double laneHeight = height / _viewModel.Horses.Count;

                for (int i = 0; i < _viewModel.Horses.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)), null, new Rect(0, i * laneHeight, width, laneHeight));
                    }
                }

                var lanePen = new Pen(new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)), 1.5)
                {
                    DashStyle = new DashStyle(new double[] { 10, 10 }, 0)
                };
                for (int i = 1; i < _viewModel.Horses.Count; i++)
                {
                    dc.DrawLine(lanePen, new Point(0, i * laneHeight), new Point(width, i * laneHeight));
                }

                double startPixelX = width * 0.08;
                double finishPixelX = width * 0.88;
                double trackPixelLength = finishPixelX - startPixelX;

                double scaleX = trackPixelLength / LogicalTrackLength;

                dc.DrawLine(new Pen(Brushes.White, 4), new Point(startPixelX, 0), new Point(startPixelX, height));

                double checkerSize = 8;
                for (double y = 0; y < height; y += checkerSize)
                {
                    Brush b1 = (int)(y / checkerSize) % 2 == 0 ? Brushes.Black : Brushes.White;
                    Brush b2 = (int)(y / checkerSize) % 2 == 0 ? Brushes.White : Brushes.Black;

                    dc.DrawRectangle(b1, null, new Rect(finishPixelX, y, checkerSize, checkerSize));
                    dc.DrawRectangle(b2, null, new Rect(finishPixelX + checkerSize, y, checkerSize, checkerSize));
                }

                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)), null, new Rect(finishPixelX, 0, checkerSize * 2, height));

                double targetImageWidth = 85;
                double targetImageHeight = 65;

                for (int i = 0; i < _viewModel.Horses.Count; i++)
                {
                    var horse = _viewModel.Horses[i];

                    if (!horse.IsFinished && _isRacing)
                    {
                        horse.PositionX += horse.Acceleration;
                        horse.UpdateAnimation(_viewModel.HorseFrames.Count);
                        horse.RaceTime = _stopwatch.Elapsed;
                    }

                    double physicalX = startPixelX + (horse.PositionX * scaleX) - targetImageWidth + 15;

                    double centerY = (i * laneHeight) + (laneHeight / 2);
                    double physicalY = centerY - (targetImageHeight / 2);

                    Rect horseRect = new Rect(physicalX, physicalY, targetImageWidth, targetImageHeight);

                    dc.DrawEllipse(
                        new SolidColorBrush(Color.FromArgb(90, 0, 0, 0)), null,
                        new Point(physicalX + targetImageWidth / 2, physicalY + targetImageHeight - 5),
                        targetImageWidth / 2.5, 6);

                    if (_viewModel.HorseFrames != null && _viewModel.HorseFrames.Any())
                    {
                        BitmapImage currentFrame = _viewModel.HorseFrames[horse.AnimationIndex];
                        dc.DrawImage(currentFrame, horseRect);
                    }

                    double saddleWidth = 25;
                    double saddleHeight = 25;
                    double saddleX = physicalX + 25;
                    double saddleY = physicalY + 15;
                    Rect saddleRect = new Rect(saddleX, saddleY, saddleWidth, saddleHeight);

                    if (_viewModel.SaddleImage != null)
                    {
                        var saddleBrush = new ImageBrush(_viewModel.SaddleImage);
                        dc.PushOpacityMask(saddleBrush);
                        dc.DrawRectangle(new SolidColorBrush(horse.HorseColor), null, saddleRect);
                        dc.Pop();
                    }

                    double jockeyWidth = 35;
                    double jockeyHeight = 35;
                    double jockeyX = physicalX + 25;
                    double jockeyY = physicalY - 1;
                    Rect jockeyRect = new Rect(jockeyX, jockeyY, jockeyWidth, jockeyHeight);

                    if (_viewModel.JockeyImage != null)
                    {
                        dc.DrawImage(_viewModel.JockeyImage, jockeyRect);
                    }
                }
            }

            bitmap.Render(drawingVisual);
            RenderImage.Source = bitmap;
        }

        private void CheckFinishLine()
        {
            bool allFinished = true;
            foreach (var horse in _viewModel.Horses)
            {
                if (horse.PositionX >= LogicalTrackLength && !horse.IsFinished)
                {
                    horse.IsFinished = true;
                    horse.RaceTime = _stopwatch.Elapsed;
                    horse.PositionX = LogicalTrackLength;
                }

                if (!horse.IsFinished)
                {
                    allFinished = false;
                }
            }

            if (allFinished)
            {
                _isRacing = false;
            }
        }
    }
}