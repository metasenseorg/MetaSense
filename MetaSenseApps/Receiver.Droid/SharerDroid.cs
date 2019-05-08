using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NodeLibrary;
using NodeLibrary.Native;
using Receiver.Droid;
using Receiver.ViewModels;
using Xamarin.Forms;

[assembly: Dependency(typeof(SharerDroid))]
namespace Receiver.Droid
{
    internal sealed class SharerDroid : ISharer
    {
        private readonly Context _context;
        public SharerDroid() : this(Forms.Context) { }

        /// <summary>
        /// Initialize the context to the specified context.
        /// </summary>
        /// <param name="context">The desired context to start the share intent</param>
        public SharerDroid(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Shares the message by opening a chooser to let user select which app to share the
        /// message on.
        /// </summary>
        /// <param name="message">The AQI message containing information about the message to be
        ///     shared</param>
        public void Share(MetaSenseAQIMessage message)
        {
            Intent shareIntent = new Intent();
            shareIntent.SetAction(Intent.ActionSend);
            shareIntent.PutExtra(Intent.ExtraText, message.ToMediaPost());
            shareIntent.SetType("text/plain");
            // always display chooser
            _context.StartActivity(Intent.CreateChooser(shareIntent, "miAQI"));
        }
    }
}