using System.Collections.Generic;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;

namespace Nursia.Samples.LevelEditor.UI
{
	public partial class MainForm
	{
		private const int LibraryButtonsPerRow = 2;

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
		private List<InstrumentButton> _allButtons = new List<InstrumentButton>();

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

			_topSplitPane.SetSplitterPosition(0, 0.25f);
			_topSplitPane.SetSplitterPosition(1, 0.7f);

			_sliderTerrainPower.ValueChanged += (s, a) => UpdateTerrainPower();

			RefreshLibrary();
			UpdateTerrainPower();
		}

		private void UpdateTerrainPower()
		{
			_labelTerrainPower.Text = _sliderTerrainPower.Value.ToString();
			_sceneWidget.Instrument.Power = _sliderTerrainPower.Value;
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

		private void AddButton(Grid container, InstrumentButton button)
		{
			var pos = container.Widgets.Count;
			button.GridRow = pos / LibraryButtonsPerRow;
			button.GridColumn = pos % LibraryButtonsPerRow;

			container.Widgets.Add(button);
		}

		private void RefreshLibrary()
		{
			_allButtons.Clear();
			_gridTerrainLibrary.Widgets.Clear();

			_gridTerrainLibrary.ColumnsProportions.Clear();
			for(var i = 0; i < LibraryButtonsPerRow; i++)
			{
				_gridTerrainLibrary.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1.0f));
			}

			var raiseButton = new InstrumentButton(_allButtons)
			{
				Text = "Raise",
			};

			raiseButton.PressedChanged += (s, a) =>
			{
				if (!raiseButton.IsPressed)
				{
					return;
				}

				_sceneWidget.Instrument.Type = InstrumentType.RaiseTerrain;
			};
			AddButton(_gridTerrainLibrary, raiseButton);

			var lowerButton = new InstrumentButton(_allButtons)
			{
				Text = "Lower",
			};

			lowerButton.PressedChanged += (s, a) =>
			{
				if (!lowerButton.IsPressed)
				{
					return;
				}

				_sceneWidget.Instrument.Type = InstrumentType.LowerTerrain;
			};

			AddButton(_gridTerrainLibrary, lowerButton);
		}
	}
}