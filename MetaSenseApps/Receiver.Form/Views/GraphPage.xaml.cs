using System;
using System.Diagnostics;
using NodeLibrary;
using Xamarin.Forms;

namespace Receiver.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public partial class GraphPage : ContentPage
    {
        public GraphPage()
        {
            InitializeComponent();
            //var vm = BindingContext as GraphViewModel;
            //Debug.Assert(vm != null, "vm != null");
            //vm.NavigationEvent += VmNavigationEvent;
        }
        //private void VmNavigationEvent(object sender, EventArgs eventArgs)
        //{
        //    try
        //    {
        //        Navigation.PopAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e);
        //    }
        //}

    }
}
