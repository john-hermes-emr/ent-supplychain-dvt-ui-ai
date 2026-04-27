using DVT.Api.Models;
using DVT.Core;

namespace DVT.Api
{
    public class AboutService:IAboutService
    {
        private StartUpLog _startupLog;        

        public AboutService(StartUpLog startUpLog)
        {
            _startupLog = startUpLog;               
        }

        public void SetStartupLog(StartUpLog startupLog)
        {
            _startupLog = startupLog;
        }   

        public AboutResponse GetAbout()
        {
            var aboutMessage = $"Env: {_startupLog.EnvironmentName} - ";
            aboutMessage += "DVT Application Core API. DVT (Data Validation Tool) is an application that loads and validates Direct Material Operational data from Procurement, Accounts Payable and MRP/ERP systems for use in Oracle Fusion Analytic Warehouse (FAW) instance.";

            //Append startup failure message if startup failed
            if (!_startupLog.StartupSuccess)
            {
                aboutMessage += " - WARNING: Application startup encountered errors. Please review the startup log for details.";
            }

            var returnObject = new AboutResponse()
            {
                AboutTime = DateTime.UtcNow.ToLongDateString() + " " + DateTime.UtcNow.ToLongTimeString(),
                AboutMessage = aboutMessage
            };
            return returnObject;
        }

        public StartupResponse GetStartupInfo()
        {
            var startupInfo = new StartupResponse()
            {
                StartupLog = _startupLog.LogEntries,
                StartupSuccess = _startupLog.StartupSuccess
            };
            return startupInfo;
        }    
        
    }

    public interface IAboutService
    {
        void SetStartupLog(StartUpLog startupLog);
        AboutResponse GetAbout();
        StartupResponse GetStartupInfo();      
    }
}
