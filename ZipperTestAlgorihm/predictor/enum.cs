using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperTestAlgorihm
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
        YOLOv8Seg,
        YOLOv8Cls,
        YOLOv8Obb,
    }
    public enum InputImgSize
    {
        IN160=160,
        IN192=192,
        IN256=256,
        IN320 = 320,
        IN480=480,
        IN640 = 640,
        IN1024 = 1024,
        IN2048 = 2048,
    }
    public enum ImgSize
    {
        S160=525,
        S192=756,
        S256=1344,
        S320 = 2100,
        S480=4725,
        S640 = 8400,
        S1024 = 21504,
        S2048 = 86016,
    }

}