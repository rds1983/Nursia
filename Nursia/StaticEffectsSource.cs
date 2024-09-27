using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nursia
{
	/// <summary>
	/// Default effects registry that simply loads precompiled effects from assembly resources
	/// </summary>
	public class StaticEffectsSource : IEffectsSource
	{
#if FNA
		private const string EffectsResourcePath = "Effects.FNA.bin";
#elif MONOGAME
		private const string EffectsResourcePath = "Effects.MonoGameOGL.bin";
#endif

		private Dictionary<string, AssetManager> _assetsManagers = new Dictionary<string, AssetManager>();

		public Effect GetEffect(Assembly assembly, string name, Dictionary<string, string> defines)
		{
			AssetManager assetManager;

			var key = assembly.GetName().Name;
			if (!_assetsManagers.TryGetValue(key, out assetManager))
			{
				assetManager = AssetManager.CreateResourceAssetManager(assembly, EffectsResourcePath);
				_assetsManagers[key] = assetManager;
			}

			name = Path.ChangeExtension(name, "efb");
			return assetManager.LoadEffect(Nrs.GraphicsDevice, name, defines);
		}
	}
}
