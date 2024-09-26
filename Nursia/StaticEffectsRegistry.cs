using AssetManagementBase;
using Nursia.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nursia
{
	/// <summary>
	/// Default effects registry that simply loads precompiled effects from assembly resources
	/// </summary>
	public class StaticEffectsRegistry : IEffectsRegistry
	{
#if FNA
		private const string EffectsResourcePath = "Effects.FNA.bin";
#elif MONOGAME
		private const string EffectsResourcePath = "Effects.MonoGameOGL.bin";
#endif

		private Dictionary<string, AssetManager> _assetsManagers = new Dictionary<string, AssetManager>();

		public EffectBinding GetEffectBinding(Assembly assembly, string name, Dictionary<string, string> defines)
		{
			AssetManager assetManager;
			if (!_assetsManagers.TryGetValue(assembly.FullName, out assetManager))
			{
				assetManager = AssetManager.CreateResourceAssetManager(assembly, EffectsResourcePath);
				_assetsManagers[assembly.FullName] = assetManager;
			}

			name = Path.ChangeExtension(name, "efb");
			var effect = assetManager.LoadEffect(Nrs.GraphicsDevice, name, defines);

			EffectBinding result;
			if (effect.Tag == null)
			{
				result = new EffectBinding(effect);
				effect.Tag = result;
			}
			else
			{
				result = (EffectBinding)effect.Tag;
			}

			return result;
		}
	}
}
