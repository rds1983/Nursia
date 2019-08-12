using System;

namespace Nursia.AssetManagement
{
	[AttributeUsage(AttributeTargets.Class)]
	public class AssetLoaderAttribute: Attribute
	{
		public Type AssetLoaderType { get; private set; }
		public bool StoreInCache { get; private set; }

		public AssetLoaderAttribute(Type assetLoaderType, bool storeInCache = true)
		{
			if (assetLoaderType == null)
			{
				throw new ArgumentNullException("assetLoaderType");
			}

			AssetLoaderType = assetLoaderType;
			StoreInCache = storeInCache;
		}
	}
}
