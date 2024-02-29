using System;
using System.Collections.Generic;

using BizHawk.Client.Common;

namespace BirdsEye {
    public class ControllerInput {
        private readonly bool[] _inputState = {false, false, false, false, false, false, false, false, false, false};
        private readonly Logging _log;

        public ControllerInput(Logging log) {
            _log = log;
        }

        ///<summary>
        /// Execute the current input state in the emulator.
        ///</summary>

        public void ExecuteInput(ApiContainer APIs) {
            APIs.Joypad.Set(new Dictionary<string, bool>() {
                {"A", _inputState[0]},
                {"B", _inputState[1]},
                {"X", _inputState[2]},
                {"Y", _inputState[3]},
                {"Up", _inputState[4]},
                {"Down", _inputState[5]},
                {"Right", _inputState[6]},
                {"Left", _inputState[7]},
                {"Start", _inputState[8]},
                {"Select", _inputState[9]}
            }, 1);
        }

        ///<summary>
        /// Set the controller input from a string containing the state of different buttons
        /// to be executed when `ExecuteInput` is called.
        ///</summary>
        public void SetInputFromString(string str) {
            string[] newState = str.Trim(';').Split(';');
            for (int i = 0; i < 10; i++) {
                _inputState[i] = Convert.ToBoolean(newState[i]);
            }
        }
    }
}