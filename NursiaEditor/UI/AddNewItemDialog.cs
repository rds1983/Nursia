using Myra.Events;
using Myra.Graphics2D.UI;

namespace NursiaEditor.UI
{
	public partial class AddNewItemDialog : Dialog
	{
		public string ItemName => _textName.Text;

		public int? SelectedIndex
		{
			get
			{
				int? result = null;
				for(var i = 0; i < _itemsPanel.Widgets.Count; ++i)
				{
					var radio = (RadioButton)_itemsPanel.Widgets[i];
					if (radio.IsPressed)
					{
						result = i;
						break;
					}
				}

				return result;
			}
		}

		public AddNewItemDialog()
		{
			BuildUI();

			_textName.TextChanged += _textName_TextChanged;

			_itemsPanel.Widgets.Clear();

			UpdateEnabled();
		}

		public int AddItem(string text)
		{
			var radio = new RadioButton
			{
				Content = new Label
				{
					Text = text
				}
			};

			var result = _itemsPanel.Widgets.Count;
			_itemsPanel.Widgets.Add(radio);

			if (result == 0)
			{
				radio.IsPressed = true;
			}

			return result;
		}

		private void _textName_TextChanged(object sender, ValueChangedEventArgs<string> e)
		{
			UpdateEnabled();
		}

		public void UpdateEnabled()
		{
			ButtonOk.Enabled = !string.IsNullOrEmpty(_textName.Text);
		}
	}
}