using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.UI;

namespace Nursia.Samples.LevelEditor.UI
{
	public partial class MainForm
	{
		private readonly PropertyGrid _propertyGrid;
		private readonly SceneWidget _sceneWidget;

		public Scene Scene
		{
			get => _sceneWidget.Scene;
			set
			{
				_sceneWidget.Scene = value;
				RefreshExplorer();
			}
		}

		public ForwardRenderer Renderer { get => _sceneWidget.Renderer; }

		public MainForm()
		{
			BuildUI();

			_propertyGrid = new PropertyGrid();
			_panelProperties.Widgets.Add(_propertyGrid);

			_sceneWidget = new SceneWidget
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};
			_panelScene.Widgets.Add(_sceneWidget);

			_listExplorer.SelectedIndexChanged += _listExplorer_SelectedIndexChanged;

			_topSplitPane.SetSplitterPosition(0, 0.7f);
		}

		private void _listExplorer_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			var list = _listExplorer;
			_propertyGrid.Object = list.SelectedItem.Tag;
		}

		private void RefreshExplorer()
		{
			var list = _listExplorer;
			list.Items.Clear();

			if (Scene == null)
			{
				return;
			}

			// Lights
			list.Items.Add(new ListItem
			{
				Text = "Directional Light",
				Tag = Scene.DirectLight
			});

			// Skybox
			if (Scene.Skybox != null)
			{
				list.Items.Add(new ListItem
				{
					Text = "Skybox",
					Tag = Scene.Skybox
				});
			}

			// Water
			foreach (var water in Scene.WaterTiles)
			{
				list.Items.Add(new ListItem
				{
					Text = "Water",
					Tag = water
				});
			}
		}
	}
}