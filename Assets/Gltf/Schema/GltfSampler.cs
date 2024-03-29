﻿using System;

namespace Gltf.Schema
{
    /// <summary>
    /// Texture sampler properties for filtering and wrapping modes.
    /// </summary>
    [Serializable]
    public class GltfSampler : GltfChildOfRootProperty
    {
        /// <summary>
        /// Magnification filter.
        /// Valid values correspond to WebGL enums: `9728` (NEAREST) and `9729` (LINEAR).
        /// </summary>
        public GltfMagnificationFilterMode magFilter = GltfMagnificationFilterMode.Linear;

        /// <summary>
        /// Minification filter. All valid values correspond to WebGL enums.
        /// </summary>
        public GltfMinFilterMode GltfMinFilter = GltfMinFilterMode.NearestMipmapLinear;

        /// <summary>
        /// s wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public GltfWrapMode wrapS = GltfWrapMode.Repeat;

        /// <summary>
        /// t wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public GltfWrapMode wrapT = GltfWrapMode.Repeat;
    }
}