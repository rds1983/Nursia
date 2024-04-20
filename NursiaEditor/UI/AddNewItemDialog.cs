using Myra.Events;
using Myra.Graphics2D.UI;

namespace NursiaEditor.UI
{
	public partial class AddNewItemDialog: Dialog
	{
		public string ItemName => _textName.Text;

		public AddNewItemDialog()
		{
			BuildUI();

			_textName.TextChanged += _textName_TextChanged;

			UpdateEnabled();
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