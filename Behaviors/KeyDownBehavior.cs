using System.Windows.Input;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Kursach.Behaviors
{
    internal class KeyDownBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register("Command", typeof(ICommand), typeof(KeyDownBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += OnKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyDown -= OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Command != null && Command.CanExecute(e))
            {
                Command.Execute(e);
            }
        }
    }
}
