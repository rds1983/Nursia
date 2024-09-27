using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;

namespace Nursia
{
	public interface IEffectsSource
	{
		Effect GetEffect(Assembly assembly, string name, Dictionary<string, string> defines);
	}
}
