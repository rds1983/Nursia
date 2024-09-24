using System;
using System.IO;

namespace NursiaEditor
{
	internal class TabInfo
	{
		private bool _dirty;

		public string FilePath { get; }
		public bool Dirty
		{
			get => _dirty;

			set
			{
				if (value == _dirty)
				{
					return;
				}

				_dirty = value;
				TitleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public string Title
		{
			get
			{
				var title = Path.GetFileName(FilePath);

				if (Dirty)
				{
					title += " *";
				}

				return title;
			}
		}

		public event EventHandler TitleChanged;

		public TabInfo(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			FilePath = filePath;
		}
	}
}
