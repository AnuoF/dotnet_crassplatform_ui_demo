using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using unotest.Model;

namespace unotest.Behavoirs
{
    public class SpectrumViewOxyBehavoir : Behavior<SpectrumViewOxy>
    {

        public Action<SpectrumData> SetDataAction
        {
            get { return (Action<SpectrumData>)GetValue(SetDataActionProperty); }
            set { SetValue(SetDataActionProperty, value); }
        }

        public static readonly DependencyProperty SetDataActionProperty =
                DependencyProperty.Register("SetDataAction", typeof(Action<SpectrumData>), typeof(SpectrumViewOxy), null);


        public Action ClearDataAction
        {
            get { return (Action)GetValue(ClearDataActionProperty); }
            set { SetValue(ClearDataActionProperty, value); }
        }

        public static readonly DependencyProperty ClearDataActionProperty =
               DependencyProperty.Register("ClearDataAction", typeof(Action), typeof(SpectrumViewOxy), null);


        protected override void OnAttached()
        {
            SetValue(SetDataActionProperty, (Action<SpectrumData>)AssociatedObject.AddData);
            SetValue(ClearDataActionProperty, (Action)AssociatedObject.Clear);

            base.OnAttached();
        }

    }
}
