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
    public partial class SpectrumViewOxy : UserControl
    {
        private OxyPlot.Avalonia.PlotView _mPlot;
        private PlotModel _mModel;
        private LineSeries _realLine;
        private LineSeries _maxLine;
        private LineSeries _minLine;

        private ConcurrentQueue<SpectrumData> _dataQueue = new ConcurrentQueue<SpectrumData>();
        private ManualResetEvent _drawEvent = new ManualResetEvent(false);
        private SpectrumData _prevSpectrum;

        private double[] _maxBuff = new double[0];
        private double[] _minBuff = new double[0];
        private bool _isNeedRestBuff = true;

        public SpectrumViewOxy()
        {
            InitializeComponent();
            CreateChart();
            Task.Factory.StartNew(() => { Deal(); });
        }

        public void AddData(SpectrumData data)
        {
            if (_prevSpectrum != null)
            {
                if (_prevSpectrum.Start != data.Start || _prevSpectrum.Resolution != data.Resolution
                  || _prevSpectrum.ValidStart != data.ValidStart || _prevSpectrum.ValidStop != data.ValidStop
                  || _prevSpectrum.DataLength != data.DataLength)
                { // 如果数据有变化，则需要清空数据和图形
                    Clear();
                    _dataQueue.Enqueue(data);
                    _isNeedRestBuff = true;
                }
                else
                {
                    _dataQueue.Enqueue(data);
                }
            }
            else
            {
                _dataQueue.Enqueue(data);   // 第一包数据不需要绘制，所以不用通知线程
                if (_dataQueue.Count > 10)
                    _dataQueue.TryDequeue(out _);
            }

            _prevSpectrum = data;
            _drawEvent.Set();
        }

        public void AddZone(int count)
        {

        }

        public void Clear()
        {
            _dataQueue.Clear();

            _isNeedRestBuff = true;
            _prevSpectrum = null;

            _realLine.Points.Clear();
            _maxLine.Points.Clear();
            _minLine.Points.Clear();

            Dispatcher.UIThread.Post(() =>
            {
                _mModel.InvalidatePlot(true);
            });
        }

        public void Deal()
        {
            DateTime prevUpdateTime = DateTime.MinValue;

            while (true)
            {
                try
                {
                    if (_drawEvent.WaitOne())
                    {
                        if (_dataQueue.TryDequeue(out SpectrumData spectrum))
                        {
                            int len = spectrum.Data.Length;
                            double[] realBuff = new double[len];

                            if (_isNeedRestBuff)
                            {
                                _maxBuff = new double[len];
                                _minBuff = new double[len];
                            }

                            DataPoint[] points = new DataPoint[len];

                            for (int i = 0; i < len; i++)
                            {
                                var tv = spectrum.Data[i];
                                realBuff[i] = tv;

                                if (_isNeedRestBuff)
                                {
                                    _minBuff[i] = tv;
                                    _maxBuff[i] = tv;
                                }
                                else
                                {
                                    var min = _minBuff[i];
                                    var max = _maxBuff[i];
                                    var real = realBuff[i];

                                    if (real < min)
                                        min = real;
                                    if (real > max)
                                        max = real;

                                    _minBuff[i] = min;
                                    _maxBuff[i] = max;
                                }

                                if (_realLine.Points.Count != len)
                                {
                                    _realLine.Points.Add(new DataPoint(i, tv));
                                }
                                else
                                {
                                    _realLine.Points[i] = new DataPoint(i, tv);
                                }

                                if (_maxLine.Points.Count != len)
                                {
                                    _maxLine.Points.Add(new DataPoint(i, _minBuff[i]));
                                }
                                else
                                {
                                    _maxLine.Points[i] = new DataPoint(i, _maxBuff[i]);
                                }

                                if (_minLine.Points.Count != len)
                                {
                                    _minLine.Points.Add(new DataPoint(i, _minBuff[i]));
                                }
                                else
                                {
                                    _minLine.Points[i] = new DataPoint(i, _minBuff[i]);
                                }
                            }

                            if (_isNeedRestBuff)
                            {
                                //  这里可以做一些事，比如调整X轴范围等

                                _isNeedRestBuff = false;
                            }

                            if ((DateTime.Now - prevUpdateTime).TotalMilliseconds > 50)
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    _mModel.InvalidatePlot(true);
                                });
                                prevUpdateTime = DateTime.Now;
                            }
                        }

                        _drawEvent.Reset();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SpectrumViewOxy: " + ex.Message);
                }
            }
        }

        private void CreateChart()
        {
            _mPlot = new OxyPlot.Avalonia.PlotView();
            _mPlot.Margin = new Avalonia.Thickness(30, 0, 0, 30);

            _mModel = new PlotModel();
            _realLine = new LineSeries();
            _realLine.Color = OxyColor.FromArgb(255, 7, 208, 220);
            _realLine.StrokeThickness = 1;
            _mModel.Series.Add(_realLine);

            _maxLine = new LineSeries();
            _maxLine.Color = OxyColor.FromRgb(0xfb, 0x6a, 0x45);
            _maxLine.StrokeThickness = 1;
            _mModel.Series.Add(_maxLine);

            _minLine = new LineSeries();
            _minLine.Color = OxyColor.FromRgb(173, 255, 47);
            _minLine.StrokeThickness = 1;
            _mModel.Series.Add(_minLine);

            _mPlot.Model = _mModel;
            this.gridChart.Children.Add(_mPlot);
        }

    }
}
