using System;

namespace Gltf.Schema
{
    [Serializable]
    public class GltfAnimationChannel : GltfProperty
    {
        /// <summary>
        /// The index of a sampler in this animation used to compute the value for the
        /// target, e.g., a node's translation, rotation, or scale (TRS).
        /// </summary>
        public uint sampler;

        /// <summary>
        /// The index of the node and TRS property to target.
        /// </summary>
        public GltfAnimationChannelTarget target;
    }
}