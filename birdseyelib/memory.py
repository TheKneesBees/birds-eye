class Memory:
    """Class containing various functions for setting controller input in BizHawk."""
    def __init__(self, client) -> None:
        self.client = client
        self.address_lists = {"SA1 IRAM": [], "SA1 BWRAM": []}
        self.received_memory = {"SA1 IRAM": {}, "SA1 BWRAM": {}}
        self.domains = ["SA1 IRAM", "SA1 BWRAM"]

    def add_address(self, addr, domain):
        """Adds an address for the external tool to return.

        :param addr: A hexidecimal value representing the address to read from
        in the BizHawk emulator's memory.
        :type addr: int"""
        if domain:
            if not str(addr) in self.address_lists.get(domain, []):
                self.address_lists[domain].append(str(addr))
                self.received_memory[domain][hex(addr)] = -1
        else:
            if not str(addr) in self.address_list:
                self.address_list.append(str(addr))
                self.received_memory[hex(addr)] = -1

    def add_address_range(self, start, end, domain):
        """Adds a range of addresses from `start` to `end`, both inclusive.

        :param start: A hexidecimal value representing the first address in the range.
        :type start: int

        :param end: A hexidecimal value representing the last address in the range.
        :type end: int

        :precondition: `start` <= `end`."""
        for addr in range(int(start), int(end) + 1):
            self.add_address(addr, domain)

    def request_memory(self):
        """Requests for the latest memory data from the external tool."""
        self.client._queue_request("MEMORY;" + ";".join(self.address_list) + "\n")
        self.address_list = []

    def get_memory(self) -> dict:
        """Gets the latest memory data received from the external tool. 

        This will return a copy of the dictionary containing the latest data
        received from each requested address. Where the address
        (in hexadecimal representation) is the key, and the data is the value
        (in decimal representation).

        The value is set to `-1` if no data has been received for that address."""
        data = self.client._get_latest_response_data("MEMORY")
        if data:
            address_value_pairs = data.strip(";").split(";")
            for addr_val_pair in address_value_pairs:
                temp = addr_val_pair.split(":")
                addr, val = temp[0], temp[1]
                self.received_memory[hex(int(addr))] = int(val)
        return self.received_memory.copy()

    def request_domains(self):
        """Requests the domains the memory api can read from"""
        self.client._queue_request("MEMORY_DOMAINS;" + "\n")
    
    def get_memory_domains(self):
        """Gets the list of domains for memory"""
        data = self.client._get_latest_response_data("MEMORY_DOMAINS")
        if data:
            return data
        
    def request_domain_change(self, domain):
        """Requests that the memory api changes the domain to read from"""
        self.client._queue_request("CHANGE_DOMAIN;" + domain  + ";\n")
        
    def get_request_domain_change(self):
        """Gets the result of the domain change request
        
            This will return a boolean True of False depending on whether or not the
            API changed to a valid memory domain"""
        data = self.client._get_latest_response_data("CHANGE_DOMAIN")
        if data and "rue" in data:
            return True
        return False
    
    def get_current_memory_domain(self):
        """Returns the domain that the memory api is reading from"""
        self.client._queue_request("CURRENT_DOMAIN;\n")
        data = self.client._get_latest_response_data("CURRENT_DOMAIN")
        return data
    
    def clear_address_list(self):
        self.address_list = []

    def add_domain(self, domain):
        self.domains.append(domain)
        self.address_lists[domain] = []

    def request_IRAM_memory(self):
        """Requests for the latest memory data from the external tool."""
        self.request_domain_change("SA1 IRAM")
        self.client._queue_request("IRAM_MEMORY;" + ";".join(self.address_lists.get("SA1 IRAM", [])) + "\n")
        self.address_lists["SA1 IRAM"] = []

    def request_BWRAM_memory(self):
        """Requests for the latest memory data from the external tool."""
        self.request_domain_change("SA1 BWRAM")
        self.client._queue_request("BWRAM_MEMORY;" + ";".join(self.address_lists.get("SA1 BWRAM", [])) + "\n")
        self.address_lists["SA1 BWRAM"] = []

    def get_IRAM_memory(self) -> dict:
        """Gets the latest memory data received from the external tool. 

        This will return a copy of the dictionary containing the latest data
        received from each requested address. Where the address
        (in hexadecimal representation) is the key, and the data is the value
        (in decimal representation).

        The value is set to `-1` if no data has been received for that address."""
        data = self.client._get_latest_response_data("IRAM_MEMORY")
        if data:
            address_value_pairs = data.strip(";").split(";")
            for addr_val_pair in address_value_pairs:
                temp = addr_val_pair.split(":")
                addr, val = temp[0], temp[1]
                self.received_memory["SA1 IRAM"][hex(int(addr))] = int(val)
        return self.received_memory["SA1 IRAM"].copy()
    
    def get_BWRAM_memory(self): 
        data = self.client._get_latest_response_data("BWRAM_MEMORY")
        if data:
            address_value_pairs = data.strip(";").split(";")
            for addr_val_pair in address_value_pairs:
                temp = addr_val_pair.split(":")
                addr, val = temp[0], temp[1]
                self.received_memory["SA1 BWRAM"][hex(int(addr))] = int(val)
        return self.received_memory["SA1 BWRAM"].copy()