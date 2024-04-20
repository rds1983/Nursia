using Microsoft.Xna.Framework;

namespace Nursia.Rendering
{
	public interface ISkinnedMaterial
	{
		Matrix[] BonesTransforms { get; set; }
	}
}
