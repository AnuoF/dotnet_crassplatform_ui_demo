using AvaCharts.Model;
using Avalonia.Controls;
using Avalonia.Threading;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AvaCharts
{
    public partial class WaveChartViewOxy : UserControl
    {
        private OxyPlot.Avalonia.PlotView _mPlot;
        private PlotModel _mModel;
        private LineSeries _line;

        private ConcurrentQueue<WavSampleData> _dataQueue = new ConcurrentQueue<WavSampleData>();
        private AutoResetEvent _dataEvent = new AutoResetEvent(false);
        private int MaxCount = 10000;

        private double _xIndex = 0;   // XÖáÆðÊ¼Öµ 

        public WaveChartViewOxy()
        {
            InitializeComponent();

            CreateChart();

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
            _dataQueue.Clear();
            _line.Points.Clear();

            Dispatcher.UIThread.Post(() =>
            {
                _mModel.InvalidatePlot(true);
            });
        }

        private void Deal()
        {
            DateTime prevUpdateTime = DateTime.MinValue;

            while (true)
            {
                try
                {
                    if (_dataEvent.WaitOne())
                    {
                        if (_dataQueue.TryDequeue(out WavSampleData data))
                        {
                            if (_line.Points.Count > MaxCount)
                            {
                                _line.Points.RemoveAt(0);
                            }

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

                            if (_line.Points.Count >= MaxCount)
                            {
                                _line.Points.RemoveRange(0, tmpYBuff.Length);
                            }

                            DataPoint[] points = new DataPoint[tmpYBuff.Length];
                            for (int i = 0; i < tmpYBuff.Length; i++)
                            {
                                var p = new DataPoint(0, tmpYBuff[i]);
                                points[i] = p;
                            }

                            _line.Points.AddRange(points);

                            for (int i = 0; i < _line.Points.Count; i++)
                            {
                                var point = _line.Points[i];
                                _line.Points[i] = new DataPoint(i, point.Y);
                            }

                            //_mModel.ResetAllAxes();

                            if ((DateTime.Now - prevUpdateTime).TotalMilliseconds > 50)
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    _mModel.InvalidatePlot(true);
                                });
                            }

                            //_line.Points.Add(new DataPoint(_xIndex, data.SampleData[0]));
                            //_mModel.InvalidatePlot(true);
                            //_xIndex++;
                        }

                        _dataEvent.Reset();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("WaveChartViewOxy:" + ex.Message);
                }
            }
        }


        private void CreateChart()
        {
            _mPlot = new OxyPlot.Avalonia.PlotView();

            _mPlot.Margin = new Avalonia.Thickness(30, 0, 0, 30);

            _mModel = new PlotModel();

            _line = new OxyPlot.Series.LineSeries()
            {
                Color = OxyColor.FromArgb(255, 7, 208, 220),
                StrokeThickness = 1
                //MarkerSize = 1,
                //MarkerStroke = OxyColors.DarkGreen,
                //MarkerType = MarkerType.Circle
            };

            _mModel.Series.Add(_line);

            _mPlot.Model = _mModel;

            this.gridChart.Children.Add(_mPlot);
        }

    }
}
