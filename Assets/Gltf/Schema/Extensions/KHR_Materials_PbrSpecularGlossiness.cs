﻿using System;

namespace Gltf.Schema.Extensions
{
    [Serializable]
    public class KHR_Materials_PbrSpecularGlossiness : GltfExtension
    {
        public float[] diffuseFactor;
        public GltfTextureInfo diffuseTexture;
        public float[] specularFactor;
        public float glossinessFactor;
        public GltfTextureInfo specularGlossinessTexture;
    }
}
