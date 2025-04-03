using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using OpenCvSharp;

namespace YoloDeployPlatform.tracker
{
    public class KalmanFilter
    {
        // 状态向量 [x, y, a, h, vx, vy, va, vh]
        public Matrix<float> State { get; private set; }

        public Matrix<float> Covariance { get; private set; }

        // 状态转移矩阵
        private readonly Matrix<float> F;

        // 过程噪声协方差
        private readonly Matrix<float> Q;

        // 观测矩阵
        private readonly Matrix<float> H;

        // 观测噪声协方差
        private readonly Matrix<float> R;

        public KalmanFilter(float[] initialBbox)
        {
            // 初始化状态向量 (x,y,a,h) + 速度初始化为0
            State = Matrix<float>.Build.Dense(8, 1);
            State[0, 0] = initialBbox[0]; // x
            State[1, 0] = initialBbox[1]; // y
            State[2, 0] = initialBbox[2]; // a (aspect ratio)
            State[3, 0] = initialBbox[3]; // h (height)

            // 初始化协方差矩阵
            Covariance = Matrix<float>.Build.DenseIdentity(8);

            // 初始化状态转移矩阵 (恒定速度模型)
            F = Matrix<float>.Build.DenseIdentity(8);
            for (int i = 0; i < 4; i++)
            {
                F[i, i + 4] = 1; // 位置和速度的关系
            }

            // 过程噪声协方差
            Q = Matrix<float>.Build.DenseIdentity(8) * 0.03f;

            // 观测矩阵 (只能观测位置信息)
            H = Matrix<float>.Build.Dense(4, 8);
            for (int i = 0; i < 4; i++)
            {
                H[i, i] = 1;
            }

            // 观测噪声协方差
            R = Matrix<float>.Build.DenseIdentity(4) * 0.1f;
        }

        public void Predict()
        {
            // 状态预测
            State = F * State;

            // 协方差预测
            Covariance = F * Covariance * F.Transpose() + Q;
        }

        public void Update(float[] measurement)
        {
            var z = Matrix<float>.Build.Dense(4, 1);

            for (int i = 0; i < 4; i++)
            {
                z[i, 0] = measurement[i];
            }

            // 计算卡尔曼增益
            var y = z - H * State;
            var S = H * Covariance * H.Transpose() + R;
            var K = Covariance * H.Transpose() * S.Inverse();

            // 更新状态
            State = State + K * y;

            // 更新协方差
            Covariance = (Matrix<float>.Build.DenseIdentity(8) - K * H) * Covariance;
        }

        public float[] GetBbox()
        {
            return new float[] { State[0, 0], State[1, 0], State[2, 0], State[3, 0] };
        }

        // 从检测框转换到状态变量
        public static float[] ParseBbox(Rect bbox)
        {
            float width = bbox.Width,
                height = bbox.Height;
            return new float[4]
            {
                bbox.X + width / 2, // center x
                bbox.Y + height / 2, // center y
                width / height, // aspect ratio
                height // height
            };
        }
    }
}