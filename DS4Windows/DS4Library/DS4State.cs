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

namespace DS4Windows
{
    public class DS4State
    {
        public uint PacketCounter;
        public DateTime ReportTimeStamp;
        public bool Square, Triangle, Circle, Cross;
        public bool DpadUp, DpadDown, DpadLeft, DpadRight;
        public bool L1, L2Btn, L3, R1, R2Btn, R3;
        public bool Share, Options, PS, Mute, Touch1, Touch2, TouchButton, TouchRight,
            TouchLeft, Touch1Finger, Touch2Fingers, OutputTouchButton,
            Capture, SideL, SideR, FnL, FnR, BLP, BRP;
        public byte Touch1Identifier, Touch2Identifier;
        public byte LX, RX, LY, RY, L2, R2;
        public byte L2Raw, R2Raw;
        public byte FrameCounter; // 0, 1, 2...62, 63, 0....
        public byte TouchPacketCounter; // we break these out automatically
        public byte Battery; // 0 for charging, 10/20/30/40/50/60/70/80/90/100 for percentage of full
        public double LSAngle; // Calculated bearing of the LS X,Y coordinates
        public double RSAngle; // Calculated bearing of the RS X,Y coordinates
        public double LSAngleRad; // Calculated bearing of the LS X,Y coordinates (in radians)
        public double RSAngleRad; // Calculated bearing of the RS X,Y coordinates (in radians)
        public double LXUnit;
        public double LYUnit;
        public double RXUnit;
        public double RYUnit;
        public byte OutputLSOuter = 0, OutputRSOuter = 0;
        public double elapsedTime = 0.0;
        public ulong totalMicroSec = 0;
        public ushort ds4Timestamp = 0;
        public SixAxis Motion = null;
        public static readonly int DEFAULT_AXISDIR_VALUE = 127;
        public Int32 SASteeringWheelEmulationUnit;

        public struct TrackPadTouch
        {
            public bool IsActive;
            public byte Id;
            public short X;
            public short Y;
            public byte RawTrackingNum;
        }

        public TrackPadTouch TrackPadTouch0;
        public TrackPadTouch TrackPadTouch1;

        public DS4State()
        {
            PacketCounter = 0;
            Square = Triangle = Circle = Cross = false;
            DpadUp = DpadDown = DpadLeft = DpadRight = false;
            L1 = L2Btn = L3 = R1 = R2Btn = R3 = false;
            Share = Options = PS = Mute = Touch1 = Touch2 = TouchButton =
                OutputTouchButton = TouchRight = TouchLeft =
                Capture = SideL = SideR =
                FnL = FnR = BLP = BRP = false;
            Touch1Finger = Touch2Fingers = false;
            LX = RX = LY = RY = 128;
            L2 = R2 = 0;
            L2Raw = R2Raw = 0;
            FrameCounter = 255; // only actually has 6 bits, so this is a null indicator
            TouchPacketCounter = 255; // 8 bits, no great junk value
            Battery = 0;
            LSAngle = 0.0;
            LSAngleRad = 0.0;
            RSAngle = 0.0;
            RSAngleRad = 0.0;
            LXUnit = 0.0;
            LYUnit = 0.0;
            RXUnit = 0.0;
            RYUnit = 0.0;
            elapsedTime = 0.0;
            totalMicroSec = 0;
            ds4Timestamp = 0;
            Motion = new SixAxis(0, 0, 0, 0, 0, 0, 0.0);
            TrackPadTouch0.IsActive = false;
            TrackPadTouch1.IsActive = false;
            SASteeringWheelEmulationUnit = 0;
            OutputLSOuter = OutputRSOuter = 0;
        }

