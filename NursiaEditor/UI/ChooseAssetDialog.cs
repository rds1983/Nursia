using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NursiaEditor.UI
{
	public partial class ChooseAssetDialog
	{
		public string SelectedId => _textName.Text;

		public string FilePath
		{
			get
			{
				if (_comboModels.SelectedItem == null)
				{
					return null;
				}

				return (string)_comboModels.SelectedItem.Tag;
			}
		}

		public ChooseAssetDialog(string assetFolder, string[] assetExtensions)
		{
			BuildUI();

			if (!Directory.Exists(assetFolder))
			{
				throw new Exception($"Could not find folder {assetFolder} that is supposed to have project's assets.");
			}

			var allFiles = Directory.GetFiles(assetFolder, "*.*", SearchOption.AllDirectories);

			var models = new List<string>();

			foreach (var f in allFiles)
			{
				foreach (var ae in assetExtensions)
				{
					if (f.EndsWith("." + ae, StringComparison.OrdinalIgnoreCase))
					{
						models.Add(f); 
						break;
					}
				}
			}

			if (models.Count == 0)
			{
				var ext = string.Join('/', assetExtensions);
				throw new Exception($"Folder {assetFolder} contains no asset({ext}) files.");
			}

			_comboModels.Widgets.Clear();
			foreach (var model in models)
			{
				var label = new Label
				{
					Text = Path.GetFileName(model),
					Tag = model
				};

				_comboModels.Widgets.Add(label);
			}

			_comboModels.SelectedIndexChanged += (s, a) => UpdateEnabled();

			UpdateEnabled();
		}

		public void UpdateEnabled()
		{
			ButtonOk.Enabled = _comboModels.SelectedIndex != null;
		}
	}
}