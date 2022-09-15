using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using unotest.Model;

namespace unotest.Behavoirs
{
    public class WaveChartViewOxyBehavoir : Behavior<WaveChartViewOxy>
    {

        public Action<WavSampleData> SetDataAction
        {
            get { return (Action<WavSampleData>)GetValue(SetDataActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }

        public static readonly DependencyProperty SetDataActionProperty =
                DependencyProperty.Register("SetDataAction", typeof(Action<WavSampleData>), typeof(WaveChartViewOxyBehavoir), null);



        public Action ClearDataAction
        {
            get { return (Action)GetValue(ClearDataActionProperty); }
            set { SetValue(ClearDataActionProperty, value); }
        }

        public static readonly DependencyProperty ClearDataActionProperty =
               DependencyProperty.Register("ClearDataAction", typeof(Action), typeof(WaveChartViewOxyBehavoir), null);


        protected override void OnAttached()
        {
            SetValue(SetDataActionProperty, (Action<WavSampleData>)AssociatedObject.AddData);
            SetValue(ClearDataActionProperty, (Action)AssociatedObject.Clear);

            base.OnAttached();
        }

    }
}