        public DS4State(DS4State state)
        {
            PacketCounter = state.PacketCounter;
            ReportTimeStamp = state.ReportTimeStamp;
            Square = state.Square;
            Triangle = state.Triangle;
            Circle = state.Circle;
            Cross = state.Cross;
            DpadUp = state.DpadUp;
            DpadDown = state.DpadDown;
            DpadLeft = state.DpadLeft;
            DpadRight = state.DpadRight;
            L1 = state.L1;
            L2 = state.L2;
            L2Raw = state.L2Raw;
            L2Btn = state.L2Btn;
            L3 = state.L3;
            R1 = state.R1;
            R2 = state.R2;
            R2Raw = state.R2Raw;
            R2Btn = state.R2Btn;
            R3 = state.R3;
            Share = state.Share;
            Options = state.Options;
            PS = state.PS;
            Mute = state.Mute;
            FnL = state.FnL;
            FnR = state.FnR;
            BLP = state.BLP;
            BRP = state.BRP;
            Capture = state.Capture;
            SideL = state.SideL;
            SideR = state.SideR;
            Touch1 = state.Touch1;
            TouchRight = state.TouchRight;
            TouchLeft = state.TouchLeft;
            Touch1Identifier = state.Touch1Identifier;
            Touch2 = state.Touch2;
            Touch2Identifier = state.Touch2Identifier;
            TouchButton = state.TouchButton;
            OutputTouchButton = state.OutputTouchButton;
            TouchPacketCounter = state.TouchPacketCounter;
            Touch1Finger = state.Touch1Finger;
            Touch2Fingers = state.Touch2Fingers;
            LX = state.LX;
            RX = state.RX;
            LY = state.LY;
            RY = state.RY;
            FrameCounter = state.FrameCounter;
            Battery = state.Battery;
            LSAngle = state.LSAngle;
            LSAngleRad = state.LSAngleRad;
            RSAngle = state.RSAngle;
            RSAngleRad = state.RSAngleRad;
            LXUnit = state.LXUnit;
            LYUnit = state.LYUnit;
            RXUnit = state.RXUnit;
            RYUnit = state.RYUnit;
            elapsedTime = state.elapsedTime;
            totalMicroSec = state.totalMicroSec;
            ds4Timestamp = state.ds4Timestamp;
            Motion = state.Motion;
            TrackPadTouch0 = state.TrackPadTouch0;
            TrackPadTouch1 = state.TrackPadTouch1;
            SASteeringWheelEmulationUnit = state.SASteeringWheelEmulationUnit;
            OutputLSOuter = state.OutputLSOuter;
            OutputRSOuter = state.OutputRSOuter;
        }

        public DS4State Clone()
        {
            return new DS4State(this);
        }

        public void CopyTo(DS4State state)
        {
            state.PacketCounter = PacketCounter;
            state.ReportTimeStamp = ReportTimeStamp;
            state.Square = Square;
            state.Triangle = Triangle;
            state.Circle = Circle;
            state.Cross = Cross;
            state.DpadUp = DpadUp;
            state.DpadDown = DpadDown;
            state.DpadLeft = DpadLeft;
            state.DpadRight = DpadRight;
            state.L1 = L1;
            state.L2 = L2;
            state.L2Raw = L2Raw;
            state.L2Btn = L2Btn;
            state.L3 = L3;
            state.R1 = R1;
            state.R2 = R2;
            state.R2Raw = R2Raw;
            state.R2Btn = R2Btn;
            state.R3 = R3;
            state.Share = Share;
            state.Options = Options;
            state.PS = PS;
            state.Mute = Mute;
            state.FnL = FnL;
            state.FnR = FnR;
            state.BLP = BLP;
            state.BRP = BRP;
            state.Capture = Capture;
            state.SideL = SideL;
            state.SideR = SideR;
            state.Touch1 = Touch1;
            state.Touch1Identifier = Touch1Identifier;
            state.Touch2 = Touch2;
            state.Touch2Identifier = Touch2Identifier;
            state.TouchLeft = TouchLeft;
            state.TouchRight = TouchRight;
            state.TouchButton = TouchButton;
            state.OutputTouchButton = OutputTouchButton;
            state.TouchPacketCounter = TouchPacketCounter;
            state.Touch1Finger = Touch1Finger;
            state.Touch2Fingers = Touch2Fingers;
            state.LX = LX;
            state.RX = RX;
            state.LY = LY;
            state.RY = RY;
            state.FrameCounter = FrameCounter;
            state.Battery = Battery;
            state.LSAngle = LSAngle;
            state.LSAngleRad = LSAngleRad;
            state.RSAngle = RSAngle;
            state.RSAngleRad = RSAngleRad;
            state.LXUnit = LXUnit;
            state.LYUnit = LYUnit;
            state.RXUnit = RXUnit;
            state.RYUnit = RYUnit;
            state.elapsedTime = elapsedTime;
            state.totalMicroSec = totalMicroSec;
            state.ds4Timestamp = ds4Timestamp;
            state.Motion = Motion;
            state.TrackPadTouch0 = TrackPadTouch0;
            state.TrackPadTouch1 = TrackPadTouch1;
            state.SASteeringWheelEmulationUnit = SASteeringWheelEmulationUnit;
            state.OutputLSOuter = OutputLSOuter;
            state.OutputRSOuter = OutputRSOuter;
        }

