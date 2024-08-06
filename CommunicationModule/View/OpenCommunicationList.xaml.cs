namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// OpenCommunicationList.xaml 的交互逻辑
    /// </summary>
    public partial class OpenCommunicationList : HandyControl.Controls.Window
    {
        public OpenCommunicationList()
        {
            InitializeComponent();
            this.DataContext = communicationVM;
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// VM
        /// </summary>
        public COpenCommunicationListVM communicationVM = new COpenCommunicationListVM();
    }
}
