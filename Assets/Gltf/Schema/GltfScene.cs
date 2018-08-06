using System.Collections.Generic;

namespace Gltf.Schema
{
    /// <summary>
    /// The indices of each root node.
    /// </summary>
    public class GltfScene : GltfChildOfRootProperty
    {
        public List<int> nodes;
    }
}