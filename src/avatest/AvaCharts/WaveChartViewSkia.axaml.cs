using AvaCharts.Model;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace AvaCharts
{
    public partial class WaveChartViewSkia : UserControl
    {
        //Our render target we compile everything to and present to the user
        private RenderTargetBitmap RenderTarget;
        private ISkiaDrawingContextImpl SkiaContext;


        private SKColor _backgroundColor;
        private int _marginTop;
        private int _marginBottom;
        private int _marginLeft;
        private int _marginRight;
        private int _gridCount;
        private SKColor _gridColor;
        private SKColor _lineColor;
        private int _maxValue;
        private int _minValue;

        private int _scaleLineLength;        // Y轴刻度长度 
        private SKColor _axisLineColor;
        private int _offsetY = 0;            // Y轴拖动时的Y轴偏移值
        private int _zoomOffsetY = 0;        // Y轴缩放时的Y轴偏移值

        private List<float> _levels;
        private int MAX_LEN = 10000;
        private object _dataLock = new object();

        private bool _isMouseDown = false;

        private Avalonia.Point _downPoint;
        private HandleAreaType _areaType = HandleAreaType.None;
        private float _zoomFactor = 5.0f;


        public WaveChartViewSkia()
        {
            InitializeComponent();

            _backgroundColor = new SKColor(0, 0, 0);
            _marginTop = 10;
            _marginBottom = 10;
            _marginLeft = 50;
            _marginRight = 10;
            _scaleLineLength = 5;
            _axisLineColor = new SKColor(255, 0, 0);
            _gridColor = new SKColor(0, 255, 0);
            _lineColor = new SKColor(7, 208, 220);
            _gridCount = 10;
            _levels = new List<float>();
            _maxValue = 1000;
            _minValue = -1000;

            PointerPressed += WaveChartViewSkia_PointerPressed;   // MouseDown
            PointerReleased += WaveChartViewSkia_PointerReleased; // MouseUp
            PointerMoved += WaveChartViewSkia_PointerMoved;       // MouseMove
            PointerLeave += WaveChartViewSkia_PointerLeave;       // MouseLeave
            PointerWheelChanged += WaveChartViewSkia_PointerWheelChanged;

            LostFocus += WaveChartViewSkia_LostFocus;
        }

        public void SetData(WavSampleData data)
        {
            int count = 1000;
            short[] buff;
            if (data.SampleData.Length > count)
            {
                buff = new short[count];
                Array.Copy(data.SampleData, buff, count);
            }
            else
            {
                buff = new short[data.SampleData.Length];
                Array.Copy(data.SampleData, buff, buff.Length);
            }

            lock (_dataLock)
            {
                if (_levels.Count > MAX_LEN - buff.Length)
                {
                    _levels.RemoveRange(0, buff.Length);
                }

                for (int i = 0; i < buff.Length; i++)
                {
                    _levels.Add(buff[i]);
                }
            }

            InvalidateVisual();
        }

        public void Clear()
        {
            lock (_dataLock)
            {
                _levels.Clear();
            }

            InvalidateVisual();
        }

        // MouseWheel
        private void WaveChartViewSkia_PointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            var zoomFactor = (int)((Bounds.Height - _marginTop - _marginBottom) / _zoomFactor);

            // 每次放大或缩小当前高度的 _zoomFactor

            if (e.Delta.Y > 0)  // 放大
            {
                _maxValue -= zoomFactor;
                _minValue += zoomFactor;
            }
            else if (e.Delta.Y < 0) // 缩小
            {
                _maxValue += zoomFactor;
                _minValue -= zoomFactor;
            }

            InvalidateVisual();
        }

        private void WaveChartViewSkia_LostFocus(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _isMouseDown = false;
        }

        // MouseLeave
        private void WaveChartViewSkia_PointerLeave(object sender, PointerEventArgs e)
        {
            if (!this.IsFocused)
                _isMouseDown = false;
        }

        // MouseMove
        private void WaveChartViewSkia_PointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isMouseDown || _areaType != HandleAreaType.YAxis)
                return;

            var currentPoint = e.GetPosition(this);
            float spanY = (float)(currentPoint.Y - _downPoint.Y);
            // 实际在屏幕上移动的距离，映射到坐标轴Y上的距离
            int spanScale = (int)(spanY / ((Bounds.Height - _marginTop - _marginBottom) / Math.Abs(_maxValue - _minValue)));
            if (spanScale != 0)
            {
                _offsetY = spanScale;
                InvalidateVisual();
            }
        }

        // MouseDown
        private void WaveChartViewSkia_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            _downPoint = e.GetPosition(this);
            _areaType = CalculateAreaType(_downPoint);
            _isMouseDown = true;
        }

        // MouseUp
        private void WaveChartViewSkia_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _isMouseDown = false;
            _maxValue += _offsetY;
            _minValue += _offsetY;
            _offsetY = 0;
        }

        public override void Render(DrawingContext context)
        {
            Draw();
            if (RenderTarget != null)
                context.DrawImage(RenderTarget, new Avalonia.Rect(0, 0, RenderTarget.PixelSize.Width, RenderTarget.PixelSize.Height), new Avalonia.Rect(0, 0, Bounds.Width, Bounds.Height));
        }

        private void Draw()
        {
            if (SkiaContext == null || RenderTarget.PixelSize.Width != (int)Bounds.Width || RenderTarget.PixelSize.Height != (int)Bounds.Height)
            {
                RenderTarget?.Dispose();
                RenderTarget = null;

                RenderTarget = new RenderTargetBitmap(new Avalonia.PixelSize((int)Bounds.Width, (int)Bounds.Height), new Avalonia.Vector(96, 96));

                var context = RenderTarget.CreateDrawingContext(null);
                SkiaContext = (context as ISkiaDrawingContextImpl);
            }

            SkiaContext.SkCanvas.Clear(_backgroundColor);

            DrawAxis();
            DrawLevel();
        }

        private void DrawAxis()
        {
            int scaleHeight = (int)Bounds.Height - _marginTop - _marginBottom;   // 数据区总高度
            int scaleWidth = (int)Bounds.Width - _marginLeft - _marginRight - _scaleLineLength;  // 数据区总宽度

            float perScaleHeight = scaleHeight / (float)_gridCount;
            float perScaleWidth = scaleWidth / (float)_gridCount;
            int maxValue = _maxValue + _offsetY - _zoomOffsetY;
            int minValue = _minValue + _offsetY + _zoomOffsetY;

            for (int i = 0; i <= _gridCount; i++)
            {
                int height = (int)(i * perScaleHeight) + _marginTop;
                int width = _marginLeft + _scaleLineLength + (int)(i * perScaleWidth);

                int startWidth = _marginLeft;
                int textHeight = 0;

                if ((i + 1) % 2 != 0)
                {
                    if (i == 0)
                        textHeight += 10;
                    else if (i == _gridCount)
                    {

                    }
                    else
                        textHeight += 10 / 2;

                    int scaleValue = maxValue - i * (maxValue - minValue) / _gridCount;
                    var axisText = scaleValue.ToString();
                    using (var pp = new SKPaint() { Color = _axisLineColor })
                    {
                        var bounds = new SKRect();
                        pp.MeasureText(axisText, ref bounds);
                        SkiaContext.SkCanvas.DrawText(axisText, startWidth - bounds.Width, height + textHeight, pp);
                    }
                }
                else
                {
                    startWidth += _scaleLineLength;
                }

                using (var paint = new SKPaint() { Color = _gridColor })
                {
                    // 画横轴
                    SkiaContext.SkCanvas.DrawLine(startWidth, height, (int)Bounds.Width - _marginRight, height, paint);
                    // 画纵轴
                    SkiaContext.SkCanvas.DrawLine(width, _marginTop, width, (int)Bounds.Height - _marginBottom, paint);
                }
            }
        }

        private void DrawLevel()
        {
            if (_levels.Count <= 0)
                return;

            float[] levels = _levels.ToArray();

            using (SKPath path = new SKPath())
            using (var paint = new SKPaint()
            {
                Color = _lineColor,
                StrokeJoin = SKStrokeJoin.Round,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            })
            {
                int maxValue = _maxValue + _offsetY - _zoomOffsetY;
                int minValue = _minValue + _offsetY + _zoomOffsetY;
                float perHeight = ((int)Bounds.Height - _marginTop - _marginBottom) / (float)Math.Abs(maxValue - minValue);
                float perWidth = ((int)Bounds.Width - _marginLeft - _scaleLineLength - _marginRight) / (float)MAX_LEN;
                for (int i = 0; i < levels.Length; i++)
                {
                    float level = levels[i];
                    int x = _marginLeft + _scaleLineLength + (int)(i * perWidth);
                    int y = _marginTop + (int)((maxValue - level) * perHeight);
                    if (i == 0)
                        path.MoveTo(x, y);
                    else
                        path.LineTo(x, y);
                }

                SkiaContext.SkCanvas.DrawPath(path, paint);
            }
        }


        private HandleAreaType CalculateAreaType(Avalonia.Point point)
        {
            // 这里只需要判断是否在Y轴上即可
            if (point.X < (_marginLeft + _scaleLineLength)
                && (point.Y > _marginTop && point.Y < (Bounds.Height - _marginBottom)))
                return HandleAreaType.YAxis;
            else
                return HandleAreaType.None;
        }

    }

    /// <summary>
    /// 鼠标作用的区域
    /// </summary>
    public enum HandleAreaType
    {
        /// <summary>
        /// 
        /// </summary>
        None,

        /// <summary>
        /// Y轴
        /// </summary>
        YAxis,

        /// <summary>
        /// X轴
        /// </summary>
        XAxis,

        /// <summary>
        /// 数据区
        /// </summary>
        DataArea
    }

}
