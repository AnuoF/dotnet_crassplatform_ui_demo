using Avalonia;
using Avalonia.Xaml.Interactivity;
using scottplotcharts.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace scottplotcharts.Behavoirs
{
    public class WaveChartViewBehavior : Behavior<WaveChartView>
    {
        public Action<WavSampleData> SetDataAction
        {
            get { return GetValue(SetDataActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }

        public static readonly StyledProperty<Action<WavSampleData>> SetDataActionProperty =
            AvaloniaProperty.Register<WaveChartView, Action<WavSampleData>>(nameof(SetDataAction));


        public Action ClearDataAction
        {
            get { return GetValue(ClearDataActionProperty); }
            set { SetValue(ClearDataActionProperty, value); }
        }

        public static readonly StyledProperty<Action> ClearDataActionProperty =
            AvaloniaProperty.Register<WaveChartView, Action>(nameof(ClearDataAction));

        protected override void OnAttached()
        {
            SetValue(SetDataActionProperty, (Action<WavSampleData>)AssociatedObject.SetData);
            SetValue(ClearDataActionProperty, (Action)AssociatedObject.Clear);
            base.OnAttached();
        }
    }
}
