class Joypad:
    def __init__(self, client) -> None:
        self.client = client
        self.slot = 1

    def setSlot(self, slot):
        self.slot = slot
    
    def pressKey(self, key):
        self.client._queue_request("PRESSKEY;" + key + "\n")
