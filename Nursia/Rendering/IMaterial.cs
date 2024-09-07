namespace Nursia.Rendering
{
	public interface IMaterial
	{
		EffectBinding DefaultEffect { get; }
		EffectBinding ShadowMapEffect { get; }
	}
}
