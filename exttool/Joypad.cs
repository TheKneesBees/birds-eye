using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BizHawk.Client.Common;

namespace BirdsEye {
    public class Joypad{


        private readonly Logging _log;
        public Joypad(Logging log){
            _log = log;
        }


        public void pressKey(ApiContainer APIs, String key){
             IReadOnlyDictionary<string, bool> keys = new Dictionary<string, bool>(){
                {key, true}
             };
             
            APIs.Joypad.Set(keys, 2);
        }
    }
}