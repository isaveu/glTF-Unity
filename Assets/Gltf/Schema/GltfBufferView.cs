using System;

namespace Gltf.Schema
{
    /// <summary>
    /// A view into a buffer generally representing a subset of the buffer.
    /// </summary>
    [Serializable]
    public class GltfBufferView : GltfChildOfRootProperty
    {
        /// <summary>
        /// The index of the buffer.
        /// </summary>
        public uint buffer;

        /// <summary>
        /// The offset into the buffer in bytes.
        /// <minimum>0</minimum>
        /// </summary>
        public uint byteOffset;

        /// <summary>
        /// The length of the bufferView in bytes.
        /// <minimum>0</minimum>
        /// </summary>
        public uint byteLength;

        /// <summary>
        /// The stride, in bytes, between vertex attributes or other interleavable data.
        /// When this is zero, data is tightly packed.
        /// <minimum>0</minimum>
        /// <maximum>255</maximum>
        /// </summary>
        public byte byteStride;

        /// <summary>
        /// The target that the WebGL buffer should be bound to.
        /// All valid values correspond to WebGL enums.
        /// When this is not provided, the bufferView contains animation or skin data.
        /// </summary>
        public GltfBufferViewTarget target = GltfBufferViewTarget.None;
    }
}