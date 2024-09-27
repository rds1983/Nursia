using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.Rendering
{
	public abstract class BaseMaterial<T> where T : EffectBinding
	{
		private T _binding;

		protected T InternalBinding
		{
			get
			{
				if (_binding == null)
				{
					_binding = CreateBinding();
				}

				return _binding;
			}
		}

		[Browsable(false)]
		[JsonIgnore] 
		public EffectBinding EffectBinding => InternalBinding;

		protected void Invalidate()
		{
			_binding = null;
		}

		protected abstract T CreateBinding();
	}
}
