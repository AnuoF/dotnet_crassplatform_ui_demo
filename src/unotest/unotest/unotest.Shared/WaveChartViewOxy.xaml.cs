using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using unotest.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
//using Windows.UI.Xaml.Controls;
//using Windows.UI.Xaml.Controls.Primitives;
//using Windows.UI.Xaml.Data;
//using Windows.UI.Xaml.Input;
//using Windows.UI.Xaml.Media;
//using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace unotest
{
    public sealed partial class WaveChartViewOxy : UserControl
    {
        private PlotView _mPlot;
        private PlotModel _mModel;
        private LineSeries _line;

        private ConcurrentQueue<WavSampleData> _dataQueue = new ConcurrentQueue<WavSampleData>();
        private AutoResetEvent _dataEvent = new AutoResetEvent(false);
        private int MaxCount = 10000;

        private double _xIndex = 0;   // X轴起始值 


        public WaveChartViewOxy()
        {
            this.InitializeComponent();

            CreateChart();
            Task.Factory.StartNew(() => { Deal(); });
        }

        public void AddData(WavSampleData data)
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

            _mModel.InvalidatePlot(true);
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
                                //Dispatcher.UIThread.Post(() =>
                                //{
                                _mModel.InvalidatePlot(true);
                                //});
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
            //_mPlot = new PlotView();
            _mPlot = mPlotView;

            _mPlot.Margin = new Microsoft.UI.Xaml.Thickness(30, 0, 0, 30);
            //_mPlot.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));

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

            _mModel.InvalidatePlot(true);

            //this.gridChart.Children.Add(_mPlot);
        }
    }
}
