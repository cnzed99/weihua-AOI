using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using AlgorithmDll;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using OpenCvSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
using WH.RunCell;
using Point = System.Windows.Point;

namespace OcrAlgorithm
{
    public class CAlgorithmParam : CAlgorithmParamBase
    {
        public CAlgorithmParam()
            : base()
        {
            DefectSpecies = new() { new("异常类", new() { new("文本异常", Category.值) }), };
            DefectFeatures = new();
        }

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CParam(name, token));
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 更新毛刺参数结构体
        /// </summary>
        public override void UpdataAlgorParamUse() { }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 获取清晰度计算函数
        /// </summary>
        /// <returns>清晰度计算函数</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Func<CImage, float> GetDistinctFunc()
        {
            return null;
        }

        OCRPredictor ocr = null;

        /// <summary>
        /// 测试算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {
            var paramClass = (CParam)AlgorParams.FirstOrDefault(o => o.Name == ParamSelect);
            bool isOk = true;
            try
            {
                if (ocr == null)
                {
                    string det_model = "model/paddle/ch_PP-OCRv4_det_infer/inference.pdmodel";
                    string cls_model =
                        "model/paddle/ch_ppocr_mobile_v2.0_cls_infer/inference.pdmodel";
                    string rec_model = "model/paddle/ch_PP-OCRv4_rec_infer/inference.pdmodel";
                    if (!File.Exists(det_model))
                    {
                        Growl.Error("model/paddle/ch_PP-OCRv4_det_infer/inference.pdmodel文件缺失！");
                        isOk = false;
                        return;
                    }
                    if (!File.Exists(cls_model))
                    {
                        Growl.Error(
                            "model/paddle/ch_ppocr_mobile_v2.0_cls_infer/inference.pdmodel文件缺失！"
                        );
                        isOk = false;
                        return;
                    }
                    if (!File.Exists(rec_model))
                    {
                        Growl.Error("model/paddle/ch_PP-OCRv4_rec_infer/inference.pdmodel文件缺失！");
                        isOk = false;
                        return;
                    }
                    ocr = new OCRPredictor(det_model, cls_model, rec_model);
                }
                Mat image = new Mat(
                    cell.Image.ImageHeight,
                    cell.Image.ImageWidth,
                    MatType.CV_8UC((cell.Image.PixelFormat.BitsPerPixel + 7) / 8),
                    cell.Image.ImageData
                );
                if (
                    cell.Image.PixelFormat == PixelFormats.Bgra32
                    || cell.Image.PixelFormat == PixelFormats.Bgr32
                )
                {
                    image = image.CvtColor(ColorConversionCodes.BGRA2RGB);
                }
                var ocr_result = ocr.ocr(image, true, true, true);
                isOk = false;
                foreach (var res in ocr_result)
                {
                    if (res.cls_score > paramClass.Score)
                    {
                        isOk = true;
                        List<Point> points = new List<Point>();
                        points.Add(new Point((int)(res.box[0][0]), (int)(res.box[0][1])));
                        points.Add(new Point((int)(res.box[2][0]), (int)(res.box[2][1])));
                        points.Add(new Point((int)(res.box[3][0]), (int)(res.box[3][1])));
                        points.Add(new Point((int)(res.box[1][0]), (int)(res.box[1][1])));
                        cell.DrawEdges.Add(new CEdgeDraw(points, Brushes.Green, true));
                        cell.DrawEdges.Add(new CEdgeDraw(res.text, points[0], Brushes.Red, 30));
                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
                isOk = false;
            }
            finally
            {
                CellDetection cellDetection = new CellDetection();
                cellDetection.Type = "异常类";
                cellDetection.RecipeDefectName = "文本异常";
                cellDetection.Category = Category.值;
                cellDetection.Value = !isOk ? new() { 1.0f } : null;
                cell.AlgorithmOut.Add(cellDetection);
            }
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public partial class CParam : CParamBase
    {
        public CParam()
            : base() { }

        public CParam(string name, Token token)
            : base(name, token) { }

        /// <summary>
        /// 2025.3.23 李焕彬
        /// 最小分数阈值
        /// </summary>
        [ObservableProperty]
        [property: Category("基础参数")]
        [property: DisplayName("最小分数阈值")]
        [property: Description("最小分数阈值")]
        private float score = 0.8f;
    };
}
