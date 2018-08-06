using System.Collections.Generic;

namespace Gltf.Schema
{
    public class GltfProperty
    {
        public readonly Dictionary<string, string> Extensions = new Dictionary<string, string>();
        public readonly Dictionary<string, string> Extras = new Dictionary<string, string>();
    }
}
