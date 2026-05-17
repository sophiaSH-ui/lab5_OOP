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

                double laneHeight = height / _viewModel.Horses.Count;

                for (int i = 0; i < _viewModel.Horses.Count; i++)
                {
                    var horse = _viewModel.Horses[i];

                    if (!horse.IsFinished)
                    {
                        horse.PositionX += horse.Acceleration;
                    }

                    double drawX = horse.PositionX;
                    double drawY = (i * laneHeight) + (laneHeight / 2) - 15;

                    var brush = new SolidColorBrush(horse.HorseColor);
                    dc.DrawRectangle(brush, null, new Rect(drawX, drawY, 40, 30));
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