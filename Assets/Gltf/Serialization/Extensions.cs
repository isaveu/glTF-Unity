﻿using System;
using Gltf.Schema;
using UnityEngine;

namespace Gltf.Serialization
{
    public static class GltfConversions
    {
        // glTF matrix: column vectors, column-major storage, +Y up, +Z forward, -X right, right-handed
        // unity matrix: column vectors, column-major storage, +Y up, +Z forward, +X right, left-handed
        // multiply by a negative X scale to convert handedness
        public static readonly Vector3 CoordinateSpaceConversionScale = new Vector3(-1, 1, 1);

        public static readonly Vector4 TangentSpaceConversionScale = new Vector4(-1, 1, 1, -1);

        public static Matrix4x4 GetTrsProperties(this GltfNode node, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            Matrix4x4 matrix = Matrix4x4.identity;

            if (node.matrix != null && node.matrix.Length == 16)
            {
                matrix = node.matrix.GetMatrix4X4Value();
            }

            if (!node.useTRS)
            {
                matrix.GetTrsProperties(out position, out rotation, out scale);
            }
            else
            {
                position = node.translation.GetVector3Value();
                rotation = node.rotation.GetQuaternionValue();
                scale = node.scale.GetVector3Value(false);
            }

            return matrix;
        }

        public static Color GetColorValue(this float[] colorArray)
        {
            return new Color(colorArray[0], colorArray[1], colorArray[2], colorArray.Length < 4 ? 1f : colorArray[3]);
        }

        public static float[] SetColorValue(this Color color)
        {
            return new[] { color.r, color.g, color.b, color.a };
        }

        public static Vector2 GetVector2Value(this float[] vector2Array)
        {
            return new Vector2(vector2Array[0], vector2Array[1]);
        }

        public static float[] SetVector2Value(this Vector2 vector)
        {
            return new[] { vector.x, vector.y };
        }

        public static Vector3 GetVector3Value(this float[] vector3Array, bool convert = true)
        {
            var vector = new Vector3(vector3Array[0], vector3Array[1], vector3Array[2]);
            return convert ? Vector3.Scale(vector, CoordinateSpaceConversionScale) : vector;
        }

        public static float[] SetVector3Value(this Vector3 vector, bool convert = true)
        {
            if (convert)
            {
                vector = Vector3.Scale(vector, CoordinateSpaceConversionScale);
            }

            return new[] { vector.x, vector.y, vector.z };
        }

        public static Quaternion GetQuaternionValue(this float[] quaternionArray, bool convert = true)
        {
            var axes = new Vector3(quaternionArray[0], quaternionArray[1], quaternionArray[2]);

            if (convert)
            {
                axes = Vector3.Scale(axes, CoordinateSpaceConversionScale) * -1.0f;
            }

            return new Quaternion(axes.x, axes.y, axes.z, quaternionArray[3]);
        }

        public static float[] SetQuaternionValue(this Quaternion quaternion, bool convert = true)
        {
            // get the original axis and apply conversion scale as well as potential rotation axis flip
            var axes = new Vector3(quaternion.x, quaternion.y, quaternion.z);

            if (convert)
            {
                axes = Vector3.Scale(axes, CoordinateSpaceConversionScale) * 1.0f;
            }

            return new[] { axes.x, axes.y, axes.z, quaternion.w };
        }

        public static Matrix4x4 GetMatrix4X4Value(this double[] matrixArray)
        {
            var matrix = new Matrix4x4(
                new Vector4((float)matrixArray[0], (float)matrixArray[1], (float)matrixArray[2], (float)matrixArray[3]),
                new Vector4((float)matrixArray[4], (float)matrixArray[5], (float)matrixArray[6], (float)matrixArray[7]),
                new Vector4((float)matrixArray[8], (float)matrixArray[9], (float)matrixArray[10], (float)matrixArray[11]),
                new Vector4((float)matrixArray[12], (float)matrixArray[13], (float)matrixArray[14], (float)matrixArray[15]));
            Matrix4x4 convert = Matrix4x4.Scale(CoordinateSpaceConversionScale);
            return convert * matrix * convert;
        }

        public static float[] SetMatrix4X4Value(this Matrix4x4 matrix)
        {
            var convert = Matrix4x4.Scale(CoordinateSpaceConversionScale);
            matrix = convert * matrix * convert;
            return new[]
            {
                matrix.m00, matrix.m10, matrix.m20, matrix.m30,
                matrix.m01, matrix.m11, matrix.m21, matrix.m31,
                matrix.m02, matrix.m12, matrix.m22, matrix.m32,
                matrix.m03, matrix.m13, matrix.m23, matrix.m33
            };
        }

        public static void GetTrsProperties(this Matrix4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = matrix.GetColumn(3);

            Vector3 x = matrix.GetColumn(0);
            Vector3 y = matrix.GetColumn(1);
            Vector3 z = matrix.GetColumn(2);

            Vector3 calculatedZ = Vector3.Cross(x, y);
            bool mirrored = Vector3.Dot(calculatedZ, z) < 0.0f;

            scale.x = x.magnitude * (mirrored ? -1.0f : 1.0f);
            scale.y = y.magnitude;
            scale.z = z.magnitude;

            rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }

