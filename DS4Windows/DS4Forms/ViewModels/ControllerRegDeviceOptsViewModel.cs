﻿/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;
using LEDBarMode = DS4Windows.DualSenseControllerOptions.LEDBarMode;
using MuteLEDMode = DS4Windows.DualSenseControllerOptions.MuteLEDMode;
using LinkMode = DS4Windows.JoyConDeviceOptions.LinkMode;
using JoinedGyroProvider = DS4Windows.JoyConDeviceOptions.JoinedGyroProvider;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ControllerRegDeviceOptsViewModel
    {
        private ControlServiceDeviceOptions serviceDeviceOpts;

        public bool EnableDS4 { get => serviceDeviceOpts.DS4DeviceOpts.Enabled; }

        public bool EnableDualSense { get => serviceDeviceOpts.DualSenseOpts.Enabled; }

        public bool EnableSwitchPro { get => serviceDeviceOpts.SwitchProDeviceOpts.Enabled; }

        public bool EnableJoyCon { get => serviceDeviceOpts.JoyConDeviceOpts.Enabled; }

        public bool EnableDS3 { get => serviceDeviceOpts.DS3DeviceOpts.Enabled; }

        public DS4DeviceOptions DS4DeviceOpts { get => serviceDeviceOpts.DS4DeviceOpts; }
        public DS3DeviceOptions DS3DeviceOpts { get => serviceDeviceOpts.DS3DeviceOpts; }
        public DualSenseDeviceOptions DSDeviceOpts { get => serviceDeviceOpts.DualSenseOpts; }
        public SwitchProDeviceOptions SwitchProDeviceOpts { get => serviceDeviceOpts.SwitchProDeviceOpts; }
        public JoyConDeviceOptions JoyConDeviceOpts { get => serviceDeviceOpts.JoyConDeviceOpts; }

        public bool VerboseLogMessages { get => serviceDeviceOpts.VerboseLogMessages; set => serviceDeviceOpts.VerboseLogMessages = value; }

        private List<DeviceListItem> currentInputDevices = new List<DeviceListItem>();
        public List<DeviceListItem> CurrentInputDevices { get => currentInputDevices; }

        // Serial, ControllerOptionsStore instance
        private Dictionary<string, ControllerOptionsStore> inputDeviceSettings = new Dictionary<string, ControllerOptionsStore>();
        private List<ControllerOptionsStore> controllerOptionsStores = new List<ControllerOptionsStore>();

        private int controllerSelectedIndex = -1;
        public int ControllerSelectedIndex
        {
            get => controllerSelectedIndex;
            set
            {
                if (controllerSelectedIndex == value) return;
                controllerSelectedIndex = value;
                ControllerSelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ControllerSelectedIndexChanged;

        public DS4ControllerOptions CurrentDS4Options
        {
            get => controllerOptionsStores[controllerSelectedIndex] as DS4ControllerOptions;
        }

        public DualSenseControllerOptions CurrentDSOptions
        {
            get => controllerOptionsStores[controllerSelectedIndex] as DualSenseControllerOptions;
        }

        public SwitchProControllerOptions CurrentSwitchProOptions
        {
            get => controllerOptionsStores[controllerSelectedIndex] as SwitchProControllerOptions;
        }

        public JoyConControllerOptions CurrentJoyConOptions
        {
            get => controllerOptionsStores[controllerSelectedIndex] as JoyConControllerOptions;
        }

        private int currentTabSelectedIndex = 0;
        public int CurrentTabSelectedIndex
        {
            get => currentTabSelectedIndex;
            set
            {
                if (currentTabSelectedIndex == value) return;
                currentTabSelectedIndex = value;
                CurrentTabSelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CurrentTabSelectedIndexChanged;

        public ControllerRegDeviceOptsViewModel(ControlServiceDeviceOptions serviceDeviceOpts,
            ControlService service)
        {
            this.serviceDeviceOpts = serviceDeviceOpts;

            int idx = 0;
            foreach(DS4Device device in service.DS4Controllers)
            {
                if (device != null)
                {
                    currentInputDevices.Add(new DeviceListItem(device));
                    inputDeviceSettings.Add(device.MacAddress, device.optionsStore);
                    controllerOptionsStores.Add(device.optionsStore);
                }
                idx++;
            }
        }

        private object dataContextObject = null;
        public object DataContextObject { get => dataContextObject; }

        public int FindTabOptionsIndex()
        {
            ControllerOptionsStore currentStore =
                controllerOptionsStores[controllerSelectedIndex];

            int result = 0;
            switch (currentStore.DeviceType)
            {
                case DS4Windows.InputDevices.InputDeviceType.DS3:
                    result = 0;
                    break;
                case DS4Windows.InputDevices.InputDeviceType.DS4:
                    result = 1;
                    break;
                case DS4Windows.InputDevices.InputDeviceType.DualSense:
                    result = 2;
                    break;
                case DS4Windows.InputDevices.InputDeviceType.SwitchPro:
                    result = 3;
                    break;
                case DS4Windows.InputDevices.InputDeviceType.JoyConL:
                case DS4Windows.InputDevices.InputDeviceType.JoyConR:
                    result = 4;
                    break;
                default:
                    // Default to empty control
                    result = 0;
                    break;
            }

            return result;
        }

        public void FindFittingDataContext()
        {
            ControllerOptionsStore currentStore =
                controllerOptionsStores[controllerSelectedIndex];

            switch (currentStore.DeviceType)
            {
                case DS4Windows.InputDevices.InputDeviceType.DS3:
                    // Does not have device specific options
                    break;
                case DS4Windows.InputDevices.InputDeviceType.DS4:
                    dataContextObject = new DS4ControllerOptionsWrapper(CurrentDS4Options, serviceDeviceOpts.DS4DeviceOpts);
                    break;
                case DS4Windows.InputDevices.InputDeviceType.DualSense:
                    dataContextObject = new DualSenseControllerOptionsWrapper(CurrentDSOptions, serviceDeviceOpts.DualSenseOpts);
                    break;
                case DS4Windows.InputDevices.InputDeviceType.SwitchPro:
                    dataContextObject = new SwitchProControllerOptionsWrapper(CurrentSwitchProOptions, serviceDeviceOpts.SwitchProDeviceOpts);
                    break;
                case DS4Windows.InputDevices.InputDeviceType.JoyConL:
                case DS4Windows.InputDevices.InputDeviceType.JoyConR:
                    dataContextObject = new JoyConControllerOptionsWrapper(CurrentJoyConOptions, serviceDeviceOpts.JoyConDeviceOpts);
                    break;
                default:
                    break;
            }
        }

        public void SaveControllerConfigs()
        {
            foreach (DeviceListItem item in currentInputDevices)
            {
                Global.SaveControllerConfigs(item.Device);
            }
        }
    }

    public class DeviceListItem
    {
        private DS4Device device;
        public DS4Device Device { get => device; }

        public string IdText
        {
            get => $"{device.DisplayName} ({device.MacAddress})";
        }

        public DeviceListItem(DS4Device device)
        {
            this.device = device;
        }
    }


    public class DS4ControllerOptionsWrapper
    {
        private DS4ControllerOptions options;
        public DS4ControllerOptions Options { get => options; }

        private DS4DeviceOptions parentOptions;
        public bool Visible
        {
            get => parentOptions.Enabled;
        }
        public event EventHandler VisibleChanged;

        public DS4ControllerOptionsWrapper(DS4ControllerOptions options, DS4DeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += (sender, e) => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }
    }

    public class DualSenseControllerOptionsWrapper
    {
        private DualSenseControllerOptions options;
        public DualSenseControllerOptions Options { get => options; }

        private DualSenseDeviceOptions parentOptions;
        public bool Visible { get => parentOptions.Enabled; }
        public event EventHandler VisibleChanged;

        private List<EnumChoiceSelection<LEDBarMode>> dsLEDModeOptions = new List<EnumChoiceSelection<LEDBarMode>>()
        {
            new EnumChoiceSelection<LEDBarMode>("Off", LEDBarMode.Off),
            new EnumChoiceSelection<LEDBarMode>("Only for multiple controllers", LEDBarMode.MultipleControllers),
            new EnumChoiceSelection<LEDBarMode>("Battery Percentage", LEDBarMode.BatteryPercentage),
            new EnumChoiceSelection<LEDBarMode>("On", LEDBarMode.On),
        };
        public List<EnumChoiceSelection<LEDBarMode>> DsLEDModes { get => dsLEDModeOptions; }

        private List<EnumChoiceSelection<MuteLEDMode>> dsMuteLEDModes = new List<EnumChoiceSelection<MuteLEDMode>>()
        {
            new EnumChoiceSelection<MuteLEDMode>("Off", MuteLEDMode.Off),
            new EnumChoiceSelection<MuteLEDMode>("On", MuteLEDMode.On),
            new EnumChoiceSelection<MuteLEDMode>("Pulse", MuteLEDMode.Pulse),
        };
        public List<EnumChoiceSelection<MuteLEDMode>> DsMuteLEDModes { get => dsMuteLEDModes; }

        public DualSenseControllerOptionsWrapper(DualSenseControllerOptions options,
            DualSenseDeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += (sender, e) => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }
    }

    public class SwitchProControllerOptionsWrapper
    {
        private SwitchProControllerOptions options;
        public SwitchProControllerOptions Options { get => options; }

        private SwitchProDeviceOptions parentOptions;
        public bool Visible { get => parentOptions.Enabled; }
        public event EventHandler VisibleChanged;

        public SwitchProControllerOptionsWrapper(SwitchProControllerOptions options,
            SwitchProDeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += (sender, e) => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }
    }

    public class JoyConControllerOptionsWrapper
    {
        private JoyConControllerOptions options;
        public JoyConControllerOptions Options { get => options; }

        private JoyConDeviceOptions parentOptions;
        public JoyConDeviceOptions ParentOptions { get => parentOptions; }

        public bool Visible { get => parentOptions.Enabled; }
        public event EventHandler VisibleChanged;

        private List<EnumChoiceSelection<LinkMode>> linkModes = new List<EnumChoiceSelection<LinkMode>>()
        {
            new EnumChoiceSelection<LinkMode>("Split", LinkMode.Split),
            new EnumChoiceSelection<LinkMode>("Joined", LinkMode.Joined),
        };
        public List<EnumChoiceSelection<LinkMode>> LinkModes { get => linkModes; }

        private List<EnumChoiceSelection<JoinedGyroProvider>> joinGyroOptions = new List<EnumChoiceSelection<JoinedGyroProvider>>()
        {
            new EnumChoiceSelection<JoinedGyroProvider>("Left", JoinedGyroProvider.JoyConL),
            new EnumChoiceSelection<JoinedGyroProvider>("Right", JoinedGyroProvider.JoyConR),
        };
        public List<EnumChoiceSelection<JoinedGyroProvider>> JoinGyroOptions { get => joinGyroOptions; }

        public JoyConControllerOptionsWrapper(JoyConControllerOptions options,
            JoyConDeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += (sender, e) => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }
    }

}
