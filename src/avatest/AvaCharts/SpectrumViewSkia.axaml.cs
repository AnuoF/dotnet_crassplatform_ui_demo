using AvaCharts.Model;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using SkiaSharp;

namespace AvaCharts
{
    public partial class SpectrumViewSkia : UserControl
    {
        private RenderTargetBitmap RenderTarget;
        private ISkiaDrawingContextImpl SkiaContext;

        private SKColor _backgroundColor;
        private int _marginTop;
        private int _marginBottom;
        private int _marginLeft;
        private int _marginRight;
        private int _axisScaleLength;

        public SpectrumViewSkia()
        {
            InitializeComponent();
            _backgroundColor = new SKColor(0, 0, 0);
            _marginTop = 10;
            _marginBottom = 10;
            _marginLeft = 10;
            _marginRight = 10;
            _axisScaleLength = 5;


        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void AddData(SpectrumData data)
        {

        }

        public void AddZone(int count)
        {

        }

        public void Clear()
        {

        }



        public override void Render(DrawingContext context)
        {
            Draw();
            if (RenderTarget != null)
            {
                context.DrawImage(RenderTarget,
                    new Rect(0, 0, RenderTarget.PixelSize.Width, RenderTarget.PixelSize.Height),
                    new Rect(0, 0, Bounds.Width, Bounds.Height));
            }
        }

        private void Draw()
        {
            if (SkiaContext == null
                || RenderTarget.PixelSize.Width != (int)Bounds.Width
                || RenderTarget.PixelSize.Height != (int)Bounds.Height)
            {
                RenderTarget?.Dispose();
                RenderTarget = null;
                RenderTarget = new RenderTargetBitmap(new PixelSize((int)Bounds.Width, (int)Bounds.Height), new Vector(96, 96));

                var context = RenderTarget.CreateDrawingContext(null);
                SkiaContext = (context as ISkiaDrawingContextImpl);
            }

            SkiaContext.SkCanvas.Clear(_backgroundColor);

            DrawAxis();
            DrawLevels();
        }

        private void DrawAxis()
        {

        }

        private void DrawLevels()
        {

        }

    }
}
