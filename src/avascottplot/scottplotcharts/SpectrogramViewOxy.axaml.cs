using Avalonia.Controls;
using OxyPlot;
using OxyPlot.Avalonia;
using OxyPlot.Series;
using System;

namespace scottplotcharts
{
    public partial class SpectrogramViewOxy : UserControl
    {
        private PlotView _mPlot;

        public SpectrogramViewOxy()
        {
            InitializeComponent();

            CreatePlot();
        }







        private void CreatePlot()
        {
            _mPlot = new PlotView();
            _mPlot.Margin = new Avalonia.Thickness(10);

            var myModel = new PlotModel { Title = "Example 1" };
            myModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
            _mPlot.Model = myModel;

            this.gridChart.Children.Add(_mPlot);
        }


    }
}
