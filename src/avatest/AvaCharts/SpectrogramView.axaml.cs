using AvaCharts.Model;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Input;
using ScottPlot.Avalonia;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvaCharts
{
    public partial class SpectrogramView : UserControl
    {
        private AvaPlot _mPlot;
        private List<Heatmap> _heatmapList = new List<Heatmap>();
        //private ConcurrentQueue<Heatmap> _heatmapQueue = new ConcurrentQueue<Heatmap>();
        private bool _isAutoX = true;

        private ConcurrentQueue<SpectrumData> _dataQueue = new ConcurrentQueue<SpectrumData>();
        private ManualResetEvent _drawEvent = new ManualResetEvent(false);
        private SpectrumData _prevSpectrum = null;

        public SpectrogramView()
        {
            InitializeComponent();

            CreatePlot();

            Task.Factory.StartNew(() => { DealData(); });
        }

        private int _displayDataLength = 500;
        public int DisplayDataLength
        {
            get { return GetValue(DisplayDataLengthProperty); }
            set { SetValue(DisplayDataLengthProperty, value); }  // 这里貌似不会进断点，但其值却变了。
        }

        public static readonly StyledProperty<int> DisplayDataLengthProperty =
            AvaloniaProperty.Register<SpectrogramView, int>("DisplayDataLength", defaultValue: 500, inherits: false,
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
                //validate: Validate, coerce: Coerce,
                notifying: DisplayDataLengthChanged);


        private static void DisplayDataLengthChanged(IAvaloniaObject obj, bool before)
        {
            var control = (SpectrogramView)obj;

            if (!before)
            {
                control._displayDataLength = control.DisplayDataLength;
                control.Clear();
                control._mPlot.Plot.SetAxisLimitsY(0, control.DisplayDataLength);  // 这里会实时的调整Y轴范围
                control._mPlot.Plot.SetOuterViewLimits(yMin: 0, yMax: control.DisplayDataLength);   //  限制Y轴放大的范围
            }
        }

        //private static bool Validate(int v)
        //{
        //    return true;
        //}

        //public static int Coerce(IAvaloniaObject obj, int v)
        //{
        //    return v;
        //}


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
                    _isAutoX = true;  // 数据变化之后需要重新调整X轴
                }
                else
                {
                    _dataQueue.Enqueue(data);
                    _drawEvent.Set();
                }
            }
            else
            {
                _dataQueue.Enqueue(data);   // 第一包数据不需要绘制，所以不用通知线程
            }

            _prevSpectrum = data;
        }

        public void Clear()
        {
            // 清空数据和图形

            while (_dataQueue.Count > 0)
                _dataQueue.Clear();

            if (_heatmapList.Count > 0)
            {
                foreach (var hm in _heatmapList)
                {
                    _mPlot.Plot.Remove(hm);
                }
                _heatmapList.Clear();
            }

            //_mPlot.Plot.RenderLock();
            //while (_heatmapQueue.Count > 0)
            //{
            //    if (_heatmapQueue.TryDequeue(out var delHm))
            //    {
            //        _mPlot.Plot.Remove(delHm);
            //    }
            //}

            _mPlot.Plot.RenderLock();
            _mPlot.Plot.SetAxisLimitsY(0, DisplayDataLength);  // 这里会实时的调整Y轴范围
            _mPlot.Plot.RenderUnlock();

            _isAutoX = true;

            SafeInvoe(() =>
            {
                _mPlot.Render();
            });
        }

        private void DealData()
        {
            var prevUpdateDateTime = DateTime.MinValue;

            while (true)
            {
                try
                {
                    if (_drawEvent.WaitOne())
                    {
                        if (_dataQueue.Count < 2)
                        {
                            _drawEvent.Reset();
                            continue;
                        }

                        while (_dataQueue.Count >= 2)
                        { // 当超过2条数据时就处理
                            _dataQueue.TryDequeue(out SpectrumData spec1);
                            _dataQueue.TryDequeue(out SpectrumData spec2);

                            int frameLength = spec1.Data.Length;
                            double[,] intensities = new double[2, frameLength];
                            for (int i = 0; i < frameLength; i++)
                            {
                                intensities[0, i] = spec1.Data[i] + 100;
                                intensities[1, i] = spec2.Data[i] + 100;
                            }

                            if (_heatmapList.Count > _displayDataLength / 2)
                            {
                                // 超过显示的长度，则需要移除第一个，并把所有的图形往下移2位
                                var removeHm = _heatmapList[0];
                                _mPlot.Plot.Remove(removeHm);
                                _heatmapList.RemoveAt(0);

                                foreach (var offsetHm in _heatmapList)
                                {
                                    offsetHm.OffsetY -= 2;
                                }
                            }

                            var addHm = _mPlot.Plot.AddHeatmap(intensities, /*new Colormap(_waterfullColor)*/  Colormap.Rain, true);
                            addHm.FlipVertically = true;
                            addHm.Smooth = true;
                            addHm.OffsetY = _heatmapList.Count * 2;
                            addHm.OffsetY = _heatmapList.Count * 2;
                            _heatmapList.Add(addHm);


                            //if (_heatmapQueue.Count > _displayDataLength / 2)
                            //{
                            //    if (_heatmapQueue.TryDequeue(out var removeHm))
                            //    {
                            //        _mPlot.Plot.RenderLock();
                            //        _mPlot.Plot.Remove(removeHm);
                            //        _mPlot.Plot.RenderUnlock();
                            //    }

                            //    foreach (var offsetHm in _heatmapQueue)
                            //    {
                            //        offsetHm.OffsetY -= 2;
                            //    }
                            //}

                            //_mPlot.Plot.RenderLock();
                            //var addHm = _mPlot.Plot.AddHeatmap(intensities, /*new Colormap(_waterfullColor)*/  Colormap.Rain, false);
                            //_mPlot.Plot.RenderUnlock();

                            ////addHm.ScaleMax = _powerMax + 100;
                            ////addHm.ScaleMin = _powerMin + 100;
                            //addHm.FlipVertically = true;
                            //addHm.Smooth = true;
                            //addHm.OffsetY = _heatmapQueue.Count * 2;
                            //_heatmapQueue.Enqueue(addHm);

                        }

                        if (_isAutoX)
                        {
                            _mPlot.Plot.XAxis.LockLimits(false);
                            _mPlot.Plot.AxisAutoX();
                            _mPlot.Plot.XAxis.LockLimits(true);
                            _isAutoX = false;
                        }


                        if ((DateTime.Now - prevUpdateDateTime).TotalMilliseconds > 100)
                        {
                            _mPlot.Render(true);

                            //Dispatcher.UIThread.InvokeAsync(() =>
                            //{
                            //});

                            prevUpdateDateTime = DateTime.Now;
                        }

                        _drawEvent.Reset();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SpectrogramView:" + ex.Message);
                }
            }
        }


        private void CreatePlot()
        {
            _mPlot = new AvaPlot();

            _mPlot.BorderThickness = new Thickness(0);
            _mPlot.Padding = new Thickness(0);
            _mPlot.Margin = new Thickness(0);

            _mPlot.Configuration.Quality = ScottPlot.Control.QualityMode.Low;
            _mPlot.Configuration.RightClickDragZoom = false;

            _mPlot.Plot.Style(new WaterFullPlotStyle());
            _mPlot.Plot.ManualDataArea(new ScottPlot.PixelPadding(0, 40, 5, 5));
            _mPlot.Plot.Margins(0, 0);

            _mPlot.Plot.YAxis.Edge = ScottPlot.Renderable.Edge.Right;
            _mPlot.Plot.YAxis.TickLabelStyle(System.Drawing.Color.FromArgb(0xD4, 0xD4, 0xD4), fontSize: 10);

            _mPlot.Plot.SetAxisLimitsY(0, DisplayDataLength);  // 这里会实时的调整Y轴范围
            _mPlot.Plot.SetOuterViewLimits(yMin: 0, yMax: DisplayDataLength);   //  限制Y轴放大的范围

            //double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            //double[] dataY = new double[] { 1, 4, 9, 16, 25 };
            //_mPlot.Plot.AddScatter(dataX, dataY);

            _mPlot.Render();
            gridChart.Children.Add(_mPlot);
        }


        private void SafeInvoe(Action action, bool isPost = true)
        {
            if (isPost)
                Dispatcher.UIThread.Post(action);
            else
                Dispatcher.UIThread.InvokeAsync(action);
        }
    }

    /// <summary>
    /// 自定义的图表Style
    /// </summary>
    internal class WaterFullPlotStyle : ScottPlot.Styles.IStyle
    {
        public WaterFullPlotStyle()
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
