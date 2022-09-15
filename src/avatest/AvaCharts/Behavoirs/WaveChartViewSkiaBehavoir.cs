using AvaCharts.Model;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using System;

namespace AvaCharts.Behavoirs
{
    public class WaveChartViewSkiaBehavoir : Behavior<WaveChartViewSkia>
    {
        public Action<WavSampleData> SetDataAction
        {
            get { return GetValue(SetDataActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }

        public static readonly StyledProperty<Action<WavSampleData>> SetDataActionProperty =
            AvaloniaProperty.Register<WaveChartViewSkia, Action<WavSampleData>>(nameof(SetDataAction));


        public Action ClearDataAction
        {
            get { return GetValue(ClearDataActionProperty); }
            set { SetValue(ClearDataActionProperty, value); }
        }

        public static readonly StyledProperty<Action> ClearDataActionProperty =
            AvaloniaProperty.Register<WaveChartViewSkia, Action>(nameof(ClearDataAction));

        protected override void OnAttached()
        {
            SetValue(SetDataActionProperty, (Action<WavSampleData>)AssociatedObject.SetData);
            SetValue(ClearDataActionProperty, (Action)AssociatedObject.Clear);

            base.OnAttached();
        }
    }
}
