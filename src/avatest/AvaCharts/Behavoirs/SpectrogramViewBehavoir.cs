using AvaCharts.Model;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaCharts.Behavoirs
{
    public class SpectrogramViewBehavoir : Behavior<SpectrogramView>
    {
        public static readonly StyledProperty<Action<SpectrumData>> SetDataActionProperty =
            AvaloniaProperty.Register<SpectrogramView, Action<SpectrumData>>(nameof(SetDataAction));
        public Action<SpectrumData> SetDataAction
        {
            get { return GetValue(SetDataActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }

        public static readonly StyledProperty<Action> ClearActionProperty =
            AvaloniaProperty.Register<SpectrogramView, Action>(nameof(ClearAction));

        public Action ClearAction
        {
            get { return GetValue(ClearActionProperty); }
            set { SetValue(ClearActionProperty, value); }
        }


        protected override void OnAttached()
        {
            SetValue(SetDataActionProperty, (Action<SpectrumData>)AssociatedObject.AddData);
            SetValue(ClearActionProperty, (Action)AssociatedObject.Clear);

            base.OnAttached();
        }
    }
}
