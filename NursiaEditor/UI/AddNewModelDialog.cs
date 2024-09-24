using Myra.Graphics2D.UI;
using System;
using System.IO;
using System.Linq;

namespace NursiaEditor.UI
{
	public partial class AddNewModelDialog
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

		public AddNewModelDialog()
		{
			BuildUI();

			var project = StudioGame.MainForm.CurrentProject;
			if (project == null)
			{
				throw new Exception("No project is selected");
			}

			var projectFolder = Path.GetDirectoryName(project.AbsolutePath);
			var modelsFolder = Path.Combine(projectFolder, Constants.ModelsFolder);
			if (!Directory.Exists(modelsFolder))
			{
				throw new Exception($"Could not find folder {modelsFolder} that is supposed to have project's models.");
			}

			var files = Directory.GetFiles(modelsFolder, "*.*", SearchOption.AllDirectories);
			var models = (from p in files
						  where
						  (p.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) ||
						  p.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase))
						  select p).ToList();

			if (models.Count == 0)
			{
				throw new Exception($"Folder {modelsFolder} contains no model(gltf/glb) files.");
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