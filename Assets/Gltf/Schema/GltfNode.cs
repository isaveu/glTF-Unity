using System;
using System.Collections.Generic;

namespace Gltf.Schema
{
    /// <summary>
    /// A node in the node hierarchy.
    /// When the node contains `skin`, all `mesh.primitives` must contain `JOINT`
    /// and `WEIGHT` attributes.  A node can have either a `matrix` or any combination
    /// of `translation`/`rotation`/`scale` (TRS) properties.
    /// TRS properties are converted to matrices and postmultiplied in
    /// the `T * R * S` order to compose the transformation matrix;
    /// first the scale is applied to the vertices, then the rotation, and then
    /// the translation. If none are provided, the transform is the Identity.
    /// When a node is targeted for animation
    /// (referenced by an animation.channel.target), only TRS properties may be present;
    /// `matrix` will not be present.
    /// </summary>
    [Serializable]
    public class GltfNode : GltfChildOfRootProperty
    {
        /// <summary>
        /// If true, extracts transform, rotation, scale values from the Matrix4x4. Otherwise uses the Transform, Rotate, Scale directly as specified by by the node.
        /// </summary>
        public bool useTRS;

        /// <summary>
        /// The index of the camera referenced by this node.
        /// </summary>
        public int camera;

        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        public int[] children;

        /// <summary>
        /// The index of the skin referenced by this node.
        /// </summary>
        public int skin;

        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column-major order.
        /// </summary>
        public float[] matrix;

        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        public int mesh;

        /// <summary>
        /// The node's unit quaternion rotation in the order (x, y, z, w),
        /// where w is the scalar.
        /// </summary>
        public float[] rotation;

        /// <summary>
        /// The node's non-uniform scale.
        /// </summary>
        public float[] scale;

        /// <summary>
        /// The node's translation.
        /// </summary>
        public float[] translation;

        /// <summary>
        /// The weights of the instantiated Morph Target.
        /// Number of elements must match number of Morph Targets of used mesh.
        /// </summary>
        public double[] weights;
    }
}