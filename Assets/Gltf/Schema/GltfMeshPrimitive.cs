﻿using System;
using System.Collections.Generic;

namespace Gltf.Schema
{
    /// <summary>
    /// Geometry to be rendered with the given material.
    /// </summary>
    [Serializable]
    public class GltfMeshPrimitive : GltfProperty
    {
        #region Serialized Fields

        /// <summary>
        /// The index of the accessor that contains mesh indices.
        /// When this is not defined, the primitives should be rendered without indices
        /// using `drawArrays()`. When defined, the accessor must contain indices:
        /// the `bufferView` referenced by the accessor must have a `target` equal
        /// to 34963 (ELEMENT_ARRAY_BUFFER); a `byteStride` that is tightly packed,
        /// i.e., 0 or the byte size of `componentType` in bytes;
        /// `componentType` must be 5121 (UNSIGNED_BYTE), 5123 (UNSIGNED_SHORT)
        /// or 5125 (UNSIGNED_INT), the latter is only allowed
        /// when `OES_element_index_uint` extension is used; `type` must be `\"SCALAR\"`.
        /// </summary>
        public int indices;

        /// <summary>
        /// The index of the material to apply to this primitive when rendering.
        /// </summary>
        public int material;

        /// <summary>
        /// The type of primitives to render. All valid values correspond to WebGL enums.
        /// </summary>
        public GltfDrawMode mode;

        #endregion Serialized Fields

        /// <summary>
        /// An array of Morph Targets, each  Morph Target is a dictionary mapping
        /// attributes (only "POSITION" and "NORMAL" supported) to their deviations
        /// in the Morph Target (index of the accessor containing the attribute
        /// displacements' data).
        /// </summary>
        public List<Dictionary<string, int>> Targets;

        /// <summary>
        /// A dictionary object, where each key corresponds to mesh attribute semantic
        /// and each value is the index of the accessor containing attribute's data.
        /// </summary>
        public GltfMeshPrimitiveAttributes Attributes;
    }
}