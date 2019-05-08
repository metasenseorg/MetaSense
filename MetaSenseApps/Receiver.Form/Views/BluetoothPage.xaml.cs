using System;
using System.Diagnostics;
using NodeLibrary;
using Xamarin.Forms;
using Receiver.ViewModels;

namespace Receiver.Views
{
    public partial class BluetoothPage : ContentPage
    {
        //private readonly BluetoothPageViewModel _vm;
        public BluetoothPage()
        {
            InitializeComponent();
            //_vm = BindingContext as BluetoothViewModel;
            //Debug.Assert(_vm != null, "_vm != null");
            //_vm.NavigationEvent += NavigationEventHandler;
        }
        //protected void NavigationEventHandler(object source, string args)
        //{
        //    try
        //    {
        //        if (args.Equals("pop"))
        //            Navigation.PopAsync();
        //    } 
        //    catch(Exception e)
        //    {
        //        Log.Error(e);
        //    }
        //}
        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    _vm.InitScanning();
        //}
        //protected override void OnDisappearing()
        //{
        //    base.OnDisappearing();
        //    _vm.EndScanning();
        //}
    }
}
