using System;

namespace Gltf.Schema
{
    /// <summary>
    /// A buffer points to binary geometry, animation, or skins.
    /// </summary>
    [Serializable]
    public class GltfBuffer : GltfChildOfRootProperty
    {
        /// <summary>
        /// The uri of the buffer.
        /// Relative paths are relative to the .gltf file.
        /// Instead of referencing an external file, the uri can also be a data-uri.
        /// </summary>
        public string uri;

        /// <summary>
        /// The length of the buffer in bytes.
        /// <minimum>0</minimum>
        /// </summary>
        public int byteLength;

        public byte[] BufferData { get; set; }
    }
}