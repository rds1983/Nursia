namespace Nursia.Rendering
{
	public interface IMaterial
	{
		EffectBinding EffectBinding { get; }
		NodeBlendMode BlendMode { get; }
		bool CastsShadows { get; }
		bool ReceivesShadows { get; }

		void SetParameters();
	}
}
