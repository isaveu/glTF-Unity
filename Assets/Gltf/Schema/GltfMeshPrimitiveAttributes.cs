﻿using System;

namespace Gltf.Schema
{
    /// <summary>
    /// Common mesh primitive attributes.
    /// </summary>
    /// <remarks>
    /// Application specific semantics are not supported
    /// </remarks>
    [Serializable]
    public class GltfMeshPrimitiveAttributes
    {
        public int POSITION = -1;
        public int NORMAL = -1;
        public int TANGENT = -1;
        public int TEXCOORD_0 = -1;
        public int TEXCOORD_1 = -1;
        public int TEXCOORD_2 = -1;
        public int TEXCOORD_3 = -1;
        public int COLOR_0 = -1;
        public int JOINTS_0 = -1;
        public int WEIGHTS_0 = -1;
    }
}