using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BizHawk.Client.Common;

namespace BirdsEye {
    public class EmuClient{


        private readonly Logging _log;
        public EmuClient(Logging log){
            _log = log;
        }

        public void GetScreenshot(ApiContainer APIs, String path){
            APIs.EmuClient.Screenshot(path);
        }
    }
}