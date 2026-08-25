using WH.DetectSystem.DetectSystem.MainModel;
using ZipperInfo;

namespace WH.DetectSystem.Models
{
    public partial class CProcessGroupModel
    {
        internal void StartZipperIdThread()
        {
            if (!COpenProjectLine.IsZipper)
            {
                return;
            }
            StopZipperIdThread();
            //20260506 鲍赞宝
            if (Name == "制程组1")
            {
                IDCreate = new CCreateIDMetalStation1();
                IDCreate.IntThread();
                IDCreate.IDSendEvent += IDSend;
            }
            //else if (Name== "制程组3")
            //{
            //    IDCreate = new CCreateIDStationUpMass();
            //}
            //else
            //{
            //    IDCreate = new CCreateIDStation3();
            //}
        }

        internal void StopZipperIdThread()
        {
            if (IDCreate == null)
            {
                return;
            }
            IDCreate.StopThread();
            IDCreate.IDSendEvent -= IDSend;
            IDCreate = null;
        }
    }
}
