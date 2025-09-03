using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.Models
{
    public enum DeviceProtocol
    {
        Unknown,
        Smb,
        Ftp,
        Adb,
        Http
    }

    public class Device : ObservableObject
    {
        string _ip = string.Empty;
        string _hostname = string.Empty;
        DeviceProtocol _protocol;
        List<DeviceProtocol> _protocols = new();
        string _protocolsDisplay = string.Empty;
        string _status = string.Empty;
        DateTime _lastSeen;
        string _username = string.Empty;
        string _password = string.Empty;
        string _domain = string.Empty;
        int? _groupId;
        int? _itemId;
        string _itemName = string.Empty;

        public string Ip
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        public string Hostname
        {
            get => _hostname;
            set => SetProperty(ref _hostname, value);
        }

        public DeviceProtocol Protocol
        {
            get => _protocol;
            set => SetProperty(ref _protocol, value);
        }

        public IList<DeviceProtocol> Protocols
        {
            get => _protocols;
            set => SetProperty(ref _protocols, (List<DeviceProtocol>)value);
        }

        public string ProtocolsDisplay
        {
            get => _protocolsDisplay;
            set => SetProperty(ref _protocolsDisplay, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public DateTime LastSeen
        {
            get => _lastSeen;
            set => SetProperty(ref _lastSeen, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string Domain
        {
            get => _domain;
            set => SetProperty(ref _domain, value);
        }

        public int? GroupId
        {
            get => _groupId;
            set => SetProperty(ref _groupId, value);
        }

        public int? ItemId
        {
            get => _itemId;
            set => SetProperty(ref _itemId, value);
        }

        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }
    }
}
