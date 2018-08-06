using System;

namespace Gltf.Schema
{
    [Serializable]
    public class GltfMaterialCommonConstant : GltfProperty
    {
        /// <summary>
        /// Used to scale the ambient light contributions to this material
        /// </summary>
        public float[] ambientFactor;

        /// <summary>
        /// Texture used to store pre-computed direct lighting
        /// </summary>
        public GltfNormalTextureInfo lightmapTexture;

        /// <summary>
        /// Scale factor for the lightmap texture
        /// </summary>
        public float[] lightmapFactor;
    }
}