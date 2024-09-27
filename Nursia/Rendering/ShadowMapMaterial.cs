using System.Collections.Generic;

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
			if (!skinning)
			{
				EffectBinding = EffectsRegistry.GetStockEffectBinding<EffectBinding>("ShadowMap");
			}
			else
			{
				EffectBinding = EffectsRegistry.GetStockEffectBinding<EffectBinding>("ShadowMap",
					new Dictionary<string, string>
					{
						["SKINNING"] = "1"
					});
			}
		}

		public void SetParameters()
		{
		}
	}
}
