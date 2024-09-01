/* Generated by MyraPad at 9/1/2024 9:03:19 PM */
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI.Properties;
using FontStashSharp.RichText;
using AssetManagementBase;

#if STRIDE
using Stride.Core.Mathematics;
#elif PLATFORM_AGNOSTIC
using System.Drawing;
using System.Numerics;
using Color = FontStashSharp.FSColor;
#else
// MonoGame/FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace NursiaEditor.UI
{
	partial class MainForm: VerticalStackPanel
	{
		private void BuildUI()
		{
			_menuItemNew = new MenuItem();
			_menuItemNew.Text = "&New";
			_menuItemNew.Id = "_menuItemNew";

			_menuItemOpen = new MenuItem();
			_menuItemOpen.Text = "&Open";
			_menuItemOpen.Id = "_menuItemOpen";

			_menuItemSave = new MenuItem();
			_menuItemSave.Text = "&Save";
			_menuItemSave.Id = "_menuItemSave";

			_menuItemSaveAs = new MenuItem();
			_menuItemSaveAs.Text = "&Save As";
			_menuItemSaveAs.Id = "_menuItemSaveAs";

			_menuItemQuit = new MenuItem();
			_menuItemQuit.Text = "&Quit";
			_menuItemQuit.Id = "_menuItemQuit";

			var menuItem1 = new MenuItem();
			menuItem1.Text = "&File";
			menuItem1.Items.Add(_menuItemNew);
			menuItem1.Items.Add(_menuItemOpen);
			menuItem1.Items.Add(_menuItemSave);
			menuItem1.Items.Add(_menuItemSaveAs);
			menuItem1.Items.Add(_menuItemQuit);

			_menuItemInsertLight = new MenuItem();
			_menuItemInsertLight.Text = "&Light...";
			_menuItemInsertLight.Id = "_menuItemInsertLight";

			_menuItemInsertPrimitive = new MenuItem();
			_menuItemInsertPrimitive.Text = "Geometric &Primitive...";
			_menuItemInsertPrimitive.Id = "_menuItemInsertPrimitive";

			_menuItemInsertGltfModel = new MenuItem();
			_menuItemInsertGltfModel.Text = "&Gltf/Glb Model...";
			_menuItemInsertGltfModel.Id = "_menuItemInsertGltfModel";

			_menuItemInsertSkybox = new MenuItem();
			_menuItemInsertSkybox.Text = "&Skybox...";
			_menuItemInsertSkybox.Id = "_menuItemInsertSkybox";

			var menuItem2 = new MenuItem();
			menuItem2.Text = "&Insert Scene Node";
			menuItem2.Items.Add(_menuItemInsertLight);
			menuItem2.Items.Add(_menuItemInsertPrimitive);
			menuItem2.Items.Add(_menuItemInsertGltfModel);
			menuItem2.Items.Add(_menuItemInsertSkybox);

			_menuItemAbout = new MenuItem();
			_menuItemAbout.Text = "&About";
			_menuItemAbout.Id = "_menuItemAbout";

			var menuItem3 = new MenuItem();
			menuItem3.Text = "&Help";
			menuItem3.Items.Add(_menuItemAbout);

			var horizontalMenu1 = new HorizontalMenu();
			horizontalMenu1.Items.Add(menuItem1);
			horizontalMenu1.Items.Add(menuItem2);
			horizontalMenu1.Items.Add(menuItem3);

			_panelSceneExplorer = new Panel();
			_panelSceneExplorer.Id = "_panelSceneExplorer";

			_panelFileSystem = new Panel();
			_panelFileSystem.Id = "_panelFileSystem";

			_leftSplitPane = new VerticalSplitPane();
			_leftSplitPane.Id = "_leftSplitPane";
			_leftSplitPane.Widgets.Add(_panelSceneExplorer);
			_leftSplitPane.Widgets.Add(_panelFileSystem);

			_labelFps = new Label();
			_labelFps.Text = "FPS: 60.0";
			_labelFps.Id = "_labelFps";

			_labelMeshes = new Label();
			_labelMeshes.Text = "Meshes: 15";
			_labelMeshes.Id = "_labelMeshes";

			_labelCamera = new Label();
			_labelCamera.Text = "Camera: 0/50/0;270;0";
			_labelCamera.Id = "_labelCamera";

			var verticalStackPanel1 = new VerticalStackPanel();
			verticalStackPanel1.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
			verticalStackPanel1.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Bottom;
			verticalStackPanel1.Widgets.Add(_labelFps);
			verticalStackPanel1.Widgets.Add(_labelMeshes);
			verticalStackPanel1.Widgets.Add(_labelCamera);

			_panelScene = new Panel();
			_panelScene.Id = "_panelScene";
			_panelScene.Widgets.Add(verticalStackPanel1);

			_propertyGrid = new PropertyGrid();
			_propertyGrid.Id = "_propertyGrid";

			_topSplitPane = new HorizontalSplitPane();
			_topSplitPane.Id = "_topSplitPane";
			_topSplitPane.Widgets.Add(_leftSplitPane);
			_topSplitPane.Widgets.Add(_panelScene);
			_topSplitPane.Widgets.Add(_propertyGrid);

			var panel1 = new Panel();
			StackPanel.SetProportionType(panel1, Myra.Graphics2D.UI.ProportionType.Fill);
			panel1.Widgets.Add(_topSplitPane);

			
			Id = "_mainPanel";
			Widgets.Add(horizontalMenu1);
			Widgets.Add(panel1);
		}

		
		public MenuItem _menuItemNew;
		public MenuItem _menuItemOpen;
		public MenuItem _menuItemSave;
		public MenuItem _menuItemSaveAs;
		public MenuItem _menuItemQuit;
		public MenuItem _menuItemInsertLight;
		public MenuItem _menuItemInsertPrimitive;
		public MenuItem _menuItemInsertGltfModel;
		public MenuItem _menuItemInsertSkybox;
		public MenuItem _menuItemAbout;
		public Panel _panelSceneExplorer;
		public Panel _panelFileSystem;
		public VerticalSplitPane _leftSplitPane;
		public Label _labelFps;
		public Label _labelMeshes;
		public Label _labelCamera;
		public Panel _panelScene;
		public PropertyGrid _propertyGrid;
		public HorizontalSplitPane _topSplitPane;
	}
}