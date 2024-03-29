# Attention

All work has stopped in this repository and the newest fixes/additions can be found in 

[XRTK.Utilities.Gltf](https://github.com/XRTK/XRTK-Core/tree/master/XRTK-Core/Assets/XRTK/Utilities/Gltf)

# glTF-Unity
Unity [glTF 2.0](https://github.com/KhronosGroup/glTF) serialization using `UnityEditor.JsonUtility`.

- [x] Import Asset
    - [x] Import asset at runtime
    - [ ] Custom asset importer for editor
    - [x] .gltf file extension
    - [ ] .glb file extension
    - [x] Json string
    - [x] Construct GameObject from GltfObject Data
    - [x] Apply texture to GameObject
    - [x] Apply PBR Settings to material
    - [ ] Import skins
    - [ ] Import Animations
    - [ ] Import Cameras
    - [ ] Apply extension properties to imported asset.
- [ ] Export Asset
    - [ ] Export asset at runtime
    - [ ] Export asset from editor
    - [ ] .gltf file extension
    - [ ] .glb file extension

# Supported Extensions
- [ ] KHR_materials_pbrSpecularGlossiness

# FAQ
To load gltf or glb files at runtime without a custom shader, you'll need to make sure that the standard shader is included in your final build with each of the required variants compiled and located either in your resources folder, or used in your scene. For more information check out the [Unity Docs explaining how to access shader properties at runtime](https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html).

## To build a glTF-Object from json:
```
GltfObject gltfObject = GltfSerializationUtility.GetGltfObjectFromJson(json);
```

## To build a glTF-Object from uri:
```
GltfObject gltfObject = GltfSerializationUtility.GetGltfObjectFromPath(uri);
```

## To Import GlFT-Object:
```
await ImportGltf.ImportGltfObjectAsync(gltfObject);
```

## To access built Gltf GameObject scene reference
```
gltfObject.GameObjectReference
```
