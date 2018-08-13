using Gltf.Schema;
using Gltf.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Gltf.Serialization
{
    public static class GltfSerializationUtility
    {
        /// <summary>
        /// Gets a glTF object from the provided uri
        /// </summary>
        /// <param name="uri">the path to the file to load</param>
        /// <returns><see cref="GltfObject"/></returns>
        public static GltfObject GetGltfObjectFromPath(string uri)
        {
            // TODO Check if glb vs gltf
            string gltfJson = File.ReadAllText(uri);
            var gltfObject = GetGltfObjectFromJson(gltfJson);
            gltfObject.Uri = uri;
            int nameStart = uri.LastIndexOf("\\", StringComparison.Ordinal) + 1;
            int nameLength = uri.Length - nameStart;
            gltfObject.Name = uri.Substring(nameStart, nameLength).Replace(".gltf", string.Empty);
            var watch = new Stopwatch();
            watch.Start();
            ImportGltf.ImportGltfObject(gltfObject);
            watch.Stop();
            Debug.Log($"Imported GltfObject in : {watch.ElapsedMilliseconds}");
            return gltfObject;
        }

        /// <summary>
        /// Gets a glTF object from the provided json string.
        /// </summary>
        /// <param name="jsonString">String defining a glTF Object.</param>
        /// <returns><see cref="GltfObject"/></returns>
        public static GltfObject GetGltfObjectFromJson(string jsonString)
        {
            var watch = new Stopwatch();
            watch.Start();
            var gltfObject = JsonUtility.FromJson<GltfObject>(jsonString);
            watch.Stop();

            Debug.Log($"Json Deserialized in : {watch.ElapsedMilliseconds}");
            watch.Reset();
            watch.Start();

            for (int i = 0; i < gltfObject.extensionsRequired?.Length; i++)
            {
                var extensionsRequired = GetGltfExtensionObjects(jsonString, gltfObject.extensionsRequired[i]);

                foreach (var extensionRequired in extensionsRequired)
                {
                    if (gltfObject.extensionsUsed[i].Equals("KHR_materials_pbrSpecularGlossiness"))
                    {
                        for (int j = 0; j < gltfObject.materials.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(gltfObject.materials[i].name) &&
                                gltfObject.materials[i].name == extensionRequired.Key)
                            {
                                gltfObject.materials[i].Extensions.Add(gltfObject.extensionsUsed[i], extensionRequired.Value);
                                var extension = JsonUtility.FromJson<KHR_Materials_PbrSpecularGlossiness>(extensionRequired.Value);
                                extension.ElementName = gltfObject.materials[i].name;
                                gltfObject.RegisteredExtensions.Add(extension);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < gltfObject.extensionsUsed?.Length; i++)
            {
                var extensionsUsed = GetGltfExtensionObjects(jsonString, gltfObject.extensionsUsed[i]);

                foreach (var extensionUsed in extensionsUsed)
                {
                    if (gltfObject.extensionsUsed[i].Equals("KHR_materials_pbrSpecularGlossiness"))
                    {
                        for (int j = 0; j < gltfObject.materials.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(gltfObject.materials[i].name) &&
                                gltfObject.materials[i].name == extensionUsed.Key)
                            {
                                gltfObject.materials[i].Extensions.Add(gltfObject.extensionsUsed[i], extensionUsed.Value);
                                var extension = JsonUtility.FromJson<KHR_Materials_PbrSpecularGlossiness>(extensionUsed.Value);
                                extension.ElementName = gltfObject.materials[i].name;
                                gltfObject.RegisteredExtensions.Add(extension);
                            }
                        }
                    }
                }
            }

            watch.Stop();
            Debug.Log($"Extensions deserialized in : {watch.ElapsedMilliseconds}");

            watch.Reset();
            watch.Start();

            var meshPrimitiveAttributes = GetGltfMeshPrimitiveAttributes(jsonString);

            for (int i = 0; i < gltfObject.meshes.Length; i++)
            {
                for (int j = 0; j < gltfObject.meshes[i].primitives.Length; j++)
                {
                    gltfObject.meshes[i].primitives[j].Attributes = JsonUtility.FromJson<GltfMeshPrimitiveAttributes>(meshPrimitiveAttributes[0]);
                    meshPrimitiveAttributes.Remove(meshPrimitiveAttributes[0]);
                }
            }

            watch.Stop();
            Debug.Log($"Mesh Primitives deserialized in : {watch.ElapsedMilliseconds}");

            return gltfObject;
        }

        /// <summary>
        /// Get a single Json Object using the handle provided.
        /// </summary>
        /// <param name="jsonString">The json string to search.</param>
        /// <param name="handle">The handle to look for.</param>
        /// <returns>A snippet of the json string that defines the object.</returns>
        private static string GetJsonObject(string jsonString, string handle)
        {
            var regex = new Regex($"\"{handle}\"\\s*:\\s*\\{{");
            var match = regex.Match(jsonString);
            return match.Success ? GetJsonObject(jsonString, match.Index + match.Length) : null;
        }

        private static List<string> GetGltfMeshPrimitiveAttributes(string jsonString)
        {
            var regex = new Regex("(?<Attributes>\"attributes\"[^}]+})");
            return GetGltfMeshPrimitiveAttributes(jsonString, regex);
        }

        private static List<string> GetGltfMeshPrimitiveAttributes(string jsonString, Regex regex)
        {
            var jsonObjects = new List<string>();

            if (!regex.IsMatch(jsonString))
            {
                return jsonObjects;
            }

            MatchCollection matches = regex.Matches(jsonString);

            for (var i = 0; i < matches.Count; i++)
            {
                jsonObjects.Add(matches[i].Groups["Attributes"].Captures[0].Value.Replace("\"attributes\": ", string.Empty));
            }

            return jsonObjects;
        }

        /// <summary>
        /// Get a collection of glTF Extensions using the handle provided.
        /// </summary>
        /// <param name="jsonString">The json string to search.</param>
        /// <param name="handle">The handle to look for.</param>
        /// <returns>A collection of snippets with the json string that defines the object.</returns>
        private static Dictionary<string, string> GetGltfExtensionObjects(string jsonString, string handle)
        {
            var regex = new Regex($"(\"name\":\\s*\"\\w*\",\\s*\"extensions\":\\s*{{\\s*?)(\"{handle}\"\\s*:\\s*{{)");
            return GetGltfExtensions(jsonString, regex);
        }

        /// <summary>
        /// Get a collection of glTF Extras using the handle provided.
        /// </summary>
        /// <param name="jsonString">The json string to search.</param>
        /// <param name="handle">The handle to look for.</param>
        /// <returns>A collection of snippets with the json string that defines the object.</returns>
        public static Dictionary<string, string> GetGltfExtraObjects(string jsonString, string handle)
        {
            var regex = new Regex($"(\"name\":\\s*\"\\w*\",\\s*\"extras\":\\s*{{\\s*?)(\"{handle}\"\\s*:\\s*{{)");
            return GetGltfExtensions(jsonString, regex);
        }

        private static Dictionary<string, string> GetGltfExtensions(string jsonString, Regex regex)
        {
            var jsonObjects = new Dictionary<string, string>();

            if (!regex.IsMatch(jsonString))
            {
                return jsonObjects;
            }

            var matches = regex.Matches(jsonString);
            var nodeName = string.Empty;

            for (var i = 0; i < matches.Count; i++)
            {
                for (int j = 0; j < matches[i].Groups.Count; j++)
                {
                    for (int k = 0; k < matches[i].Groups[i].Captures.Count; k++)
                    {
                        nodeName = GetGltfNodeName(matches[i].Groups[i].Captures[i].Value);
                    }
                }

                if (!jsonObjects.ContainsKey(nodeName))
                {
                    jsonObjects.Add(nodeName, GetJsonObject(jsonString, matches[i].Index + matches[i].Length));
                }
            }

            return jsonObjects;
        }

        private static string GetJsonObject(string jsonString, int startOfObject)
        {
            int index;
            int bracketCount = 1;

            for (index = startOfObject; bracketCount > 0; index++)
            {
                if (jsonString[index] == '{')
                {
                    bracketCount++;
                }
                else if (jsonString[index] == '}')
                {
                    bracketCount--;
                }
            }

            return $"{{{jsonString.Substring(startOfObject, index - startOfObject)}";
        }

        private static string GetGltfNodeName(string jsonString)
        {
            jsonString = jsonString.Replace("\"name\"", string.Empty);
            jsonString = jsonString.Replace(": \"", string.Empty);
            jsonString = jsonString.Replace(":\"", string.Empty);
            jsonString = jsonString.Substring(0, jsonString.IndexOf("\"", StringComparison.Ordinal));
            return jsonString;
        }
    }
}