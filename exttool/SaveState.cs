using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BizHawk.Client.Common;

namespace BirdsEye {
    public class SaveState{


        private readonly Logging _log;
        public SaveState(Logging log){
            _log = log;
        }

        public void GetScreenshot(ApiContainer APIs, String path){
            APIs.EmuClient.Screenshot(path);
        }

        public bool LoadSlot(ApiContainer APIs, String slot){
            int saveSlot = Int32.Parse(slot); 
            return APIs.SaveState.LoadSlot(saveSlot);
        }
    }
}