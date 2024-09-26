namespace Nursia.Rendering
{
	internal class ShadowMapMaterial : IMaterial
	{
		public static ShadowMapMaterial Default = new ShadowMapMaterial(false);
		public static ShadowMapMaterial DefaultSkinning = new ShadowMapMaterial(true);

		public EffectBinding EffectBinding { get; }

		public NodeBlendMode BlendMode => NodeBlendMode.Opaque;

		public bool CastsShadows => false;

		public bool ReceivesShadows => false;

		private ShadowMapMaterial(bool skinning)
		{
			EffectBinding = Resources.GetShadowMapEffectBinding(skinning, false)();
		}

		public void SetParameters()
		{
		}
	}
}
