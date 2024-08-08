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

namespace DS4WinWPF.DS4Control
{
    public class OutSlotDevice
    {
        public enum AttachedStatus : uint
        {
            UnAttached = 0,
            Attached = 1,
        }

        public enum ReserveStatus : uint
        {
            Dynamic = 0,
            Permanent = 1,
        }

        public enum InputBound : uint
        {
            Unbound = 0,
            Bound = 1,
        }

        public const int INPUT_INDEX_DEFAULT = -1;
        private AttachedStatus attachedStatus;
        private OutputDevice outputDevice;
        private ReserveStatus reserveStatus;
        private InputBound inputBound;
        private OutContType permanentType;
        private OutContType currentType;
        private int index;
        public int Index => index;

        private int inputIndex = INPUT_INDEX_DEFAULT;
        public int InputIndex
        {
            get => inputIndex;
            set => inputIndex = value;
        }

        private string inputDisplayString = string.Empty;
        public string InputDisplayString
        {
            get => inputDisplayString;
            set => inputDisplayString = value;
        }

        /// <summary>
        /// Connection status of virtual output controller
        /// </summary>
        public AttachedStatus CurrentAttachedStatus { get => attachedStatus; }

        /// <summary>
        /// Reference to output controller
        /// </summary>
        public OutputDevice OutputDevice { get => outputDevice; }

        /// <summary>
        /// Flag stating the connection preference of an output controller
        /// </summary>
        public ReserveStatus CurrentReserveStatus
        {
            get => reserveStatus;
            set
            {
                if (reserveStatus == value) return;
                reserveStatus = value;
                CurrentReserveStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CurrentReserveStatusChanged;

        /// <summary>
        /// Whether an input controller is associated with the slot
        /// </summary>
        public InputBound CurrentInputBound
        {
            get => inputBound;
            set
            {
                if (inputBound == value) return;
                inputBound = value;
                CurrentInputBoundChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CurrentInputBoundChanged;

        /// <summary>
        /// Desired device type for a permanently connected slot
        /// </summary>
        public OutContType PermanentType
        {
            get => permanentType;
            set
            {
                if (permanentType == value) return;

                if(value != OutContType.None)
                    AppLogger.LogToGui($"Output slot #{this.index+1} has permanent type {value}", false);

                permanentType = value;
                PermanentTypeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler PermanentTypeChanged;

        /// <summary>
        /// Device type of the current output controller
        /// </summary>
        public OutContType CurrentType { get => currentType; set => currentType = value; }

        public OutSlotDevice(int idx)
        {
            this.index = idx;
            CurrentReserveStatusChanged += OutSlotDevice_CurrentReserveStatusChanged;
        }

        private void OutSlotDevice_CurrentReserveStatusChanged(object sender, EventArgs e)
        {
            if (reserveStatus == ReserveStatus.Dynamic)
            {
                PermanentType = OutContType.None;
            }
            else if (currentType != OutContType.None)
            {
                PermanentType = currentType;
            }
        }

        public void AttachedDevice(OutputDevice outputDevice, OutContType contType, int inIdx, string inDisplayString)
        {
            this.outputDevice = outputDevice;
            attachedStatus = AttachedStatus.Attached;
            currentType = contType;
            inputIndex = inIdx;
            inputDisplayString = inDisplayString;
            //desiredType = contType;
        }

        public void DetachDevice()
        {
            if (outputDevice != null)
            {
                outputDevice = null;
                attachedStatus = AttachedStatus.UnAttached;
                currentType = OutContType.None;
                CurrentInputBound = InputBound.Unbound;
                if (reserveStatus == ReserveStatus.Dynamic)
                {
                    PermanentType = OutContType.None;
                }

                inputIndex = INPUT_INDEX_DEFAULT;
                inputDisplayString = string.Empty;
            }
        }

        ~OutSlotDevice()
        {
            DetachDevice();
        }
    }
}
