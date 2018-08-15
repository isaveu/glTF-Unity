using System;

namespace Gltf.Schema
{
    [Serializable]
    public class GltfNormalTextureInfo : GltfTextureInfo
    {
        /// <summary>
        /// The scalar multiplier applied to each normal vector of the texture.
        /// This value is ignored if normalTexture is not specified.
        /// This value is linear.
        /// </summary>
        public double scale = 1d;
    }
}