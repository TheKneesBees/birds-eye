using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;

namespace BirdsEye {
    [ExternalTool("BirdsEye")]
    public class CustomMainForm : ToolFormBase, IExternalToolForm {
        ///<remarks>
		/// <see cref="ApiContainer"/> can be used as a shorthand for accessing 
        /// the various APIs, more like the Lua syntax.
		///</remarks>
		public ApiContainer? _apiContainer { get; set; }
		private ApiContainer APIs => _apiContainer ?? throw new NullReferenceException();

        private static Logging _log = new Logging(0);
        private SocketServer _server = new SocketServer(_log, "127.0.0.1", 8080);
        private Memory _memory = new Memory(_log);
        private ControllerInput _input = new ControllerInput(_log);
        private bool _commandeer = false;
        private Thread _commThread;

        private Label _lblRomName;
        private Label _lblCommMode;
        private Button _btnChangeCommMode;
        private Label _lblConnectionStatus;
        private Label _lblError;

        protected override string WindowTitleStatic => "BirdsEye";

        ///<summary>
        /// Main form constructor.<br/>
        /// Code is executed only once (when EmuHawk.exe is launched).
        ///</summary>
        public CustomMainForm() {
            _log.Write(1, "Initializing main form.");
            this.FormClosing += OnFormClosing;

            _commThread = new Thread(new ThreadStart(_server.AcceptConnections));
            _commThread.Start();
            
            ClientSize = new Size(480, 320);
            SuspendLayout();

            // Rom Label
            _lblRomName = new Label {
                AutoSize = true,
                Location = new Point(0, 0),
            };
            // Communication Mode Label
            _lblCommMode = new Label {
                AutoSize = true,
                Location = new Point(240, 0),
                Text = "Communication Mode: Manual"
            };
            // Communication Mode Button
            _btnChangeCommMode = new Button {
                Location = new Point(240, 20),
                Size = new Size(100, 25),
                Text = "Change Mode"
            };
            // Connection Status Label
            _lblConnectionStatus = new Label {
                AutoSize = true,
                Location = new Point(240, 50),
                Text = "No script found",
                ForeColor = Color.Red
            };
            // Error Label
            _lblError = new Label {
                AutoSize = true,
                Location = new Point(0, 300),
                ForeColor = Color.Red
            };

            Controls.Add(_lblRomName);
            Controls.Add(_lblCommMode);
            Controls.Add(_btnChangeCommMode);
            Controls.Add(_lblConnectionStatus);
            Controls.Add(_lblError);
            ResumeLayout();

            _btnChangeCommMode.Click += btnChangeCommModeOnClick;
        }

        ///<summary>
        /// Executed once after the constructor, and again every time a rom is
        /// loaded or reloaded.
        ///</summary>
        public override void Restart() {
            DisplayLoadedRom();
        }

        ///<summary>
        /// Executed before every frame.
        ///</summary>
        protected override void UpdateBefore() {
            UpdateConnectionStatus();
            if (_server.IsConnected()) {
                ProcessRequests();
            }
            if (_commandeer) {
                _input.ExecuteInput(APIs);
            }
        }

        ///<summary>
        /// Process requests received by `_server`.
        /// Disconnects python client if an error occurs when receiving data, or
        /// if the python client sends a request for the connection to be closed.
        ///</summary>
        private void ProcessRequests() {
            string[] msgs = _server.GetRequests();
            if (msgs[0] == "ERR") {
                _log.Write(2, "Disconnecting from python client due to bad request.");
                HandleDisconnect();
            }

            string response = "";
            foreach (string msg in msgs) {
                if (msg.Length > 6 && msg.Substring(0, 6).Equals("MEMORY")) {
                    if (msg != "MEMORY;") {
                        _memory.AddAddressesFromString(msg);
                    }
                    response += "MEMORY;" + _memory.FormatMemory() + "\n";
                } else if (msg.Length > 5 && msg.Substring(0, 5).Equals("INPUT")) {
                    _input.SetInputFromString(msg);
                    response += "INPUT;\n";
                } else if (msg.Length > 5 && msg.Substring(0, 5).Equals("CLOSE")) {
                    HandleDisconnect();
                }
            }

            _server.SendMessage(response);
        }

        ///<summary>
        /// Change the text of `_lblRomName` to display the current rom.
        ///</summary>
        private void DisplayLoadedRom() {
            if (APIs.GameInfo.GetRomName() != "Null") {
                _lblRomName.Text = $"Currently loaded: {APIs.GameInfo.GetRomName()}";
            } else {
                _lblRomName.Text = "Currently loaded: Nothing";
            }
        }

        ///<summary>
        /// Determine if all characters in `str` are valid hexadecimal digits.<br/>
        /// Returns false if an invalid digit is found, otherwise this returns true.
        ///</summary>
        private bool IsHexadecimal(string str) {
            string hexadecimalChars = "0987654321ABCDEFabcdef";
            foreach (char ch in str) {
                if (!hexadecimalChars.Contains(ch)) {
                    return false;
                }
            }
            return true;
        }

        ///<summary>
        /// Update the current status of the socket connection and update
        /// `_lblConnectionStatus` accordingly.
        ///</summary>
        private void UpdateConnectionStatus() {
            if (_server.IsConnected()) {
                _lblConnectionStatus.Text = "Script found";
                _lblConnectionStatus.ForeColor = Color.Blue;
                _lblError.Text = "";
                if (!_commThread.IsAlive) {
                    _commThread.Join();
                }
            } else {
                _lblConnectionStatus.Text = "No script found";
                _lblConnectionStatus.ForeColor = Color.Red;
            }
        }

        ///<summary>
        /// Executed when the socket connection abrupty ends, or when requested
        /// by the python client.<br/>
        /// Displays an error message in the external tool and cleans up socket resources.
        ///</summary>
        private void HandleDisconnect() {
            _server.CloseConnection();
            UpdateConnectionStatus();
            _lblError.Text = "Connection with script has been stopped.";
            _commandeer = false;
            _lblCommMode.Text = "Communication Mode: Manual";

            _memory.ClearAddresses();

            _commThread = new Thread(new ThreadStart(_server.AcceptConnections));
            _commThread.Start();
        }

        ///<summary>
        /// Change the communication mode from manual -> commandeer, or commandeer -> manual.<br/>
        /// Displays an error if the user attempts to switch to commandeer mode before a
        /// python client is connected.
        ///</summary>
        private void btnChangeCommModeOnClick(object sender, EventArgs e) {
            if (!_server.IsConnected() && !_commandeer) {
                _log.Write(2, "Cannot switch to commandeer when no script is connected.");
                _lblError.Text = "ERROR: No script is connected";
                return;
            } else if (!_commandeer) {
                _log.Write(1, "Communication mode set to commandeer.");
                _commandeer = true;
                _lblCommMode.Text = "Communication Mode: Commandeer";
            } else {
                _log.Write(1, "Communication mode set to manual.");
                _commandeer = false;
                _lblCommMode.Text = "Communication Mode: Manual";
            }
            _lblError.Text = "";
        }

        ///<summary>
        /// Close any resources and threads before closing the application.
        /// Attempts to abort out of any thread, regardless of state.
        ///</summary>
        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            _log.Write(1, "Gracefully closing external tool.");
            // TODO: Is this an ok approach to terminating the thread?
            try {
                _commThread.Abort();
            } catch {}

            _server.CloseAll();
        }
    }
}