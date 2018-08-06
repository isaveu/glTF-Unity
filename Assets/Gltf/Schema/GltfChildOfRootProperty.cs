﻿using System;

namespace Gltf.Schema
{
    [Serializable]
    public class GltfChildOfRootProperty : GltfProperty
    {
        /// <summary>
        /// The user-defined name of this object.
        /// This is not necessarily unique, e.g., an accessor and a buffer could have the same name,
        /// or two accessors could even have the same name.
        /// </summary>
        public string name;
    }
}