using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Nursia
{
	public interface IExternalEffectsSource
	{
		Func<Effect> Get(GraphicsDevice graphicsDevice, string name, Dictionary<string, string> defines);
	}
}
