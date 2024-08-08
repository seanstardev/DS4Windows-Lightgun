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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    [StructLayout(LayoutKind.Sequential, Size = 9)]
    unsafe struct DS4_TOUCH
    {
        public byte bPacketCounter;
        public byte bIsUpTrackingNum1;
        public fixed byte bTouchData1[3];
        public byte bIsUpTrackingNum2;
        public fixed byte bTouchData2[3];
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe struct DS4_REPORT_UNION
    {
        [FieldOffset(0)]
        public DS4_REPORT_EX reportStruct;

        [FieldOffset(0)]
        public fixed byte Report[63];
    }

    /// <summary>
    /// Used to set data for DS4 Extended output report. StructLayout
    /// will be used to align data for a raw byte array of 63 bytes.
    /// ViGEmBus will place report ID byte into the output so this data
    /// will technically start with byte 1 of the final output report
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 63)]
    unsafe struct DS4_REPORT_EX
    {
        [FieldOffset(0)]
        public byte bThumbLX;
        [FieldOffset(1)]
        public byte bThumbLY;
        [FieldOffset(2)]
        public byte bThumbRX;
        [FieldOffset(3)]
        public byte bThumbRY;
        [FieldOffset(4)]
        public ushort wButtons;
        [FieldOffset(6)]
        public byte bSpecial;
        [FieldOffset(7)]
        public byte bTriggerL;
        [FieldOffset(8)]
        public byte bTriggerR;
        [FieldOffset(9)]
        public ushort wTimestamp;
        [FieldOffset(11)]
        public byte bBatteryLvl;
        [FieldOffset(12)]
        public short wGyroX;
        [FieldOffset(14)]
        public short wGyroY;
        [FieldOffset(16)]
        public short wGyroZ;
        [FieldOffset(18)]
        public short wAccelX;
        [FieldOffset(20)]
        public short wAccelY;
        [FieldOffset(22)]
        public short wAccelZ;
        [FieldOffset(24)]
        public fixed byte _bUnknown1[5];
        [FieldOffset(29)]
        public byte bBatteryLvlSpecial;
        [FieldOffset(30)]
        public fixed byte _bUnknown2[2];
        [FieldOffset(32)]
        public byte bTouchPacketsN;
        [FieldOffset(33)]
        public DS4_TOUCH sCurrentTouch;
        [FieldOffset(42)]
        public DS4_TOUCH sPreviousTouch1;
        [FieldOffset(51)]
        public DS4_TOUCH sPreviousTouch2;
    }

    /// <summary>
    /// Example struct for converting output report buffer array returned
    /// from IDualShock4Controller.AwaitRawOutputReport method to a struct.
    /// Used for testing and documentation. Probably will not use tbh
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct DS4OutputBufferData
    {
        [FieldOffset(0)]
        public byte reportID;
        [FieldOffset(1)]
        public byte featureFlags;
        [FieldOffset(2)]
        public byte padding1;
        [FieldOffset(3)]
        public byte padding2;
        [FieldOffset(4)]
        public byte rightFastRumble;
        [FieldOffset(5)]
        public byte leftSlowRumble;
        [FieldOffset(6)]
        public byte lightbarRedColor;
        [FieldOffset(7)]
        public byte lightbarGreenColor;
        [FieldOffset(8)]
        public byte lightbarBlueColor;
        [FieldOffset(9)]
        public byte flashOnDuration;
        [FieldOffset(10)]
        public byte flashOffDuration;
    }

    internal static class DS4OutDeviceExtras
    {
        public static void CopyBytes(ref DS4_REPORT_EX outReport, byte[] outBuffer)
        {
            GCHandle h = GCHandle.Alloc(outReport, GCHandleType.Pinned);
            Marshal.Copy(h.AddrOfPinnedObject(), outBuffer, 0, 63);
            h.Free();
        }

        // Mainly adding this as an example how to convert from a byte array
        // to a struct. Probably will not use
        public static DS4OutputBufferData ConvertOutputBufferArrayToStruct(byte[] rawOutputBuffer)
        {
            GCHandle pData = GCHandle.Alloc(rawOutputBuffer, GCHandleType.Pinned);
            DS4OutputBufferData outputBufferData =
                Marshal.PtrToStructure<DS4OutputBufferData>(pData.AddrOfPinnedObject());
            pData.Free();
            return outputBufferData;

            //int size = Marshal.SizeOf<DS4OutputBufferData>();
            //IntPtr ptr = Marshal.AllocHGlobal(size);
            //Marshal.Copy(rawOutputBuffer, 0, ptr, size);
            //Marshal.PtrToStructure<DS4OutputBufferData>(ptr, outputBufferData);
            //Marshal.FreeHGlobal(ptr);
        }
    }
}
