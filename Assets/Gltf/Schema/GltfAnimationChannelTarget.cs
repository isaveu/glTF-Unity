using System;

namespace Gltf.Schema
{
    [Serializable]
    public class GltfAnimationChannelTarget : GltfProperty
    {
        /// <summary>
        /// The index of the node to target.
        /// </summary>
        public uint node;

        /// <summary>
        /// The name of the node's TRS property to modify.
        /// </summary>
        public GltfAnimationChannelPath path;
    }
}