using System.IO;
using Gltf.Schema;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gltf.Serialization
{
    public static class ImportGltf
    {
        /// <summary>
        /// Imports the glTF Object and returns a new <see cref="GameObject"/> of the final constructed <see cref="GltfScene"/>.
        /// </summary>
        /// <param name="gltfObject"></param>
        /// <returns></returns>
        public static GameObject ImportGltfObject(GltfObject gltfObject)
        {
            if (!gltfObject.asset.version.Contains("2.0"))
            {
                Debug.LogWarning($"Expected glTF 2.0, but this asset is using {gltfObject.asset.version}");
                return null;
            }

            gltfObject.GameObjectReference = new GameObject($"glTF Scene {gltfObject.Name}");

            for (int i = 0; i < gltfObject.buffers?.Length; i++)
            {
                gltfObject.ConstructBuffer(gltfObject.buffers[i]);
            }

            for (int i = 0; i < gltfObject.textures?.Length; i++)
            {
                gltfObject.ConstructTexture(gltfObject.textures[i]);
            }

            for (int i = 0; i < gltfObject.materials?.Length; i++)
            {
                gltfObject.ConstructMaterial(gltfObject.materials[i], i);
            }

            if (gltfObject.scenes == null)
            {
                Debug.LogError($"No scenes found for {gltfObject.Name}");
            }

            for (int i = 0; i < gltfObject.scenes?.Length; i++)
            {
                gltfObject.ConstructScene(gltfObject.scenes[i]);
            }

            return gltfObject.GameObjectReference;
        }

        private static void ConstructBuffer(this GltfObject gltfObject, GltfBuffer gltfBuffer)
        {
            var parentDirectory = Directory.GetParent(gltfObject.Uri);
            gltfBuffer.BufferData = File.ReadAllBytes($"{parentDirectory}\\{gltfBuffer.uri}");
        }

        private static void ConstructTexture(this GltfObject gltfObject, GltfTexture gltfTexture)
        {
            var parentDirectory = Directory.GetParent(gltfObject.Uri);

            if (gltfTexture.source >= 0)
            {
                GltfImage gltfImage = gltfObject.images[gltfTexture.source];

                // TODO Check if texture is in unity project, and use the asset reference instead.

                gltfImage.Texture = new Texture2D(0, 0);
                gltfImage.Texture.LoadImage(File.ReadAllBytes($"{parentDirectory}\\{gltfImage.uri}"), false);
            }
        }

        private static void ConstructMaterial(this GltfObject gltfObject, GltfMaterial gltfMaterial, int materialId)
        {
            Shader shader = Shader.Find("Standard");

            if (shader == null)
            {
                Debug.LogWarning("No Standard shader found. Falling back to Legacy Diffuse");
                shader = Shader.Find("Legacy Shaders/Diffuse");
            }

            var material = new Material(shader)
            {
                name = string.IsNullOrEmpty(gltfMaterial.name) ? $"Gltf Material {materialId}" : gltfMaterial.name
            };

            if (gltfMaterial.pbrMetallicRoughness.baseColorTexture.index >= 0)
            {
                material.mainTexture = gltfObject.images[gltfMaterial.pbrMetallicRoughness.baseColorTexture.index].Texture;
            }

            material.color = gltfMaterial.pbrMetallicRoughness.baseColorFactor.GetColorValue();

            if (gltfMaterial.alphaMode == GltfAlphaMode.MASK)
            {
                if (material.HasProperty("_ALPHATEST_ON"))
                {
                    material.SetOverrideTag("RenderType", "Cut out");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.renderQueue = (int)RenderQueue.AlphaTest;
                    material.SetFloat("_Mode", (float)gltfMaterial.alphaCutoff);
                }
            }
            else if (gltfMaterial.alphaMode == GltfAlphaMode.BLEND)
            {
                if (material.HasProperty("_ALPHAPREMULTIPLY_ON"))
                {
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)RenderQueue.Transparent;
                }
            }

            if (material.HasProperty("_MetallicGlossMap"))
            {
                // TODO if using extension handle it appropriately.

                if (gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture.index >= 0)
                {
                    var texture = gltfObject.images[gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture.index].Texture;
                    var pixels = texture.GetPixels();
                    var newPixels = new Color[pixels.Length];

                    for (int c = 0; c < pixels.Length; c++)
                    {
                        // Unity only looks for metal in R channel, and smoothness in A.
                        newPixels[c] = new Color(pixels[c].g, 0f, 0f, pixels[c].b);
                    }

                    texture.SetPixels(newPixels);
                    texture.Apply();

                    material.SetTexture("_MetallicGlossMap", gltfObject.images[gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture.index].Texture);
                }

                material.SetFloat("_Glossiness", (float)gltfMaterial.pbrMetallicRoughness.roughnessFactor);
                material.SetFloat("_Metallic", (float)gltfMaterial.pbrMetallicRoughness.metallicFactor);
                material.EnableKeyword("_MetallicGlossMap");
            }

            if (gltfMaterial.normalTexture.index >= 0 && material.HasProperty("_BumpMap"))
            {
                material.SetTexture("_BumpMap", gltfObject.images[gltfMaterial.normalTexture.index].Texture);
                material.EnableKeyword("_BumpMap");
            }

            if (gltfMaterial.emissiveTexture.index >= 0 && material.HasProperty("_EmissionMap"))
            {
                material.SetTexture("_EmissionMap", gltfObject.images[gltfMaterial.emissiveTexture.index].Texture);
                material.SetColor("_EmissionColor", gltfMaterial.emissiveFactor.GetColorValue());
                material.EnableKeyword("_EmissionMap");
                material.EnableKeyword("_EMISSION");
            }

            gltfMaterial.Material = material;
        }

        private static void ConstructScene(this GltfObject gltfObject, GltfScene gltfScene)
        {
            for (int i = 0; i < gltfScene.nodes.Length; i++)
            {
                ConstructNode(gltfObject, gltfObject.nodes[gltfScene.nodes[i]], gltfScene.nodes[i], gltfObject.GameObjectReference.transform);
            }
        }

        private static void ConstructNode(GltfObject gltfObject, GltfNode node, int nodeId, Transform parent)
        {
            var nodeGameObject = new GameObject(string.IsNullOrEmpty(node.name) ? $"glTF Node {nodeId}" : node.name);

            // If we're creating a really large node, we need it to not be visible in partial stages. So we hide it while we create it
            nodeGameObject.SetActive(false);

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;

            node.Matrix = node.GetTrsProperties(out position, out rotation, out scale);

            if (node.Matrix == Matrix4x4.identity)
            {
                if (node.translation != null)
                {
                    position = node.translation.GetVector3Value();
                }

                if (node.rotation != null)
                {
                    rotation = node.rotation.GetQuaternionValue();
                }

                if (node.scale != null)
                {
                    scale = node.scale.GetVector3Value(false);
                }
            }

            nodeGameObject.transform.localPosition = position;
            nodeGameObject.transform.localRotation = rotation;
            nodeGameObject.transform.localScale = scale;

            if (node.mesh >= 0)
            {
                ConstructMesh(gltfObject, nodeGameObject, node.mesh);
            }

            if (node.children != null)
            {
                for (int i = 0; i < node.children.Length; i++)
                {
                    ConstructNode(gltfObject, gltfObject.nodes[node.children[i]], node.children[i], nodeGameObject.transform);
                }
            }

            nodeGameObject.transform.SetParent(parent, false);
            nodeGameObject.SetActive(true);
        }

        private static void ConstructMesh(GltfObject gltfObject, GameObject parent, int meshId)
        {
            var gltfMesh = gltfObject.meshes[meshId];

            for (int i = 0; i < gltfMesh.primitives.Length; i++)
            {
                var meshPrimitive = ConstructMeshPrimitive(gltfObject, gltfMesh.primitives[i]);
                var renderer = parent.gameObject.AddComponent<MeshRenderer>();
                var filter = parent.gameObject.AddComponent<MeshFilter>();
                filter.sharedMesh = meshPrimitive;
                renderer.sharedMaterial = gltfObject.materials[gltfMesh.primitives[i].material].Material;
            }
        }

        private static Mesh ConstructMeshPrimitive(GltfObject gltfObject, GltfMeshPrimitive meshPrimitive)
        {
            GltfAccessor positionAccessor = null;
            GltfAccessor normalsAccessor = null;
            GltfAccessor textCoord0Accessor = null;
            GltfAccessor textCoord1Accessor = null;
            GltfAccessor textCoord2Accessor = null;
            GltfAccessor textCoord3Accessor = null;
            GltfAccessor colorAccessor = null;
            GltfAccessor indicesAccessor = null;
            GltfAccessor tangentAccessor = null;
            GltfAccessor weight0Accessor = null;
            GltfAccessor joint0Accessor = null;
            int vertexCount = 0;

            if (meshPrimitive.Attributes.POSITION >= 0)
            {
                positionAccessor = gltfObject.accessors[meshPrimitive.Attributes.POSITION];
                positionAccessor.BufferView = gltfObject.bufferViews[positionAccessor.bufferView];
                positionAccessor.BufferView.Buffer = gltfObject.buffers[positionAccessor.BufferView.buffer];
                vertexCount = positionAccessor.count;
            }

            if (meshPrimitive.Attributes.NORMAL >= 0)
            {
                normalsAccessor = gltfObject.accessors[meshPrimitive.Attributes.NORMAL];
                normalsAccessor.BufferView = gltfObject.bufferViews[normalsAccessor.bufferView];
                normalsAccessor.BufferView.Buffer = gltfObject.buffers[normalsAccessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_0 >= 0)
            {
                textCoord0Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_0];
                textCoord0Accessor.BufferView = gltfObject.bufferViews[textCoord0Accessor.bufferView];
                textCoord0Accessor.BufferView.Buffer = gltfObject.buffers[textCoord0Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_1 >= 0)
            {
                textCoord1Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_1];
                textCoord1Accessor.BufferView = gltfObject.bufferViews[textCoord1Accessor.bufferView];
                textCoord1Accessor.BufferView.Buffer = gltfObject.buffers[textCoord1Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_2 >= 0)
            {
                textCoord2Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_2];
                textCoord2Accessor.BufferView = gltfObject.bufferViews[textCoord2Accessor.bufferView];
                textCoord2Accessor.BufferView.Buffer = gltfObject.buffers[textCoord2Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TEXCOORD_3 >= 0)
            {
                textCoord3Accessor = gltfObject.accessors[meshPrimitive.Attributes.TEXCOORD_3];
                textCoord3Accessor.BufferView = gltfObject.bufferViews[textCoord3Accessor.bufferView];
                textCoord3Accessor.BufferView.Buffer = gltfObject.buffers[textCoord3Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.COLOR_0 >= 0)
            {
                colorAccessor = gltfObject.accessors[meshPrimitive.Attributes.COLOR_0];
                colorAccessor.BufferView = gltfObject.bufferViews[colorAccessor.bufferView];
                colorAccessor.BufferView.Buffer = gltfObject.buffers[colorAccessor.BufferView.buffer];
            }

            if (meshPrimitive.indices >= 0)
            {
                indicesAccessor = gltfObject.accessors[meshPrimitive.indices];
                indicesAccessor.BufferView = gltfObject.bufferViews[indicesAccessor.bufferView];
                indicesAccessor.BufferView.Buffer = gltfObject.buffers[indicesAccessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.TANGENT >= 0)
            {
                tangentAccessor = gltfObject.accessors[meshPrimitive.Attributes.TANGENT];
                tangentAccessor.BufferView = gltfObject.bufferViews[tangentAccessor.bufferView];
                tangentAccessor.BufferView.Buffer = gltfObject.buffers[tangentAccessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.WEIGHTS_0 >= 0)
            {
                weight0Accessor = gltfObject.accessors[meshPrimitive.Attributes.WEIGHTS_0];
                weight0Accessor.BufferView = gltfObject.bufferViews[weight0Accessor.bufferView];
                weight0Accessor.BufferView.Buffer = gltfObject.buffers[weight0Accessor.BufferView.buffer];
            }

            if (meshPrimitive.Attributes.JOINTS_0 >= 0)
            {
                joint0Accessor = gltfObject.accessors[meshPrimitive.Attributes.JOINTS_0];
                joint0Accessor.BufferView = gltfObject.bufferViews[joint0Accessor.bufferView];
                joint0Accessor.BufferView.Buffer = gltfObject.buffers[joint0Accessor.BufferView.buffer];
            }

            var mesh = new Mesh
            {
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
                vertices = positionAccessor?.GetVector3Array(),
                normals = normalsAccessor?.GetVector3Array(),
                uv = textCoord0Accessor?.GetVector2Array(),
                uv2 = textCoord1Accessor?.GetVector2Array(),
                uv3 = textCoord2Accessor?.GetVector2Array(),
                uv4 = textCoord3Accessor?.GetVector2Array(),
                colors = colorAccessor?.GetColorArray(),
                triangles = indicesAccessor?.GetIntArray(),
                tangents = tangentAccessor?.GetVector4Array()
            };

            if (weight0Accessor != null && joint0Accessor != null)
            {
                mesh.boneWeights = CreateBoneWeightArray(joint0Accessor.GetVector4Array(false), weight0Accessor.GetVector4Array(false), vertexCount);
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static BoneWeight[] CreateBoneWeightArray(Vector4[] joints, Vector4[] weights, int vertexCount)
        {
            NormalizeBoneWeightArray(weights);

            var boneWeights = new BoneWeight[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                boneWeights[i].boneIndex0 = (int)joints[i].x;
                boneWeights[i].boneIndex1 = (int)joints[i].y;
                boneWeights[i].boneIndex2 = (int)joints[i].z;
                boneWeights[i].boneIndex3 = (int)joints[i].w;

                boneWeights[i].weight0 = weights[i].x;
                boneWeights[i].weight1 = weights[i].y;
                boneWeights[i].weight2 = weights[i].z;
                boneWeights[i].weight3 = weights[i].w;
            }

            return boneWeights;
        }

        private static void NormalizeBoneWeightArray(Vector4[] weights)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                var weightSum = weights[i].x + weights[i].y + weights[i].z + weights[i].w;

                if (!Mathf.Approximately(weightSum, 0))
                {
                    weights[i] /= weightSum;
                }
            }
        }
    }
}
