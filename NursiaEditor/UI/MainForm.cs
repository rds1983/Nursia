using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetManagementBase;
using Microsoft.Build.Construction;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Nursia.Rendering;
using NursiaEditor.Utility;

namespace NursiaEditor.UI
{
	public partial class MainForm
	{
		private readonly SceneWidget _sceneWidget;
		private string _filePath;
		private readonly TreeView _treeFileSystem;

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

		public string BasePath
		{
			get => _propertyGrid.Settings.BasePath;
			set => _propertyGrid.Settings.BasePath = value;
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

			_treeFileSystem.TouchDown += (s, a) =>
			{
				if (_treeFileSystem.SelectedRow == null)
				{
					return;
				}

				Desktop.BuildContextMenu(new Tuple<string, Action>[]
				{
					new Tuple<string, Action>("Add New Scene", AddNewScene)
				});
			};

			_panelFileSystem.Widgets.Add(_treeFileSystem);

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
		}

		private void AddNewScene()
		{
			var dialog = new AddNewItemDialog
			{
				Title = "Add New Scene"
			};

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					return;
				}

				var node = _treeFileSystem.SelectedRow;
				var project = (ProjectInSolution)node.Tag;
				var path = Path.Combine(Path.GetDirectoryName(project.AbsolutePath), $"{Constants.ScenesFolder}/{dialog.ItemName}");

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				var scene = new Scene();
				scene.SaveToFile(path);
				RefreshProject(node);
			};

			dialog.ShowModal(Desktop);
		}

		private void UpdateTitle()
		{
			var title = string.IsNullOrEmpty(_filePath) ? "NursiaEditor" : _filePath;

			StudioGame.Instance.Window.Title = title;
		}

		private void RefreshProject(TreeViewNode node)
		{
			node.RemoveAllSubNodes();

			var project = (ProjectInSolution)node.Tag;
			var path = Path.Combine(Path.GetDirectoryName(project.AbsolutePath), Constants.ScenesFolder);
			if (!Directory.Exists(path))
			{
				return;
			}

			var scenes = Directory.EnumerateDirectories(path);
			if (scenes.Count() == 0)
			{
				return;
			}

			foreach (var scene in scenes)
			{

			}
		}

		public void LoadSolution(string path)
		{
			try
			{
				if (!string.IsNullOrEmpty(path))
				{
					var solutionFile = SolutionFile.Parse(path);

					_treeFileSystem.RemoveAllSubNodes();
					foreach (var project in solutionFile.ProjectsInOrder)
					{
						if (project.ProjectType != SolutionProjectType.KnownToBeMSBuildFormat)
						{
							continue;
						}

						var label = new Label
						{
							Text = project.ProjectName,
						};

						var projectNode = _treeFileSystem.AddSubNode(label);
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

		private void RefreshExplorer()
		{
		}

		public void RefreshLibrary()
		{
		}
	}
}