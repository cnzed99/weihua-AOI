using OpenVinoSharp.Extensions.result;

namespace AlgorithmYoloBase
{
    ///Det 轴对齐框坐标还原。两个构造禁止合成（构造1 Value=0，构造2 Value=Score）
    public struct CoordRestoreData
    {
        ///盘齿/曲轴：X += orgx + imgWidth * imgIndex；Value = 0
        public CoordRestoreData(int imgWidth, int imgIndex, int orgx, int orgy,
            string labelstr, DetData det, int showinview = 0)
        {
            ShowLeftUp.X = det.box.Left + orgx + imgWidth * imgIndex;
            ShowLeftUp.Y = det.box.Top + orgy;
            ShowRightUp.X = det.box.Right + orgx + imgWidth * imgIndex;
            ShowRightUp.Y = det.box.Top + orgy;

            ShowRightDown.X = det.box.Right + orgx + imgWidth * imgIndex;
            ShowRightDown.Y = det.box.Bottom + orgy;

            ShowLeftDown.X = det.box.Left + orgx + imgWidth * imgIndex;
            ShowLeftDown.Y = det.box.Bottom + orgy;

            RecWidth = det.box.Width;
            RecHeight = det.box.Height;
            OrgCenterX = (float)(det.box.Left + det.box.Width / 2.0) + orgx;
            OrgCenterY = (float)(det.box.Top + det.box.Height / 2.0) + orgy;
            Score = det.score * 100;
            Labelstr = labelstr;
            Angle = 0.0f;
            Value = 0.0f;
            ShowInView = showinview;
        }

        /// 新兴：mosaic 公式；Infer 现状 mosaic2x2=false；Value = Score
        public CoordRestoreData(int imgWidth, int imgHeight, int imgIndex, int orgx, int orgy,
            string labelstr, DetData det, bool mosaic2x2, int showinview = 0)
        {
            int col;
            int row;
            if (mosaic2x2)
            {
                col = imgIndex % 2;
                row = imgIndex / 2;
            }
            else
            {
                col = imgIndex;
                row = 0;
            }

            double ox = orgx + imgWidth * col;
            double oy = orgy + imgHeight * row;

            ShowLeftUp.X = det.box.Left + ox;
            ShowLeftUp.Y = det.box.Top + oy;
            ShowRightUp.X = det.box.Right + ox;
            ShowRightUp.Y = det.box.Top + oy;

            ShowRightDown.X = det.box.Right + ox;
            ShowRightDown.Y = det.box.Bottom + oy;

            ShowLeftDown.X = det.box.Left + ox;
            ShowLeftDown.Y = det.box.Bottom + oy;

            RecWidth = det.box.Width;
            RecHeight = det.box.Height;
            OrgCenterX = (float)(det.box.Left + det.box.Width / 2.0 + ox);
            OrgCenterY = (float)(det.box.Top + det.box.Height / 2.0 + oy);
            Score = det.score * 100;
            Labelstr = labelstr;
            Angle = 0.0f;
            Value = Score;
            ShowInView = showinview;
        }

        public System.Windows.Point ShowLeftUp = new System.Windows.Point();
        public System.Windows.Point ShowRightUp = new System.Windows.Point();
        public System.Windows.Point ShowRightDown = new System.Windows.Point();
        public System.Windows.Point ShowLeftDown = new System.Windows.Point();
        public float OrgCenterX { get; set; }
        public float RecWidth { get; set; }
        public float RecHeight { get; set; }
        public float Score { get; set; }
        public string Labelstr { get; set; }
        public float Angle { get; set; }
        public float Value { get; set; }
        public int ShowInView { get; set; }
        public float OrgCenterY { get; set; }
    }
}
