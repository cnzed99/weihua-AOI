using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloobbAlgorithm
{
    public static class MyEnum
    {
        public static T GetModelType<T>(string strType)
        {
            T t = (T)Enum.Parse(typeof(T), strType);
            return t;
        }

        public static T GetEngineType<T>(string strType)
        {
            T t = (T)Enum.Parse(typeof(T), strType);
            return t;
        }
    }

    public enum EngineType
    {
        OpenVINO,
        TensorRT,
        ONNX,
        OpenCV,
        NULL
    }

    public enum ModelType
    {
        YOLOv5Det,
        YOLOv5Seg,
        YOLOv5Cls,
        YOLOv6Det,
        YOLOv7Det,
        YOLOv8Det,
        YOLOv8Seg,
        YOLOv8Cls,
        YOLOv8Pose,
        YOLOv8Obb,
        YOLOv9Det,
        YOLOv9Seg,
        YOLOWorld,
        YOLOv10Det
    }

    public enum ImgSize
    {
        S640 = 8400,
        S1024 = 21504,
        S2048 = 86016,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum InputImgSize 
    { 
        IN640=640,
        IN1024=1024,
        IN2048=2048,
    }
}
