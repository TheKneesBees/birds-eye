using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BizHawk.Client.Common;

namespace BirdsEye {
    public class Memory {
        private readonly List<long> _addressList = new();
        private readonly List<int> _memoryList = new();
        private readonly Logging _log;

        public Memory(Logging log) {
            _log = log;
        }

        ///<summary>
        /// Add a memory address to `_addressList`.<br/>
        /// An integer `-1` is added to `_memoryList`.<br/>
        /// Precondition: `address` represents a valid hexadecimal value.
        ///</summary>
        ///
        private void AddAddress(long address) {
            _log.Write(0, $"Adding address {address} to address list.");
            _addressList.Add(address);
            _memoryList.Add(-1);
        }

        ///<summary>
        /// Add memory addresses from a string.
        ///</summary>
        public void AddAddressesFromString(string str) {
            string[] addressList = str.Trim(';').Split(';');
            foreach (string addr in addressList) {
                AddAddress(Convert.ToInt64(addr, 10));
            }
        }

        ///<summary>
        /// Read each address in `_addressList` in main memory.<br/>
        /// Store the gathered values in `_memoryList`.<br/>
        /// Needs the API interface in order to read memory from the emulator.
        ///</summary>
        public int[] ReadMemory(ApiContainer APIs) {
            for (int i = 0; i < _addressList.Count; i++) {
                _memoryList[i] = (int) APIs.Memory.ReadByte(_addressList[i]);
            }
            return _memoryList.ToArray();
        }

        ///<summary>
        /// Concatenate all addresses and memory data into one string,
        /// formatted as such:<br/>
        /// "ADDR:DATA;ADDR:DATA;..." where both ADDR and DATA are in
        /// decimal form.<br/>
        /// `-1` is added if data has not been collected from an address.
        ///</summary>
        public string FormatMemory() {
            string result = "";
            for (int i = 0; i < _memoryList.Count; i++) {
                result += _addressList[i] + ":";
                if (i == -1) {
                    result += "-1;";
                } else {
                    result += _memoryList[i] + ";";
                }
            }
            return (result != "") ? result : ";";
        }

        ///<summary>
        /// Remove any addresses that have been set and empty both
        /// `_addressList` and `_memoryList`.
        ///</summary>
        public void ClearAddresses() {
            _log.Write(0, "Clearing address and memory lists.");
            _addressList.Clear();
            _memoryList.Clear();
        }

        /// <summary>
        /// Returns the list of domains that memory is capable of reading
        /// </summary>
        /// <param name="APIs"></param>
        /// <returns></returns>
         public String ReadDomainList(ApiContainer APIs){
            List<String> DomainList = (List<String>)APIs.Memory.GetMemoryDomainList();
            
            String Domains = String.Join(", ", DomainList.ToArray());
            return Domains;
        }

        /// <summary>
        /// Changes the domain that memory is reading from to MemDomain
        /// </summary>
        /// <param name="MemDomain"></param>
        /// <param name="APIs"></param>
        /// <returns></returns>
        public bool UseMemoryDomain(String MemDomain, ApiContainer APIs){
            bool DomainUse = APIs.Memory.UseMemoryDomain(MemDomain);
            return DomainUse;
        }

        /// <summary>
        /// Returns the domain that memory is reading from
        /// </summary>
        /// <param name="APIs"></param>
        /// <returns></returns>
        public String GetCurrentMemoryDomain(ApiContainer APIs){
            String Domain = APIs.Memory.GetCurrentMemoryDomain();
            return Domain;
        }
    }
}