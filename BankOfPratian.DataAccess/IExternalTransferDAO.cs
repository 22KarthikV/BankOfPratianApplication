using BankOfPratian.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankOfPratian.DataAccess
{
    public interface IExternalTransferDAO
    {
        void CreateExternalTransfer(ExternalTransfer transfer);
        ExternalTransfer GetExternalTransfer(int transID);
        List<ExternalTransfer> GetOpenExternalTransfers();
        void UpdateExternalTransfer(ExternalTransfer transfer);

        List<ExternalTransfer> GetAllExternalTransfers();
    }
}
