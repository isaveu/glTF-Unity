using System;
using System.Collections.Generic;

namespace Gltf.Schema
{
    /// <summary>
    /// A set of primitives to be rendered. A node can contain one or more meshes.
    /// A node's transform places the mesh in the scene.
    /// </summary>
    [Serializable]
    public class GltfMesh : GltfChildOfRootProperty
    {
        /// <summary>
        /// An array of primitives, each defining geometry to be rendered with
        /// a material.
        /// <minItems>1</minItems>
        /// </summary>
        public GltfMeshPrimitive[] primitives;

        /// <summary>
        /// Array of weights to be applied to the Morph Targets.
        /// <minItems>0</minItems>
        /// </summary>
        public double[] weights;
    }
}