        public static int[] GetIntArray(this GltfAccessor accessor, bool flipFaces = true)
        {
            if (accessor.type != "SCALAR")
            {
                return null;
            }

            var array = new int[accessor.count];

            int componentSize;
            float maxValue;
            GetTypeDetails(accessor.componentType, out componentSize, out maxValue);
            var stride = accessor.BufferView.byteStride > 0 ? accessor.BufferView.byteStride : componentSize;
            var byteOffset = accessor.BufferView.byteOffset;
            var bufferData = accessor.BufferView.Buffer.BufferData;

            if (accessor.byteOffset >= 0)
            {
                byteOffset += accessor.byteOffset;
            }

            for (int i = 0; i < accessor.count; i++)
            {
                if (accessor.componentType == GltfComponentType.Float)
                {
                    array[i] = (int)Mathf.Floor(BitConverter.ToSingle(bufferData, byteOffset + i * stride));
                }
                else
                {
                    array[i] = (int)GetDiscreteUnsignedElement(bufferData, byteOffset + i * stride, accessor.componentType);
                }
            }

            if (flipFaces)
            {
                for (int i = 0; i < array.Length; i += 3)
                {
                    var temp = array[i];
                    array[i] = array[i + 2];
                    array[i + 2] = temp;
                }
            }

            return array;
        }

