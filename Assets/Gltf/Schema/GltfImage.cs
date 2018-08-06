using System;

namespace Gltf.Schema
{
    /// <summary>
    /// Image data used to create a texture. Image can be referenced by URI or
    /// `bufferView` index. `mimeType` is required in the latter case.
    /// </summary>
    [Serializable]
    public class GltfImage : GltfChildOfRootProperty
    {
        /// <summary>
        /// The uri of the image.  Relative paths are relative to the .gltf file.
        /// Instead of referencing an external file, the uri can also be a data-uri.
        /// The image format must be jpg, png, bmp, or gif.
        /// </summary>
        public string uri;

        /// <summary>
        /// The image's MIME type.
        /// <minLength>1</minLength>
        /// </summary>
        public string mimeType;

        /// <summary>
        /// The index of the bufferView that contains the image.
        /// Use this instead of the image's uri property.
        /// </summary>
        public uint bufferView;
    }
}