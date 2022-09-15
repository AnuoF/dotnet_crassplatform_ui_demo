using Avalonia;
using Avalonia.Xaml.Interactivity;
using scottplotcharts.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace scottplotcharts.Behavoirs
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

            base.OnAttached();
        }

    }
}
