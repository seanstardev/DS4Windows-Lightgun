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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DS4Windows;
using DS4WinWPF.DS4Control;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class CurrentOutDeviceViewModel
    {
        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SelectedIndexChanged;

        public Visibility SidePanelVisibility
        {
            get
            {
                Visibility result = Visibility.Collapsed;
                if (selectedIndex >= 0)
                {
                    SlotDeviceEntry temp = slotDeviceEntries[selectedIndex];
                    if (temp.OutSlotDevice.CurrentType != OutContType.None)
                    {
                        result = Visibility.Visible;
                    }
                }

                return result;
            }
        }
        public event EventHandler SidePanelVisibilityChanged;

        public bool PluginEnabled
        {
            get
            {
                bool result = false;
                if (selectedIndex >= 0)
                {
                    SlotDeviceEntry temp = slotDeviceEntries[selectedIndex];
                    if (temp.OutSlotDevice.CurrentAttachedStatus ==
                        OutSlotDevice.AttachedStatus.UnAttached)
                    {
                        result = true;
                    }
                }

                return result;
            }
        }
        public event EventHandler PluginEnabledChanged;

        public bool UnpluginEnabled
        {
            get
            {
                bool result = false;
                if (selectedIndex >= 0)
                {
                    SlotDeviceEntry temp = slotDeviceEntries[selectedIndex];
                    if (temp.OutSlotDevice.CurrentAttachedStatus ==
                        OutSlotDevice.AttachedStatus.Attached)
                    {
                        result = true;
                    }
                }

                return result;
            }
        }
        public event EventHandler UnpluginEnabledChanged;

        private DS4Windows.OutputSlotManager outSlotManager;
        private List<SlotDeviceEntry> slotDeviceEntries;

        public List<SlotDeviceEntry> SlotDeviceEntries { get => slotDeviceEntries; }

        private ControlService controlService;

        public CurrentOutDeviceViewModel(ControlService controlService,
            OutputSlotManager outputMan)
        {
            outSlotManager = outputMan;
            // Set initial capacity at input controller limit in app
            slotDeviceEntries = new List<SlotDeviceEntry>(ControlService.CURRENT_DS4_CONTROLLER_LIMIT);
            int idx = 0;
            foreach(OutSlotDevice tempDev in outputMan.OutputSlots)
            {
                SlotDeviceEntry tempEntry = new SlotDeviceEntry(tempDev, idx);
                tempEntry.PluginRequest += OutSlot_PluginRequest;
                tempEntry.UnplugRequest += OutSlot_UnplugRequest;
                slotDeviceEntries.Add(tempEntry);
                idx++;
            }

            this.controlService = controlService;

            outSlotManager.SlotAssigned += OutSlotManager_SlotAssigned;
            outSlotManager.SlotUnassigned += OutSlotManager_SlotUnassigned;
            SelectedIndexChanged += CurrentOutDeviceViewModel_SelectedIndexChanged;
        }

        private void CurrentOutDeviceViewModel_SelectedIndexChanged(object sender,
            EventArgs e)
        {
            SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            PluginEnabledChanged?.Invoke(this, EventArgs.Empty);
            UnpluginEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutSlot_PluginRequest(object sender, EventArgs e)
        {
            SlotDeviceEntry entry = sender as SlotDeviceEntry;
            if (entry.OutSlotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached &&
                entry.OutSlotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
            {
                controlService.EventDispatcher.BeginInvoke((Action)(() =>
                {
                    controlService.AttachUnboundOutDev(entry.OutSlotDevice, entry.OutSlotDevice.CurrentType);
                    //SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                }));
            }
        }

        private void OutSlot_UnplugRequest(object sender, EventArgs e)
        {
            SlotDeviceEntry entry = sender as SlotDeviceEntry;
            if (entry.OutSlotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached &&
                entry.OutSlotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
            {
                controlService.EventDispatcher.BeginInvoke((Action)(() =>
                {
                    controlService.DetachUnboundOutDev(entry.OutSlotDevice);
                    //SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
                }));
            }
        }

        private void OutSlotManager_SlotUnassigned(OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            slotDeviceEntries[slotNum].RemovedDevice();
            RefreshPanels();
        }

        private void OutSlotManager_SlotAssigned(OutputSlotManager sender,
            int slotNum, OutSlotDevice _)
        {
            slotDeviceEntries[slotNum].AssignedDevice();
            RefreshPanels();
        }

        private void RefreshPanels()
        {
            SidePanelVisibilityChanged?.Invoke(this, EventArgs.Empty);
            PluginEnabledChanged?.Invoke(this, EventArgs.Empty);
            UnpluginEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SlotDeviceEntry
    {
        private OutSlotDevice outSlotDevice;
        public OutSlotDevice OutSlotDevice { get => outSlotDevice; }

        public string CurrentType
        {
            get
            {
                string temp = "Empty";
                if (outSlotDevice.OutputDevice != null)
                {
                    temp = outSlotDevice.OutputDevice.GetDeviceType();
                }

                return temp;
            }
        }
        public event EventHandler CurrentTypeChanged;

        public string DesiredType
        {
            get
            {
                string temp = "Dynamic";
                if (outSlotDevice.CurrentReserveStatus ==
                    OutSlotDevice.ReserveStatus.Permanent)
                {
                    temp = outSlotDevice.PermanentType.ToString();
                }

                return temp;
            }
        }
        public event EventHandler DesiredTypeChanged;

        public bool BoundInput
        {
            get => outSlotDevice.CurrentInputBound == OutSlotDevice.InputBound.Bound;
        }
        public event EventHandler BoundInputChanged;

        private int desiredTypeChoiceIndex = -1;
        public int DesiredTypeChoice
        {
            get => desiredTypeChoiceIndex;
            set
            {
                if (desiredTypeChoiceIndex == value) return;
                desiredTypeChoiceIndex = value;
                DesiredTypeChoiceChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DesiredTypeChoiceChanged;

        private int reserveChoiceIndex = -1;
        public int ReserveChoice
        {
            get => reserveChoiceIndex;
            set
            {
                if (reserveChoiceIndex == value) return;
                reserveChoiceIndex = value;
                ReserveChoiceChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ReserveChoiceChanged;

        private bool dirty = false;
        public bool Dirty
        {
            get => dirty;
            set
            {
                if (dirty == value) return;
                dirty = value;
                DirtyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DirtyChanged;

        public event EventHandler PluginRequest;
        public event EventHandler UnplugRequest;
        int idx;

        public bool DisplayXInputSlotNum
        {
            get
            {
                bool result = false;

                result = outSlotDevice.CurrentType == OutContType.X360 &&
                    (outSlotDevice.OutputDevice as Xbox360OutDevice).Features.HasFlag(Xbox360OutDevice.X360Features.XInputSlotNum);

                return result;
            }
        }


        public string XInputSlotNum
        {
            get
            {
                var xinputSlot = "?";
                if (outSlotDevice.CurrentType == OutContType.X360)
                {
                    var tempX360 = outSlotDevice.OutputDevice as Xbox360OutDevice;
                    if (tempX360.XinputSlotNum >= 0) xinputSlot = $"{tempX360.XinputSlotNum + 1}";
                }
                return xinputSlot;
            }

        }

        public event EventHandler DisplayXInputSlotNumChanged;
        public event EventHandler XInputSlotNumChanged;

        public string InputSlotNum
        {
            get
            {
                string result = "";
                if (outSlotDevice.InputIndex != OutSlotDevice.INPUT_INDEX_DEFAULT)
                {
                    result = $"{outSlotDevice.InputIndex+1}";
                }

                return result;
            }
        }

        public event EventHandler InputSlotNumChanged;

        public string InputSlotDisplayString
        {
            get
            {
                string result = "";
                if (outSlotDevice.InputIndex != OutSlotDevice.INPUT_INDEX_DEFAULT)
                {
                    result = $"{outSlotDevice.InputIndex + 1} ({outSlotDevice.InputDisplayString})";
                }
                return result;
            }
        }
        public event EventHandler InputSlotDisplayStringChanged;

        public SlotDeviceEntry(OutSlotDevice outSlotDevice, int idx)
        {
            this.outSlotDevice = outSlotDevice;
            this.idx = idx;

            //desiredTypeChoiceIndex = DetermineDesiredChoiceIdx();
            reserveChoiceIndex = DetermineReserveChoiceIdx();

            SetupEvents();
        }

        private void SetupEvents()
        {
            //DesiredTypeChoiceChanged += SlotDeviceEntry_FormPropChanged;
            ReserveChoiceChanged += SlotDeviceEntry_FormPropChanged;

            outSlotDevice.PermanentTypeChanged += OutSlotDevice_PermanentTypeChanged;
            outSlotDevice.CurrentInputBoundChanged += OutSlotDevice_CurrentInputBoundChanged;
        }

        private void OutSlotDevice_CurrentInputBoundChanged(object sender, EventArgs e)
        {
            BoundInputChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutSlotDevice_PermanentTypeChanged(object sender, EventArgs e)
        {
            DesiredTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SlotDeviceEntry_FormPropChanged(object sender, EventArgs e)
        {
            Dirty = true;
        }

        private int DetermineDesiredChoiceIdx()
        {
            int result = 0;
            switch (outSlotDevice.PermanentType)
            {
                case OutContType.None:
                    result = 0;
                    break;
                case OutContType.X360:
                    result = 1;
                    break;
                case OutContType.DS4:
                    result = 2;
                    break;
                default:
                    break;
            }
            return result;
        }

        private int DetermineReserveChoiceIdx()
        {
            int result = 0;
            switch (outSlotDevice.CurrentReserveStatus)
            {
                case OutSlotDevice.ReserveStatus.Dynamic:
                    result = 0;
                    break;
                case OutSlotDevice.ReserveStatus.Permanent:
                    result = 1;
                    break;
                default:
                    break;
            }

            return result;
        }

        private OutContType DetermineDesiredTypeFromIdx()
        {
            OutContType result = OutContType.None;
            switch (desiredTypeChoiceIndex)
            {
                case 0:
                    result = OutContType.None;
                    break;
                case 1:
                    result = OutContType.X360;
                    break;
                case 2:
                    result = OutContType.DS4;
                    break;
                default:
                    break;
            }

            return result;
        }

        private OutSlotDevice.ReserveStatus DetermineReserveChoiceFromIdx()
        {
            OutSlotDevice.ReserveStatus result = OutSlotDevice.ReserveStatus.Dynamic;
            switch(reserveChoiceIndex)
            {
                case 0:
                    result = OutSlotDevice.ReserveStatus.Dynamic;
                    break;
                case 1:
                    result = OutSlotDevice.ReserveStatus.Permanent;
                    break;
                default:
                    break;
            }

            return result;
        }

        public void AssignedDevice()
        {
            Refresh();
        }

        public void RemovedDevice()
        {
            Refresh();
        }

        private void Refresh()
        {
            //DesiredTypeChoice = DetermineDesiredChoiceIdx();
            ReserveChoice = DetermineReserveChoiceIdx();

            CurrentTypeChanged?.Invoke(this, EventArgs.Empty);
            DesiredTypeChanged?.Invoke(this, EventArgs.Empty);
            BoundInputChanged?.Invoke(this, EventArgs.Empty);
            XInputSlotNumChanged?.Invoke(this, EventArgs.Empty);
            DisplayXInputSlotNumChanged?.Invoke(this, EventArgs.Empty);
            InputSlotDisplayStringChanged?.Invoke(this, EventArgs.Empty);
            Dirty = false;
        }

        public void RequestPlugin()
        {
            PluginRequest?.Invoke(this, EventArgs.Empty);
        }

        public void RequestUnplug()
        {
            UnplugRequest?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyChanges()
        {
            outSlotDevice.CurrentReserveStatus = DetermineReserveChoiceFromIdx();
            /*if (outSlotDevice.CurrentReserveStatus ==
                OutSlotDevice.ReserveStatus.Permanent)
            {
                outSlotDevice.PermanentType = outSlotDevice.CurrentType;
            }
            else
            {
                outSlotDevice.PermanentType = OutContType.None;
            }
            */
        }
    }
}
