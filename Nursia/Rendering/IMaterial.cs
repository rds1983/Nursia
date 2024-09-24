using AssetManagementBase;

namespace Nursia.Rendering
{
	public interface IMaterial
	{
		EffectBinding DefaultEffect { get; }
		EffectBinding ShadowMapEffect { get; }

		void Load(AssetManager assetManager);
	}
}