        public static Vector2[] GetVector2Array(this GltfAccessor accessor, bool flip = true)
        {
            if (accessor.type != "VEC2" || accessor.componentType == GltfComponentType.UnsignedInt)
            {
                return null;
            }

            var array = new Vector2[accessor.count];

            int componentSize;
            float maxValue;
            GetTypeDetails(accessor.componentType, out componentSize, out maxValue);
            var stride = accessor.BufferView.byteStride > 0 ? accessor.BufferView.byteStride : componentSize * 2;
            var byteOffset = accessor.BufferView.byteOffset;
            var bufferData = accessor.BufferView.Buffer.BufferData;

            if (accessor.byteOffset >= 0)
            {
                byteOffset += accessor.byteOffset;
            }

            if (accessor.normalized) { maxValue = 1; }

            for (int i = 0; i < accessor.count; i++)
            {
                if (accessor.componentType == GltfComponentType.Float)
                {
                    array[i].x = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 0);
                    array[i].y = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 1);
                }
                else
                {
                    array[i].x = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 0, accessor.componentType) / maxValue;
                    array[i].y = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 1, accessor.componentType) / maxValue;
                }

                if (flip)
                {
                    array[i].y = 1.0f - array[i].y;
                }
            }

            return array;
        }

        public static Vector3[] GetVector3Array(this GltfAccessor accessor, bool convert = true)
        {
            if (accessor.type != "VEC3" || accessor.componentType == GltfComponentType.UnsignedInt)
            {
                return null;
            }

            var array = new Vector3[accessor.count];

            int componentSize;
            float maxValue;
            GetTypeDetails(accessor.componentType, out componentSize, out maxValue);
            var stride = accessor.BufferView.byteStride > 0 ? accessor.BufferView.byteStride : componentSize * 3;
            var byteOffset = accessor.BufferView.byteOffset;
            var bufferData = accessor.BufferView.Buffer.BufferData;

            if (accessor.byteOffset >= 0)
            {
                byteOffset += accessor.byteOffset;
            }

            if (accessor.normalized) { maxValue = 1; }

            for (int i = 0; i < accessor.count; i++)
            {
                if (accessor.componentType == GltfComponentType.Float)
                {
                    array[i].x = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 0);
                    array[i].y = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 1);
                    array[i].z = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 2);
                }
                else
                {
                    array[i].x = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 0, accessor.componentType) / maxValue;
                    array[i].y = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 1, accessor.componentType) / maxValue;
                    array[i].z = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 2, accessor.componentType) / maxValue;
                }

                if (convert)
                {
                    array[i].x *= CoordinateSpaceConversionScale.x;
                    array[i].y *= CoordinateSpaceConversionScale.y;
                    array[i].z *= CoordinateSpaceConversionScale.z;
                }
            }

            return array;
        }

        public static Vector4[] GetVector4Array(this GltfAccessor accessor, bool convert = true)
        {
            if (accessor.type != "VEC4" || accessor.componentType == GltfComponentType.UnsignedInt)
            {
                return null;
            }

            var array = new Vector4[accessor.count];

            int componentSize;
            float maxValue;
            GetTypeDetails(accessor.componentType, out componentSize, out maxValue);
            var stride = accessor.BufferView.byteStride > 0 ? accessor.BufferView.byteStride : componentSize * 4;
            var byteOffset = accessor.BufferView.byteOffset;
            var bufferData = accessor.BufferView.Buffer.BufferData;

            if (accessor.byteOffset >= 0)
            {
                byteOffset += accessor.byteOffset;
            }

            if (accessor.normalized) { maxValue = 1; }

            for (int i = 0; i < accessor.count; i++)
            {
                if (accessor.componentType == GltfComponentType.Float)
                {
                    array[i].x = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 0);
                    array[i].y = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 1);
                    array[i].z = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 2);
                    array[i].w = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 3);
                }
                else
                {
                    array[i].x = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 0, accessor.componentType) / maxValue;
                    array[i].y = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 1, accessor.componentType) / maxValue;
                    array[i].z = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 2, accessor.componentType) / maxValue;
                    array[i].w = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 3, accessor.componentType) / maxValue;
                }

                if (convert)
                {
                    array[i].x *= TangentSpaceConversionScale.x;
                    array[i].y *= TangentSpaceConversionScale.y;
                    array[i].z *= TangentSpaceConversionScale.z;
                    array[i].w *= TangentSpaceConversionScale.w;
                }
            }

            return array;
        }

        public static Color[] GetColorArray(this GltfAccessor accessor)
        {
            if (accessor.type != "VEC3" && accessor.type != "VEC4" || accessor.componentType == GltfComponentType.UnsignedInt)
            {
                return null;
            }

            var array = new Color[accessor.count];

            int componentSize;
            float maxValue;
            GetTypeDetails(accessor.componentType, out componentSize, out maxValue);
            bool hasAlpha = accessor.type == "VEC4";

            var stride = accessor.BufferView.byteStride > 0 ? accessor.BufferView.byteStride : componentSize * (hasAlpha ? 4 : 3);
            var byteOffset = accessor.BufferView.byteOffset;
            var bufferData = accessor.BufferView.Buffer.BufferData;

            if (accessor.byteOffset >= 0)
            {
                byteOffset += accessor.byteOffset;
            }

            for (int i = 0; i < accessor.count; i++)
            {
                if (accessor.componentType == GltfComponentType.Float)
                {
                    array[i].r = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 0);
                    array[i].g = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 1);
                    array[i].b = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 2);
                    array[i].a = hasAlpha ? BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 3) : 1f;
                }
                else
                {
                    array[i].r = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 0, accessor.componentType) / maxValue;
                    array[i].g = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 1, accessor.componentType) / maxValue;
                    array[i].b = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 2, accessor.componentType) / maxValue;
                    array[i].a = hasAlpha ? GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 3, accessor.componentType) / maxValue : 1f;
                }
            }

            return array;
        }

        private static void GetTypeDetails(GltfComponentType type, out int componentSize, out float maxValue)
        {
            componentSize = 1;
            maxValue = byte.MaxValue;

            switch (type)
            {
                case GltfComponentType.Byte:
                    componentSize = sizeof(sbyte);
                    maxValue = sbyte.MaxValue;
                    break;
                case GltfComponentType.UnsignedByte:
                    componentSize = sizeof(byte);
                    maxValue = byte.MaxValue;
                    break;
                case GltfComponentType.Short:
                    componentSize = sizeof(short);
                    maxValue = short.MaxValue;
                    break;
                case GltfComponentType.UnsignedShort:
                    componentSize = sizeof(ushort);
                    maxValue = ushort.MaxValue;
                    break;
                case GltfComponentType.UnsignedInt:
                    componentSize = sizeof(uint);
                    maxValue = uint.MaxValue;
                    break;
                case GltfComponentType.Float:
                    componentSize = sizeof(float);
                    maxValue = float.MaxValue;
                    break;
                default:
                    throw new Exception("Unsupported component type.");
            }
        }

        private static int GetDiscreteElement(byte[] data, int offset, GltfComponentType type)
        {
            switch (type)
            {
                case GltfComponentType.Byte:
                    return Convert.ToSByte(data[offset]);
                case GltfComponentType.UnsignedByte:
                    return data[offset];
                case GltfComponentType.Short:
                    return BitConverter.ToInt16(data, offset);
                case GltfComponentType.UnsignedShort:
                    return BitConverter.ToUInt16(data, offset);
                case GltfComponentType.UnsignedInt:
                    return (int)BitConverter.ToUInt32(data, offset);
                default:
                    throw new Exception($"Unsupported type passed in: {type}");
            }
        }

        private static uint GetDiscreteUnsignedElement(byte[] data, int offset, GltfComponentType type)
        {
            switch (type)
            {
                case GltfComponentType.Byte:
                    return (uint)Convert.ToSByte(data[offset]);
                case GltfComponentType.UnsignedByte:
                    return data[offset];
                case GltfComponentType.Short:
                    return (uint)BitConverter.ToInt16(data, offset);
                case GltfComponentType.UnsignedShort:
                    return BitConverter.ToUInt16(data, offset);
                case GltfComponentType.UnsignedInt:
                    return BitConverter.ToUInt32(data, offset);
                default:
                    throw new Exception($"Unsupported type passed in: {type}");
            }
        }
    }
}
