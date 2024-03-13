class SaveState:
    def __init__(self, client) -> None:
        self.client = client
        self.slot = 1

    def setSlot(self, slot):
        self.slot = slot
    
    def loadSlot(self, slot = None):
        if slot:
            self.slot = slot
        
        self.client._queue_request("LOADSLOT;" + str(self.slot) + "\n")
