using Nursia.Rendering;
using Nursia.Utilities;
using Nursia.Standard;

namespace Nursia.Primitives
{
	public abstract class PrimitiveMeshNode: MeshNodeBase
	{
		private bool _isLeftHanded;
		private Mesh _mesh;
		private float _uScale = 1.0f;
		private float _vScale = 1.0f;

		protected override Mesh RenderMesh
		{
			get
			{
				if (_mesh == null)
				{
					_mesh = CreateMesh();
				}

				return _mesh;
			}
		}

		public bool IsLeftHanded
		{
			get => _isLeftHanded;

			set
			{
				if (value == _isLeftHanded)
				{
					return;
				}

				_isLeftHanded = value;
				InvalidateMesh();
			}
		}

		public float UScale
		{
			get => _uScale;

			set
			{
				if (value.EpsilonEquals(_uScale))
				{
					return;
				}

				_uScale = value;
				InvalidateMesh();
			}
		}

		public float VScale
		{
			get => _vScale;

			set
			{
				if (value.EpsilonEquals(_vScale))
				{
					return;
				}

				_vScale = value;
				InvalidateMesh();
			}
		}

		protected abstract Mesh CreateMesh();

		public void InvalidateMesh()
		{
			_mesh = null;
		}
	}
}