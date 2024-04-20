using AssetManagementBase;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nursia.Graphics3D.Lights;
using Nursia.Modelling;
using Nursia.Landscape;
using Nursia.Sky;
using Nursia.Utilities;
using StbImageSharp;
using StbImageWriteSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nursia.Graphics3D
{
	public class Scene : ItemWithId
	{
		public List<DirectLight> DirectLights { get; } = new List<DirectLight>();
		public List<BaseLight> PointLights { get; } = new List<BaseLight>();
		public bool HasLights => DirectLights.Count > 0 || PointLights.Count > 0;

		public Skybox Skybox;

		public List<NursiaModel> Models { get; } = new List<NursiaModel>();
		public List<WaterTile> WaterTiles { get; } = new List<WaterTile>();
		public float DefaultWaterLevel { get; set; } = -2;

		public Terrain Terrain { get; set; }

		public Camera Camera { get; } = new Camera();
		public EditorMarker Marker { get; } = new EditorMarker();
		public bool HasMarker => Marker.Position != null;


		private static void LoadItem(ItemWithId item, JObject obj)
		{
			item.Id = obj.OptionalId();
		}

		private static void LoadBaseLight(BaseLight light, JObject obj)
		{
			LoadItem(light, obj);

			light.Color = obj.EnsureColor("Color");
			light.Position = obj.EnsureVector3("Position");
		}

		private static DirectLight LoadDirectLight(JObject obj)
		{
			var result = new DirectLight();
			LoadBaseLight(result, obj);

			result.Direction = obj.EnsureVector3("Direction");

			return result;
		}

		private static WaterTile LoadWaterTile(JObject obj)
		{
			var result = new WaterTile(obj.EnsureFloat("X"), obj.EnsureFloat("Z"), obj.EnsureFloat("Height"),
				obj.EnsureFloat("SizeX"), obj.EnsureFloat("SizeZ"));

			LoadItem(result, obj);

			return result;
		}

		public static Scene Load(string jsonPath, AssetManager assetManager)
		{
			var folder = Path.GetDirectoryName(jsonPath);
			var json = File.ReadAllText(jsonPath);
			var rootObject = JObject.Parse(json);

			var scene = new Scene();
			LoadItem(scene, rootObject);
			var directLightsObj = rootObject["DirectLights"];
			foreach (JObject directLightObj in directLightsObj)
			{
				var directLight = LoadDirectLight(directLightObj);
				scene.DirectLights.Add(directLight);
			}

			var terrainObj = rootObject.OptionalJObject("Terrain");
			if (terrainObj != null)
			{
				var tileSize = terrainObj.OptionalPoint("TileSize", Terrain.DefaultTileSizeX, Terrain.DefaultTileSizeY);
				var tileVertexCount = terrainObj.OptionalPoint("TileVertexCount", Terrain.DefaultTileVertexCountX, Terrain.DefaultTileVertexCountY);
				var tilesCount = terrainObj.OptionalPoint("TilesCount", Terrain.DefaultTilesCountX, Terrain.DefaultTilesCountY);
				var tileSplatTextureSize = terrainObj.OptionalPoint("TileSplatTextureSize", Terrain.DefaultSplatTextureSizeX, Terrain.DefaultSplatTextureSizeY);

				var terrain = new Terrain(tileSize.X, tileSize.Y, tileVertexCount.X, tileVertexCount.Y,
					tilesCount.X, tilesCount.Y, tileSplatTextureSize.X, tileSplatTextureSize.Y);

				var tileTextureScale = terrainObj.OptionalVector2("TileTextureScale", Terrain.DefaultTextureScaleX, Terrain.DefaultTextureScaleY);
				terrain.TileTextureScale = tileTextureScale;

				terrain.TextureBaseName = terrainObj.OptionalString("TextureBase");
				if (!string.IsNullOrEmpty(terrain.TextureBaseName))
				{
					terrain.TextureBase = assetManager.LoadTexture2D(Nrs.GraphicsDevice, terrain.TextureBaseName);
				}

				terrain.TexturePaintName1 = terrainObj.OptionalString("TexturePaint1");
				if (!string.IsNullOrEmpty(terrain.TexturePaintName1))
				{
					terrain.TexturePaint1 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, terrain.TexturePaintName1);
				}

				terrain.TexturePaintName2 = terrainObj.OptionalString("TexturePaint2");
				if (!string.IsNullOrEmpty(terrain.TexturePaintName2))
				{
					terrain.TexturePaint2 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, terrain.TexturePaintName2);
				}

				terrain.TexturePaintName3 = terrainObj.OptionalString("TexturePaint3");
				if (!string.IsNullOrEmpty(terrain.TexturePaintName3))
				{
					terrain.TexturePaint3 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, terrain.TexturePaintName3);
				}

				terrain.TexturePaintName4 = terrainObj.OptionalString("TexturePaint4");
				if (!string.IsNullOrEmpty(terrain.TexturePaintName4))
				{
					terrain.TexturePaint4 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, terrain.TexturePaintName4);
				}

				// Load tile data
				for (var x = 0; x < terrain.TilesCount.X; ++x)
				{
					for (var y = 0; y < terrain.TilesCount.Y; ++y)
					{
						var tile = terrain[x, y];

						var path = $"terrain_height_{x}_{y}.raw";
						path = Path.Combine(folder, path);

						if (File.Exists(path))
						{
							// Height Map
							var data = new byte[tileVertexCount.X * tileVertexCount.Y * sizeof(float)];
							using (var stream = File.OpenRead(path))
							{
								stream.Read(data, 0, data.Length);
							}

							Buffer.BlockCopy(data, 0, tile._heightMap, 0, data.Length);
						}

						path = $"terrain_splat_{x}_{y}.png";
						path = Path.Combine(folder, path);
						if (File.Exists(path))
						{
							using (var stream = File.OpenRead(path))
							{
								var image = ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

								unsafe
								{
									fixed (Color* ptr = &tile._splatData[0])
									{
										Marshal.Copy(image.Data, 0, new IntPtr(ptr), image.Data.Length);
									}
								}
							}
						}
					}
				}

				scene.Terrain = terrain;
			}

			var waterTilesObj = rootObject["WaterTiles"];
			if (waterTilesObj != null)
			{
				foreach (JObject waterTileObj in waterTilesObj)
				{
					var waterTile = LoadWaterTile(waterTileObj);
					scene.WaterTiles.Add(waterTile);
				}
			}

			var cameraObj = rootObject["Camera"] as JObject;
			var camera = scene.Camera;
			if (cameraObj != null)
			{
				camera.Position = cameraObj.EnsureVector3("Position");
				camera.YawAngle = cameraObj.EnsureFloat("YawAngle");
				camera.RollAngle = cameraObj.EnsureFloat("RollAngle");
				camera.PitchAngle = cameraObj.EnsureFloat("PitchAngle");
			}

			return scene;
		}

		private static JObject SaveItem(ItemWithId item)
		{
			var result = new JObject
			{
				["Id"] = item.Id
			};

			return result;
		}

		private static JObject SaveBaseLight(BaseLight light)
		{
			var result = SaveItem(light);

			result["Color"] = light.Color.ToJArray();
			result["Position"] = light.Position.ToJArray();

			return result;
		}

		private static JObject SaveDirectLight(DirectLight light)
		{
			var result = SaveBaseLight(light);

			result["Direction"] = light.Direction.ToJArray();

			return result;
		}

		private static JObject SaveWaterTile(WaterTile waterTile)
		{
			var result = SaveItem(waterTile);

			result["X"] = waterTile.X.ToJsonString();
			result["Z"] = waterTile.Z.ToJsonString();
			result["Height"] = waterTile.Height.ToJsonString();
			result["SizeX"] = waterTile.SizeX.ToJsonString();
			result["SizeZ"] = waterTile.SizeZ.ToJsonString();

			return result;
		}

		public void Save(string outputFolder)
		{
			var result = SaveItem(this);

			// Direct lights
			var directLightsObj = new JArray();
			foreach (var directLight in DirectLights)
			{
				var directLightObj = SaveDirectLight(directLight);
				directLightsObj.Add(directLightObj);
			}

			result["DirectLights"] = directLightsObj;

			if (Terrain != null)
			{
				var terrainObj = new JObject
				{
					["TileSize"] = Terrain.TileSize.ToJArray(),
					["TileVertexCount"] = Terrain.TileVertexCount.ToJArray(),
					["TilesCount"] = Terrain.TilesCount.ToJArray(),
					["TileSplatTextureSize"] = Terrain.TileSplatTextureSize.ToJArray(),
					["TileTextureScale"] = Terrain.TileTextureScale.ToJArray(),
					["TextureBase"] = Terrain.TextureBaseName,
					["TexturePaint1"] = Terrain.TexturePaintName1,
					["TexturePaint2"] = Terrain.TexturePaintName2,
					["TexturePaint3"] = Terrain.TexturePaintName3,
					["TexturePaint4"] = Terrain.TexturePaintName4,
				};

				// Write each tile
				for (var x = 0; x < Terrain.TilesCount.X; ++x)
				{
					for (var y = 0; y < Terrain.TilesCount.Y; ++y)
					{
						var tile = Terrain[x, y];

						// Height Map
						if (!tile.CheckIfFlatTile())
						{
							var data = new byte[tile._heightMap.Length * sizeof(float)];
							Buffer.BlockCopy(tile._heightMap, 0, data, 0, data.Length);

							var path = $"terrain_height_{x}_{y}.raw";
							path = Path.Combine(outputFolder, path);
							using (var stream = File.OpenWrite(path))
							using (var writer = new BinaryWriter(stream))
							{
								writer.Write(data);
							}
						}

						// Splat Texture
						if (!tile.CheckIfCleanTile())
						{
							var path = $"terrain_splat_{x}_{y}.png";
							path = Path.Combine(outputFolder, path);
							using (Stream stream = File.OpenWrite(path))
							{
								ImageWriter writer = new ImageWriter();

								unsafe
								{
									fixed (Color* ptr = &tile._splatData[0])
									{
										writer.WritePng(ptr, Terrain.TileSplatTextureSize.X, Terrain.TileSplatTextureSize.Y, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
									}
								}
							}
						}
					}
				}

				result["Terrain"] = terrainObj;
			}

			var waterTilesObj = new JArray();
			foreach (var waterTile in WaterTiles)
			{
				var waterTileObj = SaveWaterTile(waterTile);
				waterTilesObj.Add(waterTileObj);

				result["WaterTiles"] = waterTilesObj;
			}

			var cameraObj = new JObject
			{
				["Position"] = Camera.Position.ToJArray(),
				["YawAngle"] = Camera.YawAngle.ToJsonString(),
				["PitchAngle"] = Camera.PitchAngle.ToJsonString(),
				["RollAngle"] = Camera.RollAngle.ToJsonString()
			};

			result["Camera"] = cameraObj;

			var json = JsonConvert.SerializeObject(result, Formatting.Indented);

			var fileName = string.IsNullOrEmpty(Id) ? "scene.json" : Id + ".json";
			var filePath = Path.Combine(outputFolder, fileName);
			File.WriteAllText(filePath, json);
		}
	}
}
