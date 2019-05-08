using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(Button), typeof(Receiver.Droid.CircularButtonRenderer))]

namespace Receiver.Droid
{
    /// <summary>
    /// Custom renderer to force Android to use the Xamarin.Forms.Platform.Android button renderer
    /// so that border radius works.
    /// </summary>
    public class CircularButtonRenderer : ButtonRenderer
    {
        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
        }
    }
}