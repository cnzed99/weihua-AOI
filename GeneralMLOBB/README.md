<div><center><b>
    <font color="34,63,93" size="7"> 
        YOLOV8检测火腿算法插件
    </font>
</b></center></div>

## 1. 项目介绍
使用OBB模型检测火腿缺陷

## 2. 项目配置

ONNX引擎 需要将Cudnn库复制到程序目录下

Nuget包：Microsoft.ML.OnnxRuntime.Gpu.Windows 1.19.2

Cuda 12.2

Cudnn：cudnn_9.4.0_windows

![image.png](https://s2.loli.net/2024/11/04/bZMdCiHOL7YNnaI.png)

运行环境拷贝至相应目录下：

![image.png](https://s2.loli.net/2024/11/04/LENudmRcKGC56Uq.png)

TensorRT 需要安装：

1、cudnn-windows-x86_64-8.9.7.29_cuda12-archive 

2、cuda12.2

3、TensorRT-8.6.1.6.Windows10.x86_64.cuda-12.0 

三个工具包