using System.Windows.Input;
using Xamarin.Forms;

namespace Receiver
{
    public static class AttachCommandsToListView
    {
        public static readonly BindableProperty ItemTappedProperty =
            BindableProperty.CreateAttached(
                "Command",
                typeof(ICommand),
                typeof(AttachCommandsToListView),
                null,
                propertyChanged: OnItemTappedCommandChanged);
        public static readonly BindableProperty ItemSelectedProperty =
            BindableProperty.CreateAttached(
                "Command",
                typeof(ICommand),
                typeof(AttachCommandsToListView),
                null,
                propertyChanged: OnItemSelectedCommandChanged);
        public static readonly BindableProperty RefreshingProperty =
            BindableProperty.CreateAttached(
                "Command",
                typeof(ICommand),
                typeof(AttachCommandsToListView),
                null,
                propertyChanged: OnRefreshingCommandChanged);
        static void OnItemTappedCommandChanged(BindableObject view, object oldValue, object newValue)
        {
            var entry = view as ListView;
            if (entry == null)
                return;

            entry.ItemTapped += (sender, e) =>
            {
                var command = (newValue as ICommand);
                if (command == null)
                    return;

                if (command.CanExecute(e.Item))
                {
                    command.Execute(e.Item);
                }

            };
        }
        static void OnItemSelectedCommandChanged(BindableObject view, object oldValue, object newValue)
        {
            var entry = view as ListView;
            if (entry == null)
                return;

            entry.ItemSelected += (sender, e) =>
            {
                var command = (newValue as ICommand);
                if (command == null)
                    return;

                if (command.CanExecute(e.SelectedItem))
                {
                    command.Execute(e.SelectedItem);
                }

            };
        }
        static void OnRefreshingCommandChanged(BindableObject view, object oldValue, object newValue)
        {
            var entry = view as ListView;
            if (entry == null)
                return;

            entry.Refreshing += (sender, e) =>
            {
                var command = (newValue as ICommand);
                if (command == null)
                    return;

                if (command.CanExecute(entry))
                {
                    command.Execute(entry);
                }

            };
        }
    }
}
