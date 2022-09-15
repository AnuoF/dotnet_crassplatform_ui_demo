using AvaCharts.Model;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaCharts.Behavoirs
{
    public class SpectrumViewBehavoir : Behavior<SpectrumView>
    {
        public Action<SpectrumData> SetDataAction
        {
            get { return GetValue(SetDataActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }
        public static readonly StyledProperty<Action<SpectrumData>> SetDataActionProperty =
            AvaloniaProperty.Register<SpectrumView, Action<SpectrumData>>(nameof(SetDataAction));

        public Action<int> AddZoneAction
        {
            get { return GetValue(AddZoneActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }
        public static readonly StyledProperty<Action<int>> AddZoneActionProperty =
            AvaloniaProperty.Register<SpectrumView, Action<int>>(nameof(AddZoneAction));

        public Action ClearAction
        {
            get { return GetValue(ClearActionProperty); }
            set { SetValue(ClearActionProperty, value); }
        }
        public static readonly StyledProperty<Action> ClearActionProperty =
            AvaloniaProperty.Register<SpectrumView, Action>(nameof(ClearAction));

        protected override void OnAttached()
        {
            SetValue(SetDataActionProperty, AssociatedObject.AddData);
            SetValue(ClearActionProperty, AssociatedObject.Clear);
            SetValue(AddZoneActionProperty, AssociatedObject.AddZone);

            base.OnAttached();
        }

    }
}
