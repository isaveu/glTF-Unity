using System;
using System.Collections.Generic;
using Gltf.Schema.Extensions;

namespace Gltf.Schema
{
    [Serializable]
    public class GltfObject : GltfProperty
    {
        /// <summary>
        /// Names of glTF extensions used somewhere in this asset.
        /// </summary>
        public List<string> extensionsUsed;

        /// <summary>
        /// Names of glTF extensions required to properly load this asset.
        /// </summary>
        public List<string> extensionsRequired;

        /// <summary>
        /// An array of accessors. An accessor is a typed view into a bufferView.
        /// </summary>
        public List<GltfAccessor> accessors;

        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        public List<GltfAnimation> animations;

        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        public GltfAsset asset;

        /// <summary>
        /// An array of buffers. A buffer points to binary geometry, animation, or skins.
        /// </summary>
        public List<GltfBuffer> buffers;

        /// <summary>
        /// An array of bufferViews.
        /// A bufferView is a view into a buffer generally representing a subset of the buffer.
        /// </summary>
        public List<GltfBufferView> bufferViews;

        /// <summary>
        /// An array of cameras. A camera defines a projection matrix.
        /// </summary>
        public List<GltfCamera> cameras;

        /// <summary>
        /// An array of images. An image defines data used to create a texture.
        /// </summary>
        public List<GltfImage> images;

        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        public List<GltfMaterial> materials;

        /// <summary>
        /// An array of meshes. A mesh is a set of primitives to be rendered.
        /// </summary>
        public List<GltfMesh> meshes;

        /// <summary>
        /// An array of nodes.
        /// </summary>
        public List<GltfNode> nodes;

        /// <summary>
        /// An array of samplers. A sampler contains properties for texture filtering and wrapping modes.
        /// </summary>
        public List<GltfSampler> samplers;

        /// <summary>
        /// The index of the default scene.
        /// </summary>
        public int scene;

        /// <summary>
        /// An array of scenes.
        /// </summary>
        public List<GltfScene> scenes;

        /// <summary>
        /// An array of skins. A skin is defined by joints and matrices.
        /// </summary>
        public List<GltfSkin> skins;

        /// <summary>
        /// An array of textures.
        /// </summary>
        public List<GltfTexture> textures;

        [NonSerialized]
        public List<GltfExtension> RegisteredExtensions = new List<GltfExtension>();
    }
}
