using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ScottPlot.Avalonia;
using ScottPlot.Plottable;
using scottplotcharts.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace scottplotcharts
{
    public partial class WaveChartView : UserControl
    {
        private AvaPlot _mPlot;
        private SignalPlotXY _linePlot;
        private ConcurrentQueue<WavSampleData> _dataQueue = new ConcurrentQueue<WavSampleData>();
        private AutoResetEvent _dataEvent = new AutoResetEvent(false);

        private int MaxCount = 10000;
        private List<short> _yValuesList = new List<short>();
        private double[] _xBuff;
        private double[] _yBuff;
        private bool _needFit = true;

        public WaveChartView()
        {
            InitializeComponent();

            _xBuff = new double[MaxCount];
            _yBuff = new double[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                _xBuff[i] = i;
            }

            CreatePlot();

            Task.Factory.StartNew(() => { Deal(); });
        }


        public void SetData(WavSampleData data)
        {
            if (data == null)
                return;

            _dataQueue.Enqueue(data);
            _dataEvent.Set();
            if (_dataQueue.Count > 10)
            {
                _dataQueue.TryDequeue(out _);
            }
        }

        public void Clear()
        {
            while (_dataQueue.Count > 0)
                _dataQueue.TryDequeue(out _);
        }

        private void Deal()
        {
            DateTime prevUpdateTime = DateTime.MinValue;

            while (true)
            {
                try
                {
                    if (_dataEvent.WaitOne(100))
                    {
                        if (_dataQueue.TryDequeue(out WavSampleData data))
                        {
                            int count = 1000;
                            short[] tmpYBuff = new short[count];
                            if (data.SampleData.Length > count)
                            {
                                Array.Copy(data.SampleData, tmpYBuff, count);
                            }
                            else
                            {
                                Array.Copy(data.SampleData, tmpYBuff, data.SampleData.Length);
                            }

                            if (_yValuesList.Count >= MaxCount)
                            {
                                _yValuesList.RemoveRange(0, tmpYBuff.Length);
                            }

                            _yValuesList.AddRange(tmpYBuff);
                            short minValue = 0;
                            short maxValue = 0;

                            for (int i = 0; i < _yValuesList.Count; i++)
                            {
                                var v = _yValuesList[i];
                                _yBuff[i] = v;

                                if (v < minValue)
                                    minValue = v;
                                if (v > maxValue)
                                    maxValue = v;
                            }


                            if (_needFit)
                            {
                                _needFit = false;
                                _mPlot.Plot.AxisAutoY();
                            }

                            _linePlot.Update(_yBuff);

                            if ((DateTime.Now - prevUpdateTime).TotalMilliseconds > 50)
                            {
                                _mPlot.Render(true);

                                //Dispatcher.UIThread.Post(() =>
                                //{
                                //    _mPlot.Render(true);
                                //});
                            }
                        }

                        _dataEvent.Reset();
                    }
                }
                catch (System.Exception)
                {

                }
            }
        }

        private void CreatePlot()
        {
            _mPlot = new AvaPlot();

            _mPlot.Background = new SolidColorBrush(Colors.Black);
            _mPlot.Foreground = new SolidColorBrush(Color.FromArgb(0xD4, 0xD4, 0xD4, 0xD4));
            _mPlot.BorderBrush = new SolidColorBrush(Colors.Red);
            _mPlot.BorderThickness = new Thickness(0);

            //_mPlot.Plot.Palette = new PlotBackground();
            _mPlot.Plot.Style(new MyPlotStyle());
            //_mPlot.Plot.Grid(true, System.Drawing.Color.FromArgb(0x1B, 0xD4, 0xD4, 0xD4), LineStyle.Dot);
            _mPlot.Padding = new Thickness(0);
            _mPlot.Margin = new Thickness(0);

            _mPlot.Configuration.LockHorizontalAxis = true;   // Ëø¶¨XÖá²»Ëõ·Å
            _mPlot.Configuration.Quality = ScottPlot.Control.QualityMode.Low;
            _mPlot.Configuration.RightClickDragZoom = false;
            _mPlot.Configuration.Pan = false;
            _mPlot.Configuration.Zoom = true;
            _mPlot.Configuration.LeftClickDragPan = false;
            _mPlot.Configuration.MiddleClickDragZoom = false;

            _mPlot.Plot.XAxis.IsVisible = false;
            _mPlot.Plot.SetAxisLimitsX(0, MaxCount);
            _mPlot.Plot.XAxis.LockLimits(true);
            _mPlot.Plot.XAxis.Hide();

            var padding = new ScottPlot.PixelPadding(0, 40, 5, 5);
            _mPlot.Plot.ManualDataArea(padding);


            _mPlot.Plot.YAxis.TickLabelFormat((d) => { return ((int)d).ToString(); });
            _mPlot.Plot.YAxis.Edge = ScottPlot.Renderable.Edge.Right;
            _mPlot.Plot.YAxis.TickLabelStyle(System.Drawing.Color.FromArgb(0xD4, 0xD4, 0xD4), fontSize: 10);

            _linePlot = _mPlot.Plot.AddSignalXY(_xBuff, _yBuff, System.Drawing.Color.FromArgb(255, 7, 208, 220));
            //_linePlot.BaselineY = 20000;
            //_linePlot.FillBelow(System.Drawing.Color.Black, 1);



            _mPlot.Refresh();


            this.gridChart.Children.Add(_mPlot);
        }
    }

    internal class MyPlotStyle : ScottPlot.Styles.IStyle
    {
        public MyPlotStyle()
        {

        }

        #region IStyle

        public System.Drawing.Color FigureBackgroundColor => System.Drawing.Color.FromArgb(15, 18, 23);

        public System.Drawing.Color DataBackgroundColor => System.Drawing.Color.FromArgb(15, 18, 23);

        public System.Drawing.Color FrameColor => System.Drawing.Color.Transparent;

        public System.Drawing.Color GridLineColor => System.Drawing.Color.Black;

        public System.Drawing.Color AxisLabelColor => System.Drawing.Color.FromArgb(0x2D, 0x2D, 0x2D);

        public System.Drawing.Color TitleFontColor => System.Drawing.Color.FromArgb(0x2D, 0x2D, 0x2D);

        public System.Drawing.Color TickLabelColor => System.Drawing.Color.FromArgb(0xD4, 0xD4, 0xD4, 0xD4);

        public System.Drawing.Color TickMajorColor => System.Drawing.Color.FromArgb(0x1B, 0xD4, 0xD4, 0xD4);

        public System.Drawing.Color TickMinorColor => System.Drawing.Color.FromArgb(0x1B, 0xD4, 0xD4, 0xD4);

        public string AxisLabelFontName => null;

        public string TitleFontName => null;

        public string TickLabelFontName => null;

        #endregion

    }
}
