using Avalonia.Controls;
using Avalonia.Threading;
using ScottPlot.Avalonia;
using ScottPlot.Plottable;
using scottplotcharts.Model;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace scottplotcharts
{
    public partial class SpectrumView : UserControl
    {
        private AvaPlot _mPlot;
        private SignalPlotXY _realLinePlot;
        private SignalPlotXY _maxLinePlot;
        private SignalPlotXY _minLinePlot;

        private ConcurrentQueue<SpectrumData> _dataQueue = new ConcurrentQueue<SpectrumData>();
        private ManualResetEvent _drawEvent = new ManualResetEvent(false);
        private SpectrumData _prevSpectrum;

        private bool _isNeedRestBuff = true;
        private double[] _maxBuff = new double[0];
        private double[] _minBuff = new double[0];
        private double[] _xBuff = new double[0];


        public SpectrumView()
        {
            InitializeComponent();

            CreatePlot();

            Task.Factory.StartNew(() => { DealData(); });
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

        public void Clear()
        {
            if (_dataQueue.Count > 0)
                _dataQueue.Clear();

            if (_realLinePlot != null)
            {
                _mPlot.Plot.Remove(_realLinePlot);
                _realLinePlot = null;
            }
            if (_maxLinePlot != null)
            {
                _mPlot.Plot.Remove(_maxLinePlot);
                _maxLinePlot = null;
            }
            if (_minLinePlot != null)
            {
                _mPlot.Plot.Remove(_minLinePlot);
                _minLinePlot = null;
            }

            _isNeedRestBuff = true;
            _prevSpectrum = null;

            Dispatcher.UIThread.Post(() => { _mPlot.Render(); });
        }

        private void DealData()
        {
            DateTime prevUpdateTime = DateTime.MinValue;

            while (true)
            {
                try
                {
                    if (_drawEvent.WaitOne())
                    {
                        if (!_dataQueue.TryDequeue(out SpectrumData spectrum))
                        {
                            continue;
                        }

                        int len = spectrum.Data.Length;
                        double[] realBuff = new double[len];

                        if (_isNeedRestBuff)
                        {
                            _xBuff = new double[len];
                            _maxBuff = new double[len];
                            _minBuff = new double[len];
                        }

                        for (int i = 0; i < len; i++)
                        {
                            var tv = spectrum.Data[i];
                            realBuff[i] = tv;
                            _xBuff[i] = i;

                            if (_isNeedRestBuff)
                            {
                                _minBuff[i] = tv;
                                _maxBuff[i] = tv;
                            }
                        }

                        if (_isNeedRestBuff)
                        {
                            //_maxBuff = _minBuff = realBuff;    // 这里是一个巨坑，如果这样写的话，后面 _maxBuff 的值就和 _minBuff 相同！！！！

                            _mPlot.Plot.SetOuterViewLimits(xMin: 0, xMax: len);
                            _mPlot.Plot.AxisAuto();

                            _isNeedRestBuff = false;
                        }
                        else
                        {
                            for (int i = 0; i < len; i++)
                            {
                                var min = _minBuff[i];
                                var max = _maxBuff[i];
                                var real = realBuff[i];

                                if (real < min)
                                    min = real;
                                if (max < real)
                                    max = real;

                                _minBuff[i] = min;   // Math.Min(_minBuff[i], realBuff[i]);
                                _maxBuff[i] = max;   // Math.Max(_maxBuff[i], realBuff[i]);

                                //System.Diagnostics.Debug.WriteLine($"min:{min} - max:{max}");
                            }
                        }

                        if (_realLinePlot == null)
                        {
                            _realLinePlot = _mPlot.Plot.AddSignalXY(_xBuff, realBuff, System.Drawing.Color.FromArgb(7, 208, 220));
                        }
                        else
                        {
                            _realLinePlot.Update(realBuff);
                        }

                        if (_maxLinePlot == null)
                        {
                            _maxLinePlot = _mPlot.Plot.AddSignalXY(_xBuff, _maxBuff, System.Drawing.Color.FromArgb(0xfb, 0x6a, 0x45));
                        }
                        else
                        {
                            _maxLinePlot.Update(_maxBuff);
                        }

                        if (_minLinePlot == null)
                        {
                            _minLinePlot = _mPlot.Plot.AddSignalXY(_xBuff, _minBuff, System.Drawing.Color.YellowGreen);
                        }
                        else
                        {
                            _minLinePlot.Update(_minBuff);
                        }

                        if ((DateTime.Now - prevUpdateTime).TotalMilliseconds > 50)
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                _mPlot.Render(true);
                            });
                            prevUpdateTime = DateTime.Now;
                        }

                        _drawEvent.Reset();
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        private void CreatePlot()
        {
            _mPlot = new AvaPlot();



            _mPlot.Render(true);
            this.gridChart.Children.Add(_mPlot);
        }
    }
}
