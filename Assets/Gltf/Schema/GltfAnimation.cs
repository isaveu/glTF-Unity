using System;
using System.Collections.Generic;

namespace Gltf.Schema
{
    /// <summary>
    /// A keyframe animation.
    /// </summary>
    [Serializable]
    public class GltfAnimation : GltfChildOfRootProperty
    {
        /// <summary>
        /// An array of channels, each of which targets an animation's sampler at a
        /// node's property. Different channels of the same animation can't have equal
        /// targets.
        /// </summary>
        public List<GltfAnimationChannel> channels;

        /// <summary>
        /// An array of samplers that combines input and output accessors with an
        /// interpolation algorithm to define a keyframe graph (but not its target).
        /// </summary>
        public List<GltfAnimationSampler> samplers;
    }
}