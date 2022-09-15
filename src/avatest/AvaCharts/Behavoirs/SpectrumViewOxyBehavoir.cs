using AvaCharts.Model;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaCharts.Behavoirs
{
    public class SpectrumViewOxyBehavoir : Behavior<SpectrumViewOxy>
    {
        public Action<SpectrumData> SetDataAction
        {
            get { return GetValue(SetDataActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }
        public static readonly StyledProperty<Action<SpectrumData>> SetDataActionProperty =
            AvaloniaProperty.Register<SpectrumViewOxy, Action<SpectrumData>>(nameof(SetDataAction));

        public Action<int> AddZoneAction
        {
            get { return GetValue(AddZoneActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }
        public static readonly StyledProperty<Action<int>> AddZoneActionProperty =
            AvaloniaProperty.Register<SpectrumViewOxy, Action<int>>(nameof(AddZoneAction));

        public Action ClearAction
        {
            get { return GetValue(ClearActionProperty); }
            set { SetValue(ClearActionProperty, value); }
        }

        public static readonly StyledProperty<Action> ClearActionProperty =
            AvaloniaProperty.Register<SpectrumViewOxy, Action>(nameof(ClearAction));

        protected override void OnAttached()
        {
            SetValue(SetDataActionProperty, AssociatedObject.AddData);
            SetValue(ClearActionProperty, AssociatedObject.Clear);
            SetValue(AddZoneActionProperty, AssociatedObject.AddZone);

            base.OnAttached();
        }

    }
}
