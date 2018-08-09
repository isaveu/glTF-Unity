using System;

namespace Gltf.Schema
{
    /// <summary>
    /// The indices of each root node.
    /// </summary>
    [Serializable]
    public class GltfScene : GltfChildOfRootProperty
    {
        /// <summary>
        /// Indices of each root node.
        /// </summary>
        public int[] nodes;
    }
}