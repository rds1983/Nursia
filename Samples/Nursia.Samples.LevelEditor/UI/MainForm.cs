using System.Collections.Generic;
using Myra.Graphics2D.TextureAtlases;
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
				RefreshLibrary();
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

			_sliderTerrainRadius.ValueChanged += (s, a) => UpdateTerrainRadius();
			_sliderTerrainPower.ValueChanged += (s, a) => UpdateTerrainPower();

			UpdateTerrainRadius();
			UpdateTerrainPower();
		}

		private void UpdateTerrainRadius()
		{
			_labelTerrainRadius.Text = $"Radius: {_sliderTerrainRadius.Value}";
			_sceneWidget.Instrument.Radius = _sliderTerrainRadius.Value;
		}

		private void UpdateTerrainPower()
		{
			_labelTerrainPower.Text = $"Power: {_sliderTerrainPower.Value}";
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
				Tag = Scene.DirectLights[0]
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
			button.Height = 100;

			if (button.Image != null)
			{
				button.ImageWidth = 75;
				button.ImageHeight = 75;
				button.ImageTextSpacing = 4;
			}

			container.Widgets.Add(button);
		}

		public void RefreshLibrary()
		{
			_allButtons.Clear();
			_gridTerrainLibrary.Widgets.Clear();
			_gridModelsLibrary.Widgets.Clear();

			if (Scene == null || Scene.Terrain == null)
			{
				return;
			}

			var raiseButton = new InstrumentButton(_allButtons)
			{
				Text = "Raise",
			};

			raiseButton.TouchDown += (s, a) => _sceneWidget.Instrument.Type = InstrumentType.RaiseTerrain;

			AddButton(_gridTerrainLibrary, raiseButton);

			var lowerButton = new InstrumentButton(_allButtons)
			{
				Text = "Lower",
			};

			lowerButton.TouchDown += (s, a) => _sceneWidget.Instrument.Type = InstrumentType.LowerTerrain;

			AddButton(_gridTerrainLibrary, lowerButton);

			var waterButton = new InstrumentButton(_allButtons)
			{
				Text = "Water",
			};

			waterButton.TouchDown += (s, a) => _sceneWidget.Instrument.Type = InstrumentType.Water;

			AddButton(_gridTerrainLibrary, waterButton);

			var terrain = Scene.Terrain;
			if (terrain.TexturePaint1 != null)
			{
				var texturePaintButton = new InstrumentButton(_allButtons)
				{
					Text = terrain.TexturePaintName1,
					Image = new TextureRegion(terrain.TexturePaint1)
				};

				texturePaintButton.TouchDown += (s, a) =>
				{
					_sceneWidget.Instrument.Type = InstrumentType.PaintTexture1;
				};

				AddButton(_gridTerrainLibrary, texturePaintButton);
			}

			if (terrain.TexturePaint2 != null)
			{
				var texturePaintButton = new InstrumentButton(_allButtons)
				{
					Text = terrain.TexturePaintName2,
					Image = new TextureRegion(terrain.TexturePaint2)
				};

				texturePaintButton.TouchDown += (s, a) =>
				{
					_sceneWidget.Instrument.Type = InstrumentType.PaintTexture2;
				};

				AddButton(_gridTerrainLibrary, texturePaintButton);
			}

			if (terrain.TexturePaint3 != null)
			{
				var texturePaintButton = new InstrumentButton(_allButtons)
				{
					Text = terrain.TexturePaintName3,
					Image = new TextureRegion(terrain.TexturePaint3)
				};

				texturePaintButton.TouchDown += (s, a) =>
				{
					_sceneWidget.Instrument.Type = InstrumentType.PaintTexture3;
				};

				AddButton(_gridTerrainLibrary, texturePaintButton);
			}

			if (terrain.TexturePaint4 != null)
			{
				var texturePaintButton = new InstrumentButton(_allButtons)
				{
					Text = terrain.TexturePaintName4,
					Image = new TextureRegion(terrain.TexturePaint4)
				};

				texturePaintButton.TouchDown += (s, a) =>
				{
					_sceneWidget.Instrument.Type = InstrumentType.PaintTexture4;
				};

				AddButton(_gridTerrainLibrary, texturePaintButton);
			}

			foreach(var pair in ModelStorage.Models)
			{
				var modelButton = new InstrumentButton(_allButtons)
				{
					Text = pair.Key
				};

				modelButton.TouchDown += (s, a) =>
				{
					_sceneWidget.Instrument.Type = InstrumentType.Model;
					_sceneWidget.Instrument.Model = pair.Value;
				};

				AddButton(_gridModelsLibrary, modelButton);
			}
		}
	}
}