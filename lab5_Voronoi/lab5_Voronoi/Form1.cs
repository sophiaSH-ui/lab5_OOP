using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab5_Voronoi 
{
    public partial class Form1 : Form
    {
        public delegate double DistanceMetric(int dx, int dy);

        private class Seed
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Color Color { get; set; }
            public int PixelCount { get; set; }
        }

        private List<Seed> seeds = new List<Seed>();
        private Random rnd = new Random();

        public Form1()
        {
            InitializeComponent(); 

            if (cmbMetrics.Items.Count == 0)
            {
                cmbMetrics.Items.AddRange(new string[] { "Евклідова (Звичайна)", "Манхеттенська", "Чебишова" });
            }
            cmbMetrics.SelectedIndex = 0;

            btnGenerate.Click += (s, e) => GenerateRandomSeeds((int)numPoints.Value);
            btnClear.Click += (s, e) => { seeds.Clear(); RedrawDiagram(); };
            btnRemoveSmallest.Click += BtnRemoveSmallest_Click;

            chkMultiThread.CheckedChanged += (s, e) => RedrawDiagram();
            cmbMetrics.SelectedIndexChanged += (s, e) => RedrawDiagram();

            pbCanvas.MouseClick += PbCanvas_MouseClick;
            pbCanvas.SizeChanged += (s, e) => RedrawDiagram();
        }

        private void PbCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                seeds.Add(new Seed { X = e.X, Y = e.Y, Color = GetRandomColor() });
            }
            else if (e.Button == MouseButtons.Right)
            {
                var toRemove = seeds.FirstOrDefault(s => Math.Sqrt(Math.Pow(s.X - e.X, 2) + Math.Pow(s.Y - e.Y, 2)) < 10);
                if (toRemove != null)
                    seeds.Remove(toRemove);
            }
            RedrawDiagram();
        }

        private void GenerateRandomSeeds(int count)
        {
            seeds.Clear();
            int w = pbCanvas.Width;
            int h = pbCanvas.Height;
            if (w == 0 || h == 0) return;

            for (int i = 0; i < count; i++)
            {
                seeds.Add(new Seed
                {
                    X = rnd.Next(w),
                    Y = rnd.Next(h),
                    Color = GetRandomColor()
                });
            }
            RedrawDiagram();
        }

        private Color GetRandomColor()
        {
            return Color.FromArgb(rnd.Next(50, 255), rnd.Next(50, 255), rnd.Next(50, 255));
        }

        private DistanceMetric GetSelectedMetric()
        {
            switch (cmbMetrics.SelectedIndex)
            {
                case 1: return (dx, dy) => Math.Abs(dx) + Math.Abs(dy);
                case 2: return (dx, dy) => Math.Max(Math.Abs(dx), Math.Abs(dy));
                default: return (dx, dy) => dx * dx + dy * dy;
            }
        }

        private void RedrawDiagram()
        {
            int width = pbCanvas.Width;
            int height = pbCanvas.Height;
            if (width <= 0 || height <= 0 || seeds.Count == 0)
            {
                pbCanvas.Image = null;
                return;
            }

            foreach (var s in seeds) s.PixelCount = 0;

            int[] pixels = new int[width * height];
            DistanceMetric metric = GetSelectedMetric();
            bool isMultiThreaded = chkMultiThread.Checked;

            GC.Collect();
            long memBefore = GC.GetTotalMemory(true);
            var process = Process.GetCurrentProcess();
            TimeSpan cpuStart = process.TotalProcessorTime;
            Stopwatch sw = Stopwatch.StartNew();

            if (isMultiThreaded)
            {
                object syncRoot = new object();

                int blockSize = 50; 
                int blocksX = (int)Math.Ceiling((double)width / blockSize);
                int blocksY = (int)Math.Ceiling((double)height / blockSize);
                int totalBlocks = blocksX * blocksY;

                Parallel.For(0, totalBlocks, () => new int[seeds.Count], (blockIdx, loopState, localCounts) =>
                {
                    int bx = blockIdx % blocksX;
                    int by = blockIdx / blocksX;
                    int startX = bx * blockSize;
                    int startY = by * blockSize;
                    int endX = Math.Min(startX + blockSize, width);
                    int endY = Math.Min(startY + blockSize, height);

                    int cx = startX + blockSize / 2; 
                    int cy = startY + blockSize / 2; 

                    int closestDistToCenter = int.MaxValue;
                    foreach (var s in seeds)
                    {
                        int dist = Math.Abs(s.X - cx) + Math.Abs(s.Y - cy);
                        if (dist < closestDistToCenter) closestDistToCenter = dist;
                    }

                    int threshold = closestDistToCenter + blockSize * 3;
                    var candidates = new List<int>();

                    for (int i = 0; i < seeds.Count; i++)
                    {
                        if (Math.Abs(seeds[i].X - cx) + Math.Abs(seeds[i].Y - cy) <= threshold)
                        {
                            candidates.Add(i);
                        }
                    }

                    int[] localSeeds = candidates.ToArray();

                    for (int y = startY; y < endY; y++)
                    {
                        int offset = y * width;
                        for (int x = startX; x < endX; x++)
                        {
                            int bestIndex = localSeeds[0];
                            double minDistance = double.MaxValue;

                            for (int i = 0; i < localSeeds.Length; i++)
                            {
                                int seedIdx = localSeeds[i];
                                double d = metric(seeds[seedIdx].X - x, seeds[seedIdx].Y - y);
                                if (d < minDistance)
                                {
                                    minDistance = d;
                                    bestIndex = seedIdx;
                                }
                            }

                            pixels[offset + x] = seeds[bestIndex].Color.ToArgb();

                            localCounts[bestIndex]++;
                        }
                    }
                    return localCounts;
                },
                (localCounts) =>
                {
                    lock (syncRoot)
                    {
                        for (int i = 0; i < seeds.Count; i++)
                            seeds[i].PixelCount += localCounts[i];
                    }
                });
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    int offset = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        int bestIndex = 0;
                        double minDistance = double.MaxValue;

                        for (int i = 0; i < seeds.Count; i++)
                        {
                            double d = metric(seeds[i].X - x, seeds[i].Y - y);
                            if (d < minDistance)
                            {
                                minDistance = d;
                                bestIndex = i;
                            }
                        }

                        pixels[offset + x] = seeds[bestIndex].Color.ToArgb();
                        seeds[bestIndex].PixelCount++; 
                    }
                }
            }

            sw.Stop();
            TimeSpan cpuEnd = process.TotalProcessorTime;
            long memAfter = GC.GetTotalMemory(false);

            TimeSpan cpuTime = cpuEnd - cpuStart;
            long memUsed = Math.Max(0, memAfter - memBefore) / 1024; 

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bmp.UnlockBits(data);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                foreach (var s in seeds)
                {
                    g.FillEllipse(Brushes.Black, s.X - 3, s.Y - 3, 6, 6);
                    g.FillEllipse(Brushes.White, s.X - 1, s.Y - 1, 2, 2);
                }
            }

            pbCanvas.Image = bmp;

            lblStats.Text = $"Вершин: {seeds.Count}\n\n" +
                            $"Реальний час: {sw.ElapsedMilliseconds} мс\n" +
                            $"CPU час: {cpuTime.TotalMilliseconds:F0} мс\n" +
                            $"Витрати пам'яті: ~{memUsed} КБ";
        }

        private void BtnRemoveSmallest_Click(object sender, EventArgs e)
        {
            if (seeds.Count == 0) return;

            int toRemoveCount = (int)Math.Ceiling(seeds.Count * ((double)numRemovePercent.Value / 100.0));
            if (toRemoveCount >= seeds.Count) toRemoveCount = seeds.Count - 1;

            var sortedByArea = seeds.OrderBy(s => s.PixelCount).ToList();

            for (int i = 0; i < toRemoveCount; i++)
            {
                seeds.Remove(sortedByArea[i]);
            }

            RedrawDiagram();
        }
    }
}