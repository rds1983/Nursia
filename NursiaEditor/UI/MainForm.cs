using System;
using System.Collections.Generic;
using System.IO;
using AssetManagementBase;
using Microsoft.Build.Construction;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Myra.Graphics2D.UI.Properties;
using Nursia;
using Nursia.Modelling;
using Nursia.Primitives;
using Nursia.Rendering;
using Nursia.Rendering.Lights;
using Nursia.Standard;
using NursiaEditor.Utility;

namespace NursiaEditor.UI
{
	public partial class MainForm
	{
		private string _filePath;
		private bool _explorerTouchDown = false;
		private readonly TreeView _treeFileExplorer, _treeFileSolution;

		public string FilePath
		{
			get => _filePath;

			set
			{
				if (value == _filePath)
				{
					return;
				}

				_filePath = value;
				UpdateTitle();
			}
		}

		public object SelectedObject
		{
			get => _propertyGrid.Object;
			set => _propertyGrid.Object = value;
		}

		public SceneWidget CurrentSceneWidget
		{
			get
			{
				if (_tabControlScenes.SelectedIndex == null)
				{
					return null;
				}

				return (SceneWidget)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Content;
			}
		}

		public Scene CurrentScene => CurrentSceneWidget.Scene;

		public ProjectInSolution CurrentProject => CurrentSceneWidget.Project;

		private readonly List<InstrumentButton> _allButtons = new List<InstrumentButton>();

		public event EventHandler SelectedObjectChanged
		{
			add
			{
				_treeFileExplorer.SelectionChanged += value;
			}

			remove
			{
				_treeFileExplorer.SelectionChanged -= value;
			}
		}


		public MainForm()
		{
			BuildUI();

			_treeFileSolution = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};
			_panelSolution.Widgets.Add(_treeFileSolution);

			_treeFileExplorer = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};
			_panelSceneExplorer.Widgets.Add(_treeFileExplorer);

			/*			_propertyGrid.Settings.ImagePropertyValueGetter = name =>
						{
							switch (name)
							{
								case "TextureBase":
									return Scene.Terrain.TextureBaseName;
								case "TexturePaint1":
									return Scene.Terrain.TexturePaintName1;
								case "TexturePaint2":
									return Scene.Terrain.TexturePaintName2;
								case "TexturePaint3":
									return Scene.Terrain.TexturePaintName3;
								case "TexturePaint4":
									return Scene.Terrain.TexturePaintName4;
							}

							throw new Exception($"Unknown property {name}");
						};

						_propertyGrid.Settings.ImagePropertyValueSetter = (name, value) =>
						{
							switch (name)
							{
								case "TextureBase":
									Scene.Terrain.TextureBaseName = value;
									break;
								case "TexturePaint1":
									Scene.Terrain.TexturePaintName1 = value;
									RefreshLibrary();
									break;
								case "TexturePaint2":
									Scene.Terrain.TexturePaintName2 = value;
									RefreshLibrary();
									break;
								case "TexturePaint3":
									Scene.Terrain.TexturePaintName3 = value;
									RefreshLibrary();
									break;
								case "TexturePaint4":
									Scene.Terrain.TexturePaintName4 = value;
									RefreshLibrary();
									break;
								default:
									throw new Exception($"Unknown property {name}");
							}
						};*/

			_topSplitPane.SetSplitterPosition(0, 0.2f);
			_topSplitPane.SetSplitterPosition(1, 0.6f);

			_menuItemNew.Selected += (s, a) => NewScene();

			_menuItemOpenSolution.Selected += (s, a) =>
			{
				FileDialog dialog = new FileDialog(FileDialogMode.OpenFile)
				{
					Filter = "*.sln"
				};

				if (!string.IsNullOrEmpty(_filePath))
				{
					dialog.Folder = Path.GetDirectoryName(_filePath);
				}

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						// "Cancel" or Escape
						return;
					}

