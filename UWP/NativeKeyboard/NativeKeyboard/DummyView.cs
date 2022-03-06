using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace NativeKeyboardUWP
{
	public class DummyView: TextBox
	{
		private bool pressingEnter;

		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			if(pressingEnter) //Workaround for this event getting called twice when pressing Enter key
			{
				e.Handled = true;
				return;
			}

			base.OnKeyDown(e);

			if(e.Key == Windows.System.VirtualKey.Enter)
			{
				pressingEnter = true;
			}
		}

		protected override void OnKeyUp(KeyRoutedEventArgs e)
		{
			base.OnKeyUp(e);

			if(e.Key == Windows.System.VirtualKey.Enter)
			{
				pressingEnter = false;
			}
		}
	}
}