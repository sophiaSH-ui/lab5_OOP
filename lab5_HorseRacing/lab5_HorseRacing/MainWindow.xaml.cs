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

namespace lab5_HorseRacing
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _isRacing;
        private Stopwatch _stopwatch;
        private const double TrackLength = 600;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            _stopwatch = new Stopwatch();

            Loaded += (s, e) => RenderFrame();
        }

        private async void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            if (_isRacing) return;

            _isRacing = true;
            _stopwatch.Restart();

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
            _viewModel.ProcessResults();
        }

        private void StopSimulation_Click(object sender, RoutedEventArgs e)
        {
            _isRacing = false;
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
            double width = RenderImage.ActualWidth > 0 ? RenderImage.ActualWidth : 800;
            double height = RenderImage.ActualHeight > 0 ? RenderImage.ActualHeight : 400;

            var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();

            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.LightGreen, null, new Rect(0, 0, width, height));
                dc.DrawLine(new Pen(Brushes.Red, 3), new Point(TrackLength, 0), new Point(TrackLength, height));

                double targetImageWidth = 80;
                double targetImageHeight = 60;

                double laneHeight = height / _viewModel.Horses.Count;

                for (int i = 0; i < _viewModel.Horses.Count; i++)
                {
                    var horse = _viewModel.Horses[i];

                    if (!horse.IsFinished && _isRacing)
                    {
                        horse.PositionX += horse.Acceleration;
                        horse.UpdateAnimation(_viewModel.HorseFrames.Count);
                    }

                    double drawX = horse.PositionX;
                    double centerY = (i * laneHeight) + (laneHeight / 2);
                    double drawY = centerY - (targetImageHeight / 2);

                    Rect horseRect = new Rect(drawX, drawY, targetImageWidth, targetImageHeight);

                    if (_viewModel.HorseFrames != null && _viewModel.HorseFrames.Any())
                    {
                        BitmapImage currentFrame = _viewModel.HorseFrames[horse.AnimationIndex];
                        dc.DrawImage(currentFrame, horseRect);
                    }

                    double saddleWidth = 30;
                    double saddleHeight = 30;
                    double saddleX = drawX + 25;
                    double saddleY = drawY + 5;
                    Rect saddleRect = new Rect(saddleX, saddleY, saddleWidth, saddleHeight);

                    if (_viewModel.SaddleImage != null)
                    {
                        var saddleBrush = new ImageBrush(_viewModel.SaddleImage);
                        dc.PushOpacityMask(saddleBrush);
                        dc.DrawRectangle(new SolidColorBrush(horse.HorseColor), null, saddleRect);
                        dc.Pop();
                    }

                    double jockeyWidth = 40;
                    double jockeyHeight = 40;
                    double jockeyX = drawX + 15;
                    double jockeyY = drawY - 15;
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
                if (horse.PositionX >= TrackLength && !horse.IsFinished)
                {
                    horse.IsFinished = true;
                    horse.RaceTime = _stopwatch.Elapsed;
                    horse.PositionX = TrackLength;
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