					// "Ok" or Enter
					LoadSolution(dialog.FilePath);
				};

				dialog.ShowModal(Desktop);
			};

			_menuItemSaveCurrentItem.Selected += (s, a) => SaveCurrentItem();

			_treeFileExplorer.TouchDown += (s, e) =>
			{
				var mouseState = Mouse.GetState();
				if (mouseState.RightButton == ButtonState.Pressed)
				{
					_explorerTouchDown = true;
				}
			};
			_treeFileExplorer.TouchUp += _treeFileExplorer_TouchUp;

			_treeFileExplorer.SelectionChanged += (s, a) =>
			{
				_propertyGrid.Object = _treeFileExplorer.SelectedNode?.Tag;
			};

			_treeFileSolution.TouchDoubleClick += (s, a) => OpenCurrentSolutionItem();

			_propertyGrid.PropertyChanged += (s, a) =>
			{
				InvalidateCurrentItem();

				switch (a.Data)
				{
					case "Id":
						UpdateTreeNodeId(_treeFileExplorer.SelectedNode);
						break;

					case "PrimitiveMeshType":
						_propertyGrid.Rebuild();
						break;
				}
			};

			_propertyGrid.CustomWidgetProvider = CreateCustomEditor;

			_tabControlScenes.SelectedIndexChanged += (s, a) => RefreshExplorer(null);

			_tabControlScenes.ItemsCollectionChanged += (s, a) => UpdateStackPanelEditor();
			_tabControlEffects.ItemsCollectionChanged += (s, a) => UpdateStackPanelEditor();

			_buttonGrid.IsToggled = NursiaEditorOptions.ShowGrid;
			_buttonBoundingBoxes.IsToggled = DebugSettings.DrawBoundingBoxes;
			_buttonLightViewFrustum.IsToggled = DebugSettings.DrawLightViewFrustrum;
			_buttonShadowMap.IsToggled = NursiaEditorOptions.DrawShadowMap;

			_buttonGrid.IsToggledChanged += (s, a) => UpdateDebugOptions();
			_buttonBoundingBoxes.IsToggledChanged += (s, a) => UpdateDebugOptions();
			_buttonLightViewFrustum.IsToggledChanged += (s, a) => UpdateDebugOptions();
			_buttonShadowMap.IsToggledChanged += (s, a) => UpdateDebugOptions();

			UpdateStackPanelEditor();
		}

		private Widget CreateCustomEditor(Record record, object obj)
		{
			if (obj is DefaultMaterial && record.Name == "Texture")
			{
				var material = (DefaultMaterial)obj;
				var propertyType = record.Type;

				var result = new HorizontalStackPanel
				{
					Spacing = 8
				};

				var path = new TextBox
				{
					Readonly = true,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Text = material.TexturePath
				};

				StackPanel.SetProportionType(path, ProportionType.Fill);
				result.Widgets.Add(path);

				var button = new Button
				{
					Tag = obj,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Content = new Label
					{
						Text = "Change...",
						HorizontalAlignment = HorizontalAlignment.Center,
					}
				};
				Grid.SetColumn(button, 1);

				button.Click += (sender, args) =>
				{
					try
					{
						var project = CurrentProject;
						var projectFolder = Path.GetDirectoryName(project.AbsolutePath);
						var texturesFolder = Path.Combine(projectFolder, Constants.TexturesFolder);

						var dialog = new ChooseAssetDialog(texturesFolder, new[] { "dds", "png", "jpg", "gif", "bmp", "tga" });

						dialog.Closed += (s, a) =>
						{
							if (!dialog.Result)
							{
								// "Cancel" or Escape
								return;
							}

							// "Ok" or Enter
							try
							{
								var path = dialog.FilePath;
								var assetManager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(path));

								var texture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, path);

								material.Texture = texture;
								material.TexturePath = path;
							}
							catch (Exception ex)
							{
								var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
								dialog.ShowModal(Desktop);
							}
						};

						dialog.ShowModal(Desktop);
					}
					catch (Exception ex)
					{
						var dialog = Dialog.CreateMessageBox("Error", ex.Message);
						dialog.ShowModal(Desktop);
					}
				};

				result.Widgets.Add(button);

				return result;
			}

			return null;
		}

		private bool SetTabByName(TabControl tabControl, string filePath)
		{
			for (var i = 0; i < tabControl.Items.Count; ++i)
			{
				var tabItem = tabControl.Items[i];
				var tabInfo = (TabInfo)tabItem.Tag;
				if (tabInfo.FilePath == filePath)
				{
					tabControl.SelectedIndex = i;
					return true;
				}
			}

			return false;
		}

		private void UpdateStackPanelEditor()
		{
			if (_tabControlScenes.Items.Count == 0 && _splitPaneEditor.Widgets.Contains(_panelScenes))
			{
				_splitPaneEditor.Widgets.Remove(_panelScenes);
			}
			else if (_tabControlScenes.Items.Count > 0 && !_splitPaneEditor.Widgets.Contains(_panelScenes))
			{
				_splitPaneEditor.Widgets.Insert(0, _panelScenes);
			}

			if (_tabControlEffects.Items.Count == 0 && _splitPaneEditor.Widgets.Contains(_tabControlEffects))
			{
				_splitPaneEditor.Widgets.Remove(_tabControlEffects);
			}
			else if (_tabControlEffects.Items.Count > 0 && !_splitPaneEditor.Widgets.Contains(_tabControlEffects))
			{
				_splitPaneEditor.Widgets.Add(_tabControlEffects);
			}
		}

		private void OpenCurrentSolutionItem()
		{
			try
			{
				var node = _treeFileSolution.SelectedNode;
				if (node == null || node.Tag == null || !(node.Tag is string))
				{
					return;
				}

				var file = (string)node.Tag;
				if (file.EndsWith(".scene"))
				{
					if (SetTabByName(_tabControlScenes, file))
					{
						return;
					}

					// Load scene
					Scene scene;
					var folder = Path.GetDirectoryName(file);
					var assetManager = AssetManager.CreateFileAssetManager(folder);

					scene = assetManager.LoadScene(file);

					var sceneWidget = new SceneWidget
					{
						HorizontalAlignment = HorizontalAlignment.Stretch,
						VerticalAlignment = VerticalAlignment.Stretch,
						Scene = scene,
						Project = (ProjectInSolution)node.ParentNode.ParentNode.Tag
					};

					var tabInfo = new TabInfo(file);

					var tabItem = new TabItem
					{
						Text = tabInfo.Title,
						Content = sceneWidget,
						Tag = tabInfo
					};

					tabInfo.TitleChanged += (s, a) => tabItem.Text = tabInfo.Title;

					_tabControlScenes.Items.Add(tabItem);
					_tabControlScenes.SelectedIndex = _tabControlScenes.Items.Count - 1;
				}
				else if (file.EndsWith(".fx"))
				{
					if (SetTabByName(_tabControlEffects, file))
					{
						return;
					}

					// Effect
					var effect = File.ReadAllText(file);

					var textEditor = new TextBox
					{
						Multiline = true,
						Wrap = true,
						Text = effect,
						HorizontalAlignment = HorizontalAlignment.Stretch,
						VerticalAlignment = VerticalAlignment.Stretch,
					};

					var scrollViewer = new ScrollViewer
					{
						Content = textEditor,
					};

					var tabInfo = new TabInfo(file);

					var tabItem = new TabItem
					{
						Text = tabInfo.Title,
						Content = textEditor,
						Tag = tabInfo
					};

					tabInfo.TitleChanged += (s, a) => tabItem.Text = tabInfo.Title;
					_tabControlEffects.Items.Add(tabItem);
					_tabControlEffects.SelectedIndex = _tabControlEffects.Items.Count - 1;
				}
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
				return;
			}
		}

		private void AddNewNode(SceneNode parent, SceneNode child)
		{
			parent.Children.Add(child);
			InvalidateCurrentItem();
			RefreshExplorer(child);
		}

		private void OnAddNode(SceneNode sceneNode)
		{
			var newNode = new SceneNode();
			AddNewNode(sceneNode, newNode);
		}

		private void OnAddBillboard(SceneNode sceneNode)
		{
			var newNode = new BillboardNode();
			AddNewNode(sceneNode, newNode);
		}

		private void OnAddText3D(SceneNode sceneNode)
		{
			var newNode = new Text3DNode();
			AddNewNode(sceneNode, newNode);
		}

		private void OnAddNewLight(SceneNode sceneNode)
		{
			var dialog = new AddNewItemDialog
			{
				Title = "Add New Light"
			};

			dialog.AddItem("Direct");
			dialog.AddItem("Point");

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				BaseLight light = null;
				switch (dialog.SelectedIndex)
				{
					case 0:
						light = new DirectLight();
						break;
					case 1:
						light = new PointLight();
						break;
				}

				if (light != null)
				{
					light.Id = dialog.ItemName;
					AddNewNode(sceneNode, light);
				}
			};

			dialog.ShowModal(Desktop);
		}

		private void OnAddNewGeometricPrimitive(SceneNode sceneNode)
		{
			var dialog = new AddNewItemDialog
			{
				Title = "Add New Geometric Primitive"
			};

			dialog.AddItem("Capsule");
			dialog.AddItem("Cone");
			dialog.AddItem("Cube");
			dialog.AddItem("Cylinder");
			dialog.AddItem("Disc");
			dialog.AddItem("GeoSphere");
			dialog.AddItem("Plane");
			dialog.AddItem("Sphere");
			dialog.AddItem("Teapot");
			dialog.AddItem("Torus");

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				PrimitiveMesh primitiveMesh = null;
				switch (dialog.SelectedIndex)
				{
					case 0:
						primitiveMesh = new Capsule();
						break;

					case 1:
						primitiveMesh = new Cone();
						break;

					case 2:
						primitiveMesh = new Cube();
						break;

					case 3:
						primitiveMesh = new Cylinder();
						break;

					case 4:
						primitiveMesh = new Disc();
						break;

					case 5:
						primitiveMesh = new GeoSphere();
						break;

					case 6:
						primitiveMesh = new Nursia.Primitives.Plane();
						break;

					case 7:
						primitiveMesh = new Sphere();
						break;

					case 8:
						primitiveMesh = new Teapot();
						break;

					case 9:
						primitiveMesh = new Torus();
						break;
				}

				if (primitiveMesh != null)
				{
					var meshNode = new PrimitiveMeshNode
					{
						Id = dialog.ItemName,
						Material = new DefaultMaterial(),
						PrimitiveMesh = primitiveMesh
					};

					AddNewNode(sceneNode, meshNode);
				}
			};

			dialog.ShowModal(Desktop);
		}

		private void OnAddGltfModel(SceneNode sceneNode)
		{
			try
			{
				var project = CurrentProject;
				var projectFolder = Path.GetDirectoryName(project.AbsolutePath);
				var modelsFolder = Path.Combine(projectFolder, Constants.ModelsFolder);

				var dialog = new ChooseAssetDialog(modelsFolder, new[] { "glb", "gltf" });

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						// "Cancel" or Escape
						return;
					}

					// "Ok" or Enter
					try
					{
						var path = dialog.FilePath;
						var node = new NursiaModel
						{
							Id = dialog.SelectedId,
							ModelPath = dialog.FilePath
						};

						var assetManager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(path));
						node.Load(assetManager);

						AddNewNode(sceneNode, node);
					}
					catch (Exception ex)
					{
						var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
						dialog.ShowModal(Desktop);
					}
				};

				dialog.ShowModal(Desktop);
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.Message);
				dialog.ShowModal(Desktop);
			}
		}

		private void _treeFileExplorer_TouchUp(object sender, EventArgs e)
		{
			if (!_explorerTouchDown)
			{
				return;
			}

			_explorerTouchDown = false;

			var treeNode = _treeFileExplorer.SelectedNode;
			if (treeNode == null || treeNode.Tag == null)
			{
				return;
			}

			var sceneNode = (SceneNode)treeNode.Tag;

			var contextMenuOptions = new List<Tuple<string, Action>>
			{
				new Tuple<string, Action>("Insert &Node...", () => OnAddNode(sceneNode)),
				new Tuple<string, Action>("Insert &Light...", () => OnAddNewLight(sceneNode)),
				new Tuple<string, Action>("Insert &Geometric Primitive...", () => OnAddNewGeometricPrimitive(sceneNode)),
				new Tuple<string, Action>("Insert &Gltf/Glb Model...", () => OnAddGltfModel(sceneNode)),
				new Tuple<string, Action>("Insert &Billboard...", () => OnAddBillboard(sceneNode)),
				new Tuple<string, Action>("Insert &Text 3D...", () => OnAddText3D(sceneNode)),
			};

			if (treeNode != _treeFileExplorer.GetSubNode(0))
			{
				// Not root, add delete
				contextMenuOptions.Add(new Tuple<string, Action>("Delete Current Node", () =>
				{
					sceneNode.RemoveFromParent();
					InvalidateCurrentItem();
					RefreshExplorer(null);
				}));
			}

			Desktop.BuildContextMenu(contextMenuOptions);
		}

		private void NewScene()
		{
			/*			var scene = new Scene
						{
							Id = "Root"
						};

						scene.Camera.Position = new Vector3(0, 15, 15);
						scene.Camera.YawAngle = 180;
						scene.Camera.PitchAngle = 30;

						var light = new DirectLight
						{
							Id = "DirectLight",
							Translation = new Vector3(-5, 5, 5),
							Direction = new Vector3(1, -1, 1)
						};

						scene.Children.Add(light);

						Scene = scene;
						FilePath = string.Empty;*/
		}

		private void UpdateTitle()
		{
			var title = string.IsNullOrEmpty(_filePath) ? "NursiaEditor" : _filePath;
			StudioGame.Instance.Window.Title = title;
		}

		private void RefreshProject(TreeViewNode projectNode)
		{
			projectNode.RemoveAllSubNodes();

			var project = (ProjectInSolution)projectNode.Tag;

			// Scenes
			var scenesNode = projectNode.AddSubNode(new Label
			{
				Text = "Scenes"
			});
			scenesNode.IsExpanded = true;

			var folder = Path.GetDirectoryName(project.AbsolutePath);
			var scenesFolder = Path.Combine(folder, Constants.ScenesFolder);
			if (Directory.Exists(scenesFolder))
			{
				var files = Directory.GetFiles(scenesFolder, "*.scene");
				foreach (var file in files)
				{
					var node = scenesNode.AddSubNode(new Label
					{
						Text = Path.GetFileName(file),
					});

					node.Tag = file;
				}
			}

			// Effects
			var effectsNode = projectNode.AddSubNode(new Label
			{
				Text = "Effects"
			});
			effectsNode.IsExpanded = true;

			var effectsFolder = Path.Combine(folder, Constants.EffectsFolder);
			if (Directory.Exists(effectsFolder))
			{
				var files = Directory.GetFiles(effectsFolder, "*.fx");
				foreach (var file in files)
				{
					var node = effectsNode.AddSubNode(new Label
					{
						Text = Path.GetFileName(file),
					});

					node.Tag = file;
				}
			}
		}

		public void LoadSolution(string path)
		{
			try
			{
				if (!string.IsNullOrEmpty(path))
				{
					var solutionFile = SolutionFile.Parse(path);

					Nrs.EffectsRegistry = new DynamicEffectsRegistry(Path.GetDirectoryName(path));

					_treeFileSolution.RemoveAllSubNodes();
					foreach (var project in solutionFile.ProjectsInOrder)
					{
						if (project.ProjectType != SolutionProjectType.KnownToBeMSBuildFormat)
						{
							continue;
						}

						var folder = Path.GetDirectoryName(project.AbsolutePath);
						var markerPath = Path.Combine(folder, Constants.MarkerFile);
						if (!File.Exists(markerPath))
						{
							continue;
						}

						var label = new Label
						{
							Text = project.ProjectName,
						};

						var projectNode = _treeFileSolution.AddSubNode(label);
						projectNode.IsExpanded = true;
						projectNode.Tag = project;

						RefreshProject(projectNode);
					}
				}

				_filePath = path;
				UpdateTitle();
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		private void ProcessSave(Scene scene, string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}

			try
			{
				scene.SaveToFile(filePath);
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		/*		private void Save(bool setFileName)
				{
					if (string.IsNullOrEmpty(FilePath) || setFileName)
					{
						var dlg = new FileDialog(FileDialogMode.SaveFile)
						{
							Filter = "*.scene"
						};

						if (!string.IsNullOrEmpty(FilePath))
						{
							dlg.FilePath = FilePath;
						}

						dlg.ShowModal(Desktop);

						dlg.Closed += (s, a) =>
						{
							if (dlg.Result)
							{
								ProcessSave(dlg.FilePath);
							}
						};
					}
					else
					{
						ProcessSave(FilePath);
					}
				}*/

		private void UpdateTreeNodeId(TreeViewNode node)
		{
			var sceneNode = (SceneNode)node.Tag;
			var label = (Label)node.Content;
			label.Text = $"{sceneNode.GetType().Name} (#{sceneNode.Id})";
		}

		private TreeViewNode RecursiveAddToExplorer(ITreeViewNode treeViewNode, SceneNode sceneNode)
		{
			var label = new Label();
			var newNode = treeViewNode.AddSubNode(label);
			newNode.IsExpanded = true;
			newNode.Tag = sceneNode;

			UpdateTreeNodeId(newNode);

			foreach (var child in sceneNode.Children)
			{
				RecursiveAddToExplorer(newNode, child);
			}

			return newNode;
		}

		private void RefreshExplorer(SceneNode selectedNode)
		{
			_treeFileExplorer.RemoveAllSubNodes();
			if (_tabControlScenes.SelectedIndex == null ||
				_tabControlScenes.SelectedIndex < 0 ||
				_tabControlScenes.SelectedIndex >= _tabControlScenes.Items.Count)
			{
				return;
			}

			var sceneWidget = (SceneWidget)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Content;
			RecursiveAddToExplorer(_treeFileExplorer, sceneWidget.Scene);
			_treeFileExplorer.SelectedNode = _treeFileExplorer.FindNode(n => n.Tag == selectedNode);
		}

		public void RefreshLibrary()
		{
		}

		private void InvalidateCurrentItem()
		{
			if (_tabControlScenes.SelectedIndex == null)
			{
				return;
			}

			var tabInfo = (TabInfo)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Tag;
			tabInfo.Dirty = true;
		}

		private void SaveCurrentItem()
		{
			if (_tabControlScenes.SelectedIndex == null)
			{
				return;
			}

			var tab = _tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value];
			var sceneWidget = (SceneWidget)tab.Content;

			var tabInfo = (TabInfo)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Tag;
			sceneWidget.Scene.SaveToFile(tabInfo.FilePath);
			tabInfo.Dirty = false;
		}

		private void UpdateDebugOptions()
		{
			NursiaEditorOptions.ShowGrid = _buttonGrid.IsToggled;
			DebugSettings.DrawBoundingBoxes = _buttonBoundingBoxes.IsToggled;
			DebugSettings.DrawLightViewFrustrum = _buttonLightViewFrustum.IsToggled;
			NursiaEditorOptions.DrawShadowMap = _buttonShadowMap.IsToggled;
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			if (_panelStatistics.Visible == false || CurrentSceneWidget == null)
			{
				return;
			}

			var stats = CurrentSceneWidget.RenderStatistics;

			_labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_labelDrawCalls.Text = stats.DrawCalls.ToString();
			_labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();
			_labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			_labelMeshesDrawn.Text = stats.MeshesDrawn.ToString();
		}
	}
}