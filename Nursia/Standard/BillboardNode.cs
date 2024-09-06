using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Standard
{
	public class BillboardNode: BillboardNodeBase
	{
		public Texture2D Texture { get; set; }

		protected internal override Texture2D RenderTexture => Texture;
	}
}
