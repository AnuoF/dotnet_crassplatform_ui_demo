// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Series.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Abstract base class for series.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using Avalonia;

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Controls;
    using global::Avalonia.Media;
    using global::Avalonia.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Abstract base class for series.
    /// </summary>
    public abstract class Series : ItemsControl
    {
        /// <summary>
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<Color> ColorProperty = AvaloniaProperty.Register<Series, Color>(nameof(Color), MoreColors.Automatic);

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Series, string>(nameof(Title), null);

         /// <summary>
        /// Identifies the <see cref="RenderInLegend"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<bool> RenderInLegendProperty = AvaloniaProperty.Register<Series, bool>(nameof(RenderInLegend), true);

        /// <summary>
        /// Identifies the <see cref="TrackerFormatString"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<string> TrackerFormatStringProperty = AvaloniaProperty.Register<Series, string>(nameof(TrackerFormatString), null);

        /// <summary>
        /// Identifies the <see cref="TrackerKey"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<string> TrackerKeyProperty = AvaloniaProperty.Register<Series, string>(nameof(TrackerKey), null);

        /// <summary>
        /// Identifies the <see cref="EdgeRenderingMode"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<EdgeRenderingMode> EdgeRenderingModeProperty = AvaloniaProperty.Register<Series, EdgeRenderingMode>(nameof(EdgeRenderingMode), EdgeRenderingMode.Automatic);

        /// <summary>
        /// The event listener used to subscribe to ItemSource.CollectionChanged events
        /// </summary>
        private readonly EventListener eventListener;

        /// <summary>
        /// Initializes static members of the <see cref="Series" /> class.
        /// </summary>
        static Series()
        {
            IsVisibleProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            BackgroundProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            ColorProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            TitleProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            RenderInLegendProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            TrackerFormatStringProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            TrackerKeyProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            EdgeRenderingModeProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Series" /> class.
        /// </summary>
        protected Series()
        {
            eventListener = new EventListener(OnCollectionChanged);

            // Set Items to null for consistency with WPF behaviour in Oxyplot-Contrib
            // Works around issue with BarSeriesManager throwing on empty Items collection in OxyPlot.Core 2.1
            Items = null;
        }

        /// <summary>
        /// Gets or sets Color.
        /// </summary>
        public Color Color
        {
            get
            {
                return GetValue(ColorProperty);
            }

            set
            {
                SetValue(ColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the internal series.
        /// </summary>
        public OxyPlot.Series.Series InternalSeries { get; protected set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title
        {
            get
            {
                return GetValue(TitleProperty);
            }

            set
            {
                SetValue(TitleProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the series should be rendered in the legend.
        /// </summary>
        public bool RenderInLegend
        {
            get
            {
                return GetValue(RenderInLegendProperty);
            }

            set
            {
                SetValue(RenderInLegendProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets TrackerFormatString.
        /// </summary>
        public string TrackerFormatString
        {
            get
            {
                return GetValue(TrackerFormatStringProperty);
            }

            set
            {
                SetValue(TrackerFormatStringProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets TrackerKey.
        /// </summary>
        public string TrackerKey
        {
            get
            {
                return GetValue(TrackerKeyProperty);
            }

            set
            {
                SetValue(TrackerKeyProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="OxyPlot.EdgeRenderingMode"/> for the series.
        /// </summary>
        public EdgeRenderingMode EdgeRenderingMode
        {
            get
            {
                return GetValue(EdgeRenderingModeProperty);
            }

            set
            {
                SetValue(EdgeRenderingModeProperty, value);
            }
        }

        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <returns>A series.</returns>
        public abstract OxyPlot.Series.Series CreateModel();

        /// <summary>
        /// The appearance changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Series)d).OnVisualChanged();
        }

        /// <summary>
        /// The on visual changed handler.
        /// </summary>
        protected void OnVisualChanged()
        {
            (this.Parent as IPlot)?.ElementAppearanceChanged(this);
        }

        /// <summary>
        /// The data changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        protected static void DataChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Series)d).OnDataChanged();
        }

        /// <summary>
        /// The on data changed handler.
        /// </summary>
        protected void OnDataChanged()
        {
            (this.Parent as IPlot)?.ElementDataChanged(this);
        }

        /// <summary>
        /// The on items source changed.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
            SubscribeToCollectionChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
            OnDataChanged();
        }

        protected override void OnAttachedToLogicalTree(global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            //BeginInit();
            //EndInit();
        }

        /// <summary>
        /// Synchronizes the properties.
        /// </summary>
        /// <param name="s">The series.</param>
        protected virtual void SynchronizeProperties(OxyPlot.Series.Series s)
        {
            s.Background = Background.ToOxyColor();
            s.Title = Title;
            s.RenderInLegend = RenderInLegend;
            s.TrackerFormatString = TrackerFormatString;
            s.TrackerKey = TrackerKey;
            s.TrackerFormatString = TrackerFormatString;
            s.IsVisible = IsVisible;
            s.Font = FontFamily.ToString();
            s.TextColor = Foreground.ToOxyColor();
            s.EdgeRenderingMode = EdgeRenderingMode;
        }

        /// <summary>
        /// If the ItemsSource implements INotifyCollectionChanged update the visual when the collection changes.
        /// </summary>
        /// <param name="oldValue">The old ItemsSource</param>
        /// <param name="newValue">The new ItemsSource</param>
        private void SubscribeToCollectionChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged collection)
            {
                WeakSubscriptionManager.Unsubscribe(collection, "CollectionChanged", eventListener);
            }

            collection = newValue as INotifyCollectionChanged;
            if (collection != null)
            {
                WeakSubscriptionManager.Subscribe(collection, "CollectionChanged", eventListener);
            }
        }

        /// <summary>
        /// Invalidate the view when the collection changes
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="notifyCollectionChangedEventArgs">The collection changed args</param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnDataChanged();
        }

        /// <summary>
        /// Listens to and forwards any collection changed events
        /// </summary>
        private class EventListener : IWeakSubscriber<NotifyCollectionChangedEventArgs>
        {
            /// <summary>
            /// The delegate to forward to
            /// </summary>
            private readonly EventHandler<NotifyCollectionChangedEventArgs> onCollectionChanged;

            /// <summary>
            /// Initializes a new instance of the <see cref="EventListener" /> class
            /// </summary>
            /// <param name="onCollectionChanged">The handler</param>
            public EventListener(EventHandler<NotifyCollectionChangedEventArgs> onCollectionChanged)
            {
                this.onCollectionChanged = onCollectionChanged;
            }

            public void OnEvent(object sender, NotifyCollectionChangedEventArgs e)
            {
                onCollectionChanged(sender, e);
            }
        }
    }




    public static class WeakSubscriptionManager
    {
        /// <summary>
        /// Subscribes to an event on an object using a weak subscription.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="target">The event source.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="subscriber">The subscriber.</param>
        public static void Subscribe<TTarget, TEventArgs>(TTarget target, string eventName, IWeakSubscriber<TEventArgs> subscriber)
            where TEventArgs : EventArgs
        {
            var dic = SubscriptionTypeStorage<TEventArgs>.Subscribers.GetOrCreateValue(target);
            Subscription<TEventArgs> sub;

            if (!dic.TryGetValue(eventName, out sub))
            {
                dic[eventName] = sub = new Subscription<TEventArgs>(dic, typeof(TTarget), target, eventName);
            }

            sub.Add(new WeakReference<IWeakSubscriber<TEventArgs>>(subscriber));
        }

        /// <summary>
        /// Unsubscribes from an event.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments.</typeparam>
        /// <param name="target">The event source.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="subscriber">The subscriber.</param>
        public static void Unsubscribe<T>(object target, string eventName, IWeakSubscriber<T> subscriber)
            where T : EventArgs
        {
            SubscriptionDic<T> dic;

            if (SubscriptionTypeStorage<T>.Subscribers.TryGetValue(target, out dic))
            {
                Subscription<T> sub;

                if (dic.TryGetValue(eventName, out sub))
                {
                    sub.Remove(subscriber);
                }
            }
        }

        private static class SubscriptionTypeStorage<T>
            where T : EventArgs
        {
            public static readonly ConditionalWeakTable<object, SubscriptionDic<T>> Subscribers
                = new ConditionalWeakTable<object, SubscriptionDic<T>>();
        }

        private class SubscriptionDic<T> : Dictionary<string, Subscription<T>>
            where T : EventArgs
        {
        }

        private static readonly Dictionary<Type, Dictionary<string, EventInfo>> Accessors
            = new Dictionary<Type, Dictionary<string, EventInfo>>();

        private class Subscription<T> where T : EventArgs
        {
            private readonly EventInfo _info;
            private readonly SubscriptionDic<T> _sdic;
            private readonly object _target;
            private readonly string _eventName;
            private readonly Delegate _delegate;

            private WeakReference<IWeakSubscriber<T>>[] _data = new WeakReference<IWeakSubscriber<T>>[16];
            private int _count = 0;

            public Subscription(SubscriptionDic<T> sdic, Type targetType, object target, string eventName)
            {
                _sdic = sdic;
                _target = target;
                _eventName = eventName;
                Dictionary<string, EventInfo> evDic;
                if (!Accessors.TryGetValue(targetType, out evDic))
                    Accessors[targetType] = evDic = new Dictionary<string, EventInfo>();

                if (!evDic.TryGetValue(eventName, out _info))
                {
                    var ev = targetType.GetRuntimeEvents().FirstOrDefault(x => x.Name == eventName);

                    if (ev == null)
                    {
                        throw new ArgumentException(
                            $"The event {eventName} was not found on {target.GetType()}.");
                    }

                    evDic[eventName] = _info = ev;
                }

                var del = new Action<object, T>(OnEvent);
                _delegate = del.GetMethodInfo().CreateDelegate(_info.EventHandlerType, del.Target);
                _info.AddMethod.Invoke(target, new[] { _delegate });
            }

            void Destroy()
            {
                _info.RemoveMethod.Invoke(_target, new[] { _delegate });
                _sdic.Remove(_eventName);
            }

            public void Add(WeakReference<IWeakSubscriber<T>> s)
            {
                if (_count == _data.Length)
                {
                    //Extend capacity
                    var ndata = new WeakReference<IWeakSubscriber<T>>[_data.Length * 2];
                    Array.Copy(_data, ndata, _data.Length);
                    _data = ndata;
                }
                _data[_count] = s;
                _count++;
            }

            public void Remove(IWeakSubscriber<T> s)
            {
                var removed = false;

                for (int c = 0; c < _count; ++c)
                {
                    var reference = _data[c];
                    IWeakSubscriber<T> instance;

                    if (reference != null && reference.TryGetTarget(out instance) && instance == s)
                    {
                        _data[c] = null;
                        removed = true;
                    }
                }

                if (removed)
                {
                    Compact();
                }
            }

            void Compact()
            {
                int empty = -1;
                for (int c = 0; c < _count; c++)
                {
                    var r = _data[c];
                    //Mark current index as first empty
                    if (r == null && empty == -1)
                        empty = c;
                    //If current element isn't null and we have an empty one
                    if (r != null && empty != -1)
                    {
                        _data[c] = null;
                        _data[empty] = r;
                        empty++;
                    }
                }
                if (empty != -1)
                    _count = empty;
                if (_count == 0)
                    Destroy();
            }

            void OnEvent(object sender, T eventArgs)
            {
                var needCompact = false;
                for (var c = 0; c < _count; c++)
                {
                    var r = _data[c];
                    IWeakSubscriber<T> sub;
                    if (r.TryGetTarget(out sub))
                        sub.OnEvent(sender, eventArgs);
                    else
                        needCompact = true;
                }
                if (needCompact)
                    Compact();
            }
        }
    }
}