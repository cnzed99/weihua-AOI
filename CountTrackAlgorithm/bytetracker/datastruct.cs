using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YoloDeployPlatform.Bytetrack
{
    // 5. 辅助数学结构
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector8
    {
        public float X,
            Y,
            Z,
            W,
            VX,
            VY,
            VZ,
            VW;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector8(float x, float y, float z, float w, float vx, float vy, float vz, float vw)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            VX = vx;
            VY = vy;
            VZ = vz;
            VW = vw;
        }

        // 索引器实现
        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return index switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    3 => W,
                    4 => VX,
                    5 => VY,
                    6 => VZ,
                    7 => VW,
                    _ => throw new IndexOutOfRangeException("Vector8 index must be between 0 and 7")
                };
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;

                    case 1:
                        Y = value;
                        break;

                    case 2:
                        Z = value;
                        break;

                    case 3:
                        W = value;
                        break;

                    case 4:
                        VX = value;
                        break;

                    case 5:
                        VY = value;
                        break;

                    case 6:
                        VZ = value;
                        break;

                    case 7:
                        VW = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException("Vector8 index must be between 0 and 7");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 operator +(Vector8 a, Vector8 b) =>
            new(
                a.X + b.X,
                a.Y + b.Y,
                a.Z + b.Z,
                a.W + b.W,
                a.VX + b.VX,
                a.VY + b.VY,
                a.VZ + b.VZ,
                a.VW + b.VW
            );

        // 实现Transform方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector8 Transform(Vector8 vector, Matrix8x8 matrix)
        {
            Vector8 result = new();

            // 手动展开循环以提高性能
            result.X =
                vector.X * matrix[0, 0]
                + vector.Y * matrix[0, 1]
                + vector.Z * matrix[0, 2]
                + vector.W * matrix[0, 3]
                + vector.VX * matrix[0, 4]
                + vector.VY * matrix[0, 5]
                + vector.VZ * matrix[0, 6]
                + vector.VW * matrix[0, 7];

            result.Y =
                vector.X * matrix[1, 0]
                + vector.Y * matrix[1, 1]
                + vector.Z * matrix[1, 2]
                + vector.W * matrix[1, 3]
                + vector.VX * matrix[1, 4]
                + vector.VY * matrix[1, 5]
                + vector.VZ * matrix[1, 6]
                + vector.VW * matrix[1, 7];

            result.Z =
                vector.X * matrix[2, 0]
                + vector.Y * matrix[2, 1]
                + vector.Z * matrix[2, 2]
                + vector.W * matrix[2, 3]
                + vector.VX * matrix[2, 4]
                + vector.VY * matrix[2, 5]
                + vector.VZ * matrix[2, 6]
                + vector.VW * matrix[2, 7];

            result.W =
                vector.X * matrix[3, 0]
                + vector.Y * matrix[3, 1]
                + vector.Z * matrix[3, 2]
                + vector.W * matrix[3, 3]
                + vector.VX * matrix[3, 4]
                + vector.VY * matrix[3, 5]
                + vector.VZ * matrix[3, 6]
                + vector.VW * matrix[3, 7];

            result.VX =
                vector.X * matrix[4, 0]
                + vector.Y * matrix[4, 1]
                + vector.Z * matrix[4, 2]
                + vector.W * matrix[4, 3]
                + vector.VX * matrix[4, 4]
                + vector.VY * matrix[4, 5]
                + vector.VZ * matrix[4, 6]
                + vector.VW * matrix[4, 7];

            result.VY =
                vector.X * matrix[5, 0]
                + vector.Y * matrix[5, 1]
                + vector.Z * matrix[5, 2]
                + vector.W * matrix[5, 3]
                + vector.VX * matrix[5, 4]
                + vector.VY * matrix[5, 5]
                + vector.VZ * matrix[5, 6]
                + vector.VW * matrix[5, 7];

            result.VZ =
                vector.X * matrix[6, 0]
                + vector.Y * matrix[6, 1]
                + vector.Z * matrix[6, 2]
                + vector.W * matrix[6, 3]
                + vector.VX * matrix[6, 4]
                + vector.VY * matrix[6, 5]
                + vector.VZ * matrix[6, 6]
                + vector.VW * matrix[6, 7];

            result.VW =
                vector.X * matrix[7, 0]
                + vector.Y * matrix[7, 1]
                + vector.Z * matrix[7, 2]
                + vector.W * matrix[7, 3]
                + vector.VX * matrix[7, 4]
                + vector.VY * matrix[7, 5]
                + vector.VZ * matrix[7, 6]
                + vector.VW * matrix[7, 7];

            return result;
        }

        // 添加运算符重载以便更自然地使用
        public static Vector8 operator *(Matrix8x8 matrix, Vector8 vector) =>
            Transform(vector, matrix);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix8x8
    {
        private float[,] values = new float[8, 8];

        public Matrix8x8()
        { }

        public float this[int row, int col]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => values[row, col];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => values[row, col] = value;
        }

        public static Matrix8x8 Identity
        {
            get
            {
                var m = new Matrix8x8();
                for (int i = 0; i < 8; i++)
                    m[i, i] = 1;
                return m;
            }
        }

        public static Matrix8x8 operator *(Matrix8x8 a, Matrix8x8 b)
        {
            Matrix8x8 result = new();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j += 4) // 每次处理4列
                {
                    Vector4 sum = Vector4.Zero;

                    // 计算4列的块
                    for (int k = 0; k < 8; k++)
                    {
                        Vector4 aVec = new(a[i, k]); // 重复a[i,k]到4个分量
                        Vector4 bVec = new(b[k, j], b[k, j + 1], b[k, j + 2], b[k, j + 3]);
                        sum += aVec * bVec;
                    }

                    // 存储结果
                    result[i, j] = sum.X;
                    result[i, j + 1] = sum.Y;
                    result[i, j + 2] = sum.Z;
                    result[i, j + 3] = sum.W;
                }
            }
            return result;
        }

        // 矩阵减法

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix8x8 operator -(Matrix8x8 a, Matrix8x8 b)
        {
            Matrix8x8 result = new();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return result;
        }

        public static Matrix8x4 operator *(Matrix8x8 a, Matrix8x4 b)
        {
            Matrix8x4 result = new();

            // 转置b矩阵的4列为连续内存块（优化内存访问）
            Span<Vector4> bColumns = stackalloc Vector4[4];
            for (int j = 0; j < 4; j++)
            {
                bColumns[j] = new Vector4(b[0, j], b[1, j], b[2, j], b[3, j]); // 前4行
            }

            for (int i = 0; i < 8; i++) // 遍历结果行
            {
                // 加载a矩阵的当前行（分两段处理）
                Vector4 aRowLow = new(a[i, 0], a[i, 1], a[i, 2], a[i, 3]);
                Vector4 aRowHigh = new(a[i, 4], a[i, 5], a[i, 6], a[i, 7]);

                for (int j = 0; j < 4; j++) // 遍历结果列
                {
                    // 计算内积（SIMD加速）
                    Vector4 bCol = bColumns[j];
                    float sum = Vector4.Dot(aRowLow, bCol);

                    // 处理高4位（若Matrix8x4支持>4行）
                    if (8 > 4) // 实际总是执行
                    {
                        Vector4 bColHigh = new(b[4, j], b[5, j], b[6, j], b[7, j]);
                        sum += Vector4.Dot(aRowHigh, bColHigh);
                    }

                    result[i, j] = sum;
                }
            }

            return result;
        }

        // 转置方法
        public Matrix8x8 Transpose()
        {
            Matrix8x8 result = new();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    result[j, i] = this[i, j];
            return result;
        }
    }

    public struct Matrix4x8
    {
        private float[,] data = new float[4, 8];

        public Matrix4x8()
        { }

        public static Matrix4x8 Identity
        {
            get
            {
                var m = new Matrix4x8();
                for (int i = 0; i < 4; i++)
                    m[i, i] = 1.0f;
                return m;
            }
        }

        public float this[int row, int col]
        {
            get => data[row, col];
            set => data[row, col] = value;
        }

        public Matrix8x4 Transpose()
        {
            Matrix8x4 result = new Matrix8x4();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 8; j++)
                    result[j, i] = this[i, j];
            return result;
        }

        public static Matrix4x4 operator *(Matrix4x8 a, Matrix8x4 b)
        {
            Matrix4x4 result = new();
            // 将b矩阵的列转换为Vector4数组（优化内存访问）
            Span<Vector4> bColumns = stackalloc Vector4[4];
            for (int j = 0; j < 4; j++)
            {
                bColumns[j] = new Vector4(b[0, j], b[1, j], b[2, j], b[3, j]); // 前4行
            }

            for (int i = 0; i < 4; i++) // 遍历结果行
            {
                // 加载a矩阵的当前行（分两段处理）
                Vector4 aRowFirst = new(a[i, 0], a[i, 1], a[i, 2], a[i, 3]);
                Vector4 aRowSecond = new(a[i, 4], a[i, 5], a[i, 6], a[i, 7]);

                for (int j = 0; j < 4; j++) // 遍历结果列
                {
                    // 计算前4行贡献
                    Vector4 bColFirst = bColumns[j];
                    float sum = Vector4.Dot(aRowFirst, bColFirst);

                    // 计算后4行贡献
                    Vector4 bColSecond = new(b[4, j], b[5, j], b[6, j], b[7, j]);
                    sum += Vector4.Dot(aRowSecond, bColSecond);

                    result[i, j] = sum;
                }
            }
            return result;
        }

        public static Matrix8x8 operator *(Matrix8x4 a, Matrix4x8 b)
        {
            Matrix8x8 result = new Matrix8x8();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // 手动展开4次乘法
                    result[i, j] =
                        a[i, 0] * b[0, j]
                        + a[i, 1] * b[1, j]
                        + a[i, 2] * b[2, j]
                        + a[i, 3] * b[3, j];
                }
            }
            return result;
        }

        public static Matrix4x8 operator *(Matrix4x8 a, Matrix8x8 b)
        {
            Matrix4x8 result = new Matrix4x8();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Vector4 sumVec = Vector4.Zero;

                    // 每次处理4个元素（SIMD优化）
                    for (int k = 0; k < 8; k += 4)
                    {
                        Vector4 aVec = new(a[i, k], a[i, k + 1], a[i, k + 2], a[i, k + 3]);
                        Vector4 bVec = new(b[k, j], b[k + 1, j], b[k + 2, j], b[k + 3, j]);
                        sumVec += aVec * bVec;
                    }

                    result[i, j] = sumVec.X + sumVec.Y + sumVec.Z + sumVec.W;
                }
            }

            return result;
        }

        public static Vector4 operator *(Matrix4x8 m, Vector8 v)
        {
            Vector4 result = new();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 8; j++)
                    result[i] += m[i, j] * v[j];
            return result;
        }
    }

    public struct Matrix8x4
    {
        private float[,] data = new float[8, 4]; // 8行4列

        public Matrix8x4()
        { }

        // 二维索引器
        public float this[int row, int col]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (row < 0 || row >= 8 || col < 0 || col >= 4)
                    throw new IndexOutOfRangeException("Matrix8x4 index out of range");
                return data[row, col];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (row < 0 || row >= 8 || col < 0 || col >= 4)
                    throw new IndexOutOfRangeException("Matrix8x4 index out of range");
                data[row, col] = value;
            }
        }

        public static Matrix8x4 operator *(Matrix8x4 a, Matrix4x4 b)
        {
            Matrix8x4 result = new();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            }

            return result;
        }

        public static Vector8 operator *(Matrix8x4 m, Vector4 v)
        {
            Vector8 result = new();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 4; j++)
                    result[i] += m[i, j] * v[j];
            return result;
        }

        // 转置为Matrix4x8
        public Matrix4x8 Transpose()
        {
            Matrix4x8 result = new();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 4; j++)
                    result[j, i] = this[i, j];
            return result;
        }
    }

    public struct Matrix4x4
    {
        private float[,] data = new float[4, 4];

        public Matrix4x4()
        { }

        public float this[int row, int col]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data[row, col];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => data[row, col] = value;
        }

        public static Matrix4x4 Identity
        {
            get
            {
                Matrix4x4 m = new();
                for (int i = 0; i < 4; i++)
                    m[i, i] = 1.0f;
                return m;
            }
        }

        // 矩阵求逆实现
        public Matrix4x4 Inverse()
        {
            Matrix4x4 result = new();

            // 计算所有2x2子矩阵的行列式
            float[,] minors = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    // 获取3x3子矩阵
                    float[,] subMatrix = new float[3, 3];
                    int si = 0;
                    for (int ii = 0; ii < 4; ii++)
                    {
                        if (ii == i)
                            continue;
                        int sj = 0;
                        for (int jj = 0; jj < 4; jj++)
                        {
                            if (jj == j)
                                continue;
                            subMatrix[si, sj] = this[ii, jj];
                            sj++;
                        }
                        si++;
                    }

                    // 计算3x3行列式
                    float det =
                        subMatrix[0, 0]
                            * (
                                subMatrix[1, 1] * subMatrix[2, 2]
                                - subMatrix[1, 2] * subMatrix[2, 1]
                            )
                        - subMatrix[0, 1]
                            * (
                                subMatrix[1, 0] * subMatrix[2, 2]
                                - subMatrix[1, 2] * subMatrix[2, 0]
                            )
                        + subMatrix[0, 2]
                            * (
                                subMatrix[1, 0] * subMatrix[2, 1]
                                - subMatrix[1, 1] * subMatrix[2, 0]
                            );

                    minors[i, j] = (float)Math.Pow(-1, i + j) * det;
                }
            }

            // 计算行列式
            float determinant =
                this[0, 0] * minors[0, 0]
                + this[0, 1] * minors[0, 1]
                + this[0, 2] * minors[0, 2]
                + this[0, 3] * minors[0, 3];

            if (Math.Abs(determinant) < float.Epsilon)
                return Matrix4x4.Identity;

            // 计算伴随矩阵并转置
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[j, i] = minors[i, j] / determinant;
                }
            }

            return result;
        }

        // 矩阵加法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator +(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return result;
        }

        // 矩阵减法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator -(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return result;
        }

        // 矩阵乘法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            }
            return result;
        }

        // 标量乘法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator *(Matrix4x4 a, float scalar)
        {
            Matrix4x4 result = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = a[i, j] * scalar;
                }
            }
            return result;
        }
    }
}