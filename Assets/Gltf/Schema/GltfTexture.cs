using System;

namespace Gltf.Schema
{
    /// <summary>
    /// A texture and its sampler.
    /// </summary>
    [Serializable]
    public class GltfTexture : GltfChildOfRootProperty
    {
        /// <summary>
        /// The index of the sampler used by this texture.
        /// </summary>
        public int sampler;

        /// <summary>
        /// The index of the image used by this texture.
        /// </summary>
        public int source;
    }
}