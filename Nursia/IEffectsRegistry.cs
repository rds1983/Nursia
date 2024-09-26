using Nursia.Rendering;
using System.Collections.Generic;
using System.Reflection;

namespace Nursia
{
	public interface IEffectsRegistry
	{
		EffectBinding GetEffectBinding(Assembly assembly, string name, Dictionary<string, string> defines);
	}
}
