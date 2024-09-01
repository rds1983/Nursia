using System;
using System.Collections.Generic;
using System.IO;
using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Nursia;
using Nursia.Modelling;
using Nursia.Rendering;
using Nursia.Rendering.Lights;
using NursiaEditor.Utility;

namespace NursiaEditor.UI
{
	public partial class MainForm
	{
		private bool _isDirty;
		private readonly SceneWidget _sceneWidget;
		private string _filePath;
		private bool _explorerTouchDown = false;
		private readonly TreeView _treeFileExplorer, _treeFileSystem;

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
				AssetManager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(_filePath));
				IsDirty = false;
				UpdateTitle();
			}
		}

		public bool IsDirty
		{
			get
			{
				return _isDirty;
			}

			set
			{
				if (value == _isDirty)
				{
					return;
				}

				_isDirty = value;
				UpdateTitle();
			}
		}

		public Scene Scene
		{
			get => _sceneWidget.Scene;
			set
			{
				_sceneWidget.Scene = value;
				RefreshExplorer(value);
				RefreshLibrary();
			}
		}

		public AssetManager AssetManager
		{
			get => _propertyGrid.Settings.AssetManager;
			set => _propertyGrid.Settings.AssetManager = value;
		}

		public ForwardRenderer Renderer { get => _sceneWidget.Renderer; }
		private readonly List<InstrumentButton> _allButtons = new List<InstrumentButton>();

		public MainForm()
		{
			BuildUI();

			_treeFileSystem = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};

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

			_sceneWidget = new SceneWidget
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};
			_panelScene.Widgets.Add(_sceneWidget);

			_topSplitPane.SetSplitterPosition(0, 0.25f);
			_topSplitPane.SetSplitterPosition(1, 0.7f);

			_menuItemNew.Selected += (s, a) => NewScene();

			_menuItemOpen.Selected += (s, a) =>
			{
				FileDialog dialog = new FileDialog(FileDialogMode.OpenFile)
				{
					Filter = "*.scene"
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
					Load(dialog.FilePath);
				};

				dialog.ShowModal(Desktop);
			};

			_menuItemSave.Selected += (s, a) => Save(false);
			_menuItemSaveAs.Selected += (s, a) => Save(true);

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
				_propertyGrid.Object = _treeFileExplorer.SelectedRow.Tag;
			};

			_propertyGrid.PropertyChanged += (s, a) =>
			{
				IsDirty = true;
			};

			NewScene();
		}

		private void AddNewNode(SceneNode parent, SceneNode child)
		{
			parent.Children.Add(child);
			RefreshExplorer(child);
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

			dialog.AddItem("Cube");
			dialog.AddItem("Cylinder");
			dialog.AddItem("GeoSphere");

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				object creationParams = null;
				Mesh mesh = null;
				switch (dialog.SelectedIndex)
				{
					case 0:
						{
							var p = PrimitiveMeshes.CubeParameters.Default;
							mesh = PrimitiveMeshes.CreateCube(p);
							creationParams = p;
						}
						break;

					case 1:
						{
							var p = PrimitiveMeshes.CylinderParameters.Default;
							mesh = PrimitiveMeshes.CreateCylinder(p);
							creationParams = p;
						}
						break;

					case 2:
						{
							var p = PrimitiveMeshes.GeoSphereParameters.Default;
							mesh = PrimitiveMeshes.CreateGeoSphere(p);
							creationParams = p;
						}
						break;
				}

				if (creationParams != null && mesh != null)
				{
					var meshNode = new MeshNode
					{
						Id = dialog.ItemName,
						Mesh = mesh,
						Material = new DefaultMaterial(),
						CreationParams = creationParams
					};

					AddNewNode(sceneNode, meshNode);
				}
			};

			dialog.ShowModal(Desktop);
		}

		private void OnAddGltfModel(SceneNode sceneNode)
		{
			var dialog = new FileDialog(FileDialogMode.OpenFile)
			{
				Filter = "*.gltf|*.glb"
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
				try
				{
					var node = new NursiaModel
					{
						ModelPath = dialog.FilePath
					};
					node.Load(AssetManager);

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

		private void _treeFileExplorer_TouchUp(object sender, EventArgs e)
		{
			if (!_explorerTouchDown)
			{
				return;
			}

			_explorerTouchDown = false;

			var treeNode = _treeFileExplorer.SelectedRow;
			if (treeNode == null || treeNode.Tag == null)
			{
				return;
			}

			var sceneNode = (SceneNode)treeNode.Tag;

			var contextMenuOptions = new List<Tuple<string, Action>>
			{
				new Tuple<string, Action>("Insert &Light...", () => OnAddNewLight(sceneNode)),
				new Tuple<string, Action>("Insert &Geometric Primitive...", () => OnAddNewGeometricPrimitive(sceneNode)),
				new Tuple<string, Action>("Insert &Gltf/Glb Model...", () => OnAddGltfModel(sceneNode)),
			};

			if (treeNode != _treeFileExplorer.GetSubNode(0))
			{
				// Not root, add delete
				contextMenuOptions.Add(new Tuple<string, Action>("Delete Current Node", () =>
				{
					sceneNode.RemoveFromParent();
					RefreshExplorer(null);
				}));
			}

			Desktop.BuildContextMenu(contextMenuOptions);
		}

		private void NewScene()
		{
			var scene = new Scene
			{
				Id = "Root"
			};

			scene.Camera.Position = new Vector3(0, 7, 7);
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
			FilePath = string.Empty;
		}

		private void UpdateTitle()
		{
			var title = string.IsNullOrEmpty(_filePath) ? "NursiaEditor" : _filePath;

			if (_isDirty)
			{
				title += " *";
			}

			StudioGame.Instance.Window.Title = title;
		}

		public void Load(string path)
		{
			try
			{
				var folder = Path.GetDirectoryName(path);
				AssetManager = AssetManager.CreateFileAssetManager(folder);

				Scene = AssetManager.LoadScene(path);
				FilePath = path;
				IsDirty = false;
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		private void ProcessSave(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}

			try
			{
				Scene.SaveToFile(filePath);
				FilePath = filePath;
				IsDirty = false;
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		private void Save(bool setFileName)
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
		}

		private TreeViewNode RecursiveAddToExplorer(ITreeViewNode treeViewNode, SceneNode sceneNode)
		{
			var label = new Label
			{
				Text = $"{sceneNode.GetType().Name} (#{sceneNode.Id})",
			};
			var newNode = treeViewNode.AddSubNode(label);
			newNode.IsExpanded = true;
			newNode.Tag = sceneNode;

			foreach (var child in sceneNode.Children)
			{
				RecursiveAddToExplorer(newNode, child);
			}

			return newNode;
		}

		private void RefreshExplorer(SceneNode selectedNode)
		{
			_treeFileExplorer.RemoveAllSubNodes();

			RecursiveAddToExplorer(_treeFileExplorer, Scene);

			_treeFileExplorer.SelectedRow = _treeFileExplorer.FindNode(n => n.Tag == selectedNode);
		}

		public void RefreshLibrary()
		{
		}
	}
}