class EmuClient:
    def __init__(self, client) -> None:
        self.client = client
        self.path = ""

    def setPath(self, path):
        self.path = path
    
    def requestScreenshot(self, path = None):
        if path:
            self.path = path
        
        self.client._queue_request("SCREENSHOT;" + self.path + "\n")

    def LoadState(self, name):
        self.client._queue_request("LOADSTATE;" + name +"\n")