        /// <summary>
        /// Only copy extra DS4State data that is not output directly tied
        /// to the mapper routine. Gyro motion data, Touchpad touch data,
        /// and timestamp data are copied
        /// </summary>
        /// <param name="state">State object to copy data to</param>
        public void CopyExtrasTo(DS4State state)
        {
            state.Motion = Motion;
            state.ds4Timestamp = ds4Timestamp;
            state.FrameCounter = FrameCounter;
            state.TouchPacketCounter = TouchPacketCounter;
            state.TrackPadTouch0 = TrackPadTouch0;
            state.TrackPadTouch1 = TrackPadTouch1;
        }

        public void calculateStickAngles()
        {
            double lsangle = Math.Atan2(-(LY - 128), (LX - 128));
            LSAngleRad = lsangle;
            lsangle = (lsangle >= 0 ? lsangle : (2 * Math.PI + lsangle)) * 180 / Math.PI;
            LSAngle = lsangle;
            LXUnit = Math.Abs(Math.Cos(LSAngleRad));
            LYUnit = Math.Abs(Math.Sin(LSAngleRad));

            double rsangle = Math.Atan2(-(RY - 128), (RX - 128));
            RSAngleRad = rsangle;
            rsangle = (rsangle >= 0 ? rsangle : (2 * Math.PI + rsangle)) * 180 / Math.PI;
            RSAngle = rsangle;
            RXUnit = Math.Abs(Math.Cos(RSAngleRad));
            RYUnit = Math.Abs(Math.Sin(RSAngleRad));
        }

        /// <summary>
        /// Rotate LX and LY by a rotation angle (radians)
        /// </summary>
        /// <param name="rotationRad">Rotation angle in radians</param>
        public void rotateLSCoordinates(double rotationRad)
        {
            double sinAngle = Math.Sin(rotationRad), cosAngle = Math.Cos(rotationRad);
            double tempLX = LX - 128.0, tempLY = LY - 128.0;
            LX = (Byte)(Global.Clamp(-128.0, (tempLX * cosAngle - tempLY * sinAngle), 127.0) + 128.0);
            LY = (Byte)(Global.Clamp(-128.0, (tempLX * sinAngle + tempLY * cosAngle), 127.0) + 128.0);
        }

        /// <summary>
        /// Rotate RX and RY by a rotation angle (radians)
        /// </summary>
        /// <param name="rotationRad">Rotation angle in radians</param>
        public void rotateRSCoordinates(double rotationRad)
        {
            double sinAngle = Math.Sin(rotationRad), cosAngle = Math.Cos(rotationRad);
            double tempRX = RX - 128.0, tempRY = RY - 128.0;
            RX = (Byte)(Global.Clamp(-128.0, (tempRX * cosAngle - tempRY * sinAngle), 127.0) + 128.0);
            RY = (Byte)(Global.Clamp(-128.0, (tempRX * sinAngle + tempRY * cosAngle), 127.0) + 128.0);
        }
    }
}
