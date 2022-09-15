using AvaCharts.Model;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvaCharts
{
    public partial class SpectrogramViewOxy : UserControl
    {
        private OxyPlot.Avalonia.PlotView _mPlot;
        private PlotModel _mModel;
        private HeatMapSeries _heatMap;

        private int _displayDataLength = 500;
        private ConcurrentQueue<SpectrumData> _dataQueue = new ConcurrentQueue<SpectrumData>();
        private ManualResetEvent _dataEvent = new ManualResetEvent(false);
        private SpectrumData _prevSpectrum = null;



        public SpectrogramViewOxy()
        {
            InitializeComponent();
            CreateChart();
            Task.Factory.StartNew(() => { Deal(); });
        }

        public int DisplayDataLength
        {
            get { return GetValue(DisplayDataLengthProperty); }
            set { SetValue(DisplayDataLengthProperty, value); }  // 这里貌似不会进断点，但其值却变了。
        }

        public static readonly StyledProperty<int> DisplayDataLengthProperty =
            AvaloniaProperty.Register<SpectrogramViewOxy, int>("DisplayDataLength", defaultValue: 500, inherits: false,
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
                //validate: Validate, coerce: Coerce,
                notifying: DisplayDataLengthChanged);

        private static void DisplayDataLengthChanged(IAvaloniaObject obj, bool before)
        {
            var control = (SpectrogramViewOxy)obj;

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
                }
                else
                {
                    _dataQueue.Enqueue(data);
                    if (_dataQueue.Count > _displayDataLength)
                    {
                        _dataQueue.TryDequeue(out _);
                    }
                }
            }
            else
            {
                _dataQueue.Enqueue(data);
            }

            _prevSpectrum = data;
            _dataEvent.Set();
        }

        public void Clear()
        {
            _dataQueue.Clear();
            _prevSpectrum = null;

            if (_heatMap != null)
            {
                _mModel.Series.Remove(_heatMap);
                _heatMap = null;
            }

            Dispatcher.UIThread.Post(() =>
            {
                _mModel.InvalidatePlot(true);
            });
        }

        private void Deal()
        {
            DateTime prevUpdateTime = DateTime.Now;

            while (true)
            {
                try
                {
                    if (_dataEvent.WaitOne())
                    {
                        if (_dataQueue.Count <= 0)
                        {
                            _dataEvent.Reset();
                            continue;
                        }

                        var datas = _dataQueue.ToArray();
                        int xLen = datas[0].Data.Length;
                        int yLen = datas.Length;
                        double[,] arr2d = new double[xLen, yLen];
                        for (int x = 0; x < xLen; x++)
                        {
                            for (int y = 0; y < yLen; y++)
                            {
                                arr2d[x, y] = datas[y].Data[x];
                            }
                        }

                        if (_heatMap == null)
                        {
                            _heatMap = new HeatMapSeries();
                            _heatMap.X0 = 0;
                            _heatMap.Y0 = 0;
                            _heatMap.X1 = 4096;
                            _heatMap.Y1 = _displayDataLength;
                            _heatMap.RenderMethod = HeatMapRenderMethod.Bitmap;
                            _heatMap.EdgeRenderingMode = EdgeRenderingMode.PreferSpeed;
                            _heatMap.RenderInLegend = false;
                            _heatMap.Interpolate = false;

                            _mModel.Series.Add(_heatMap);
                        }

                        _heatMap.Data = arr2d;

                        if ((DateTime.Now - prevUpdateTime).TotalMilliseconds > 100)
                        {
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _mModel.InvalidatePlot(true);
                            });

                            prevUpdateTime = DateTime.Now;
                        }

                        _dataEvent.Reset();
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SpectrogramViewOxy:" + ex.Message);
                }
            }
        }


        private void CreateChart()
        {
            _mPlot = new OxyPlot.Avalonia.PlotView();
            _mPlot.Margin = new Avalonia.Thickness(30, 0, 0, 30);

            _mModel = new PlotModel();

            _mModel.Axes.Add(new LinearColorAxis
            {
                Palette = OxyPalettes.Hot(14)
            });

            _mPlot.Model = _mModel;
            this.gridChart.Children.Add(_mPlot);
        }
    }
}
