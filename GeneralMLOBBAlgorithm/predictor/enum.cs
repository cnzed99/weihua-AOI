using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralMLOBBAlgorithm
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
        YOLOv8Det,
        YOLOv8Obb,
    }

    public enum ImgSize
    {
        S320 = 2100,
        S640 = 8400,
        S1024 = 21504,
        S2048 = 86016,
    }
}