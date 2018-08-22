using Gltf.Schema;
using Gltf.Serialization;
using UnityEngine;

namespace Gltf.Components
{
    public class GltfComponent : MonoBehaviour
    {
        [SerializeField]
        private string uri = string.Empty;

        public GameObject ImportedGltf;

        private void Awake()
        {
            GltfObject gltfObject = GltfSerializationUtility.GetGltfObjectFromPath(Application.streamingAssetsPath + "/" + uri);

            if (gltfObject == null)
            {
                Debug.LogError("Failed to load gltf object");
            }
            else
            {
                Debug.Log("Import successful");
            }
        }
    }
}