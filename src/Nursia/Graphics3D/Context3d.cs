using Microsoft.Xna.Framework;
using Nursia.Graphics3D.Lights;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Context3d
	{
		private Matrix? _viewProjection;
		private BoundingFrustum _frustrum;
		private Matrix _projection = Matrix.Identity, _view = Matrix.Identity;
		private readonly List<DirectionalLight> _lights = new List<DirectionalLight>();

		public List<DirectionalLight> Lights
		{
			get
			{
				return _lights;
			}
		}

		public Matrix Projection
		{
			get
			{
				return _projection;
			}

			set
			{
				if (value == _projection)
				{
					return;
				}

				_projection = value;
				ResetView();
			}
		}

		public Matrix View
		{
			get
			{
				return _view;
			}

			set
			{
				if (value == _view)
				{
					return;
				}


				_view = value;
				ResetView();
			}
		}

		public Matrix ViewProjection
		{
			get
			{
				if (_viewProjection == null)
				{
					_viewProjection = View * Projection;
				}

				return _viewProjection.Value;
			}
		}

		public BoundingFrustum Frustrum
		{
			get
			{
				if (_frustrum == null)
				{
					_frustrum = new BoundingFrustum(ViewProjection);
				}

				return _frustrum;
			}
		}

		internal Matrix World { get; set; }

		public int MeshesDrawn { get; internal set; }

		public void ResetStatistics()
		{
			MeshesDrawn = 0;
		}

		private void ResetView()
		{
			_viewProjection = null;
			_frustrum = null;
		}

		public Context3d()
		{
			World = Matrix.Identity;
		}
	}
}
