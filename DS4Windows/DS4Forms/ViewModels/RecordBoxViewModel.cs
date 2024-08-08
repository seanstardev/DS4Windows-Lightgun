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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class RecordBoxViewModel
    {
        private Stopwatch sw = new Stopwatch();
        private int deviceNum;
        public int DeviceNum { get => deviceNum; }

        private DS4ControlSettings settings;
        public DS4ControlSettings Settings { get => settings; }

        private bool shift;
        public bool Shift { get => shift; }

        private bool recordDelays;
        public bool RecordDelays { get => recordDelays; set => recordDelays = value; }

        private int macroModeIndex;
        public int MacroModeIndex { get => macroModeIndex; set => macroModeIndex = value; }

        private bool recording;
        public bool Recording { get => recording; set => recording = value; }

        private bool toggleLightbar;
        public bool ToggleLightbar { get => toggleLightbar; set => toggleLightbar = value; }

        private bool toggleRummble;
        public bool ToggleRumble { get => toggleRummble; set => toggleRummble = value; }

        private bool toggle4thMouse;
        private bool toggle5thMouse;
        private int appendIndex = -1;

        private object _colLockobj = new object();
        private ObservableCollection<MacroStepItem> macroSteps =
            new ObservableCollection<MacroStepItem>();
        public ObservableCollection<MacroStepItem> MacroSteps { get => macroSteps; }
        
        private int macroStepIndex;
        public int MacroStepIndex
        {
            get => macroStepIndex;
            set
            {
                if (macroStepIndex == value) return;
                macroStepIndex = value;
                MacroStepIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MacroStepIndexChanged;
        public Stopwatch Sw { get => sw; }
        public bool Toggle4thMouse { get => toggle4thMouse; set => toggle4thMouse = value; }
        public bool Toggle5thMouse { get => toggle5thMouse; set => toggle5thMouse = value; }
        public int AppendIndex { get => appendIndex; set => appendIndex = value; }
        public int EditMacroIndex { get => editMacroIndex; set => editMacroIndex = value; }
        public Dictionary<int, bool> KeysdownMap { get => keysdownMap; }
        public bool UseScanCode { get => useScanCode; set => useScanCode = value; }
        public static HashSet<int> KeydownOverrides { get => keydownOverrides; }

        private int editMacroIndex = -1;
        /// <summary>
        /// (Output value, active bool)
        /// </summary>
        private Dictionary<int, bool> keysdownMap = new Dictionary<int, bool>();
        private static HashSet<int> keydownOverrides;

        private Dictionary<int, bool> ds4InputMap = new Dictionary<int, bool>();

        private bool useScanCode;

        private bool repeatable;
        public bool Repeatable { get => repeatable; }

        /// <summary>
        /// Cached initial profile mode set for Touchpad.
        /// Needed to revert output control to Touchpad later
        /// </summary>
        private TouchpadOutMode oldTouchpadMode = TouchpadOutMode.None;


        public RecordBoxViewModel(int deviceNum, DS4ControlSettings controlSettings, bool shift, bool repeatable = true)
        {
            if (keydownOverrides == null)
            {
                CreateKeyDownOverrides();
            }

            this.deviceNum = deviceNum;
            settings = controlSettings;
            this.shift = shift;
            if (!shift && settings.keyType.HasFlag(DS4KeyType.HoldMacro))
            {
                macroModeIndex = 1;
            }
            else if (shift && settings.shiftKeyType.HasFlag(DS4KeyType.HoldMacro))
            {
                macroModeIndex = 1;
            }

            if (!shift && settings.keyType.HasFlag(DS4KeyType.ScanCode))
            {
                useScanCode = true;
            }
            else if (shift && settings.shiftKeyType.HasFlag(DS4KeyType.ScanCode))
            {
                useScanCode = true;
            }

            if (!shift && settings.actionType == DS4ControlSettings.ActionType.Macro)
            {
                LoadMacro();
            }
            else if (shift && settings.shiftActionType == DS4ControlSettings.ActionType.Macro)
            {
                LoadMacro();
            }

            this.repeatable = repeatable;

            BindingOperations.EnableCollectionSynchronization(macroSteps, _colLockobj);
            
            // By default RECORD button appends new steps. User must select (click) an existing step to insert new steps in front of the selected step
            this.MacroStepIndex = -1;

            MacroStepItem.CacheImgLocations();

            // Temporarily use Passthru mode for Touchpad. Store old TouchOutMode.
            // Don't conflict Touchpad Click with default output Mouse button controls
            oldTouchpadMode = Global.TouchOutMode[deviceNum];
            Global.TouchOutMode[deviceNum] = TouchpadOutMode.Passthru;
        }

        private void CreateKeyDownOverrides()
        {
            keydownOverrides = new HashSet<int>()
            {
                44,
            };
        }

        public void LoadMacro()
        {
            int[] macro;
            if (!shift)
            {
                macro = (int[])settings.action.actionMacro;
            }
            else
            {
                macro = (int[])settings.shiftAction.actionMacro;
            }

            MacroParser macroParser = new MacroParser(macro);
            macroParser.LoadMacro();
            foreach(MacroStep step in macroParser.MacroSteps)
            {
                MacroStepItem item = new MacroStepItem(step);
                macroSteps.Add(item);
            }
        }

        public void ExportMacro()
        {
            int[] outmac = new int[macroSteps.Count];
            int index = 0;
            foreach(MacroStepItem step in macroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            if (!shift)
            {
                settings.action.actionMacro = outmac;
                settings.actionType = DS4ControlSettings.ActionType.Macro;
                settings.keyType = DS4KeyType.Macro;
                if (macroModeIndex == 1)
                {
                    settings.keyType |= DS4KeyType.HoldMacro;
                }
                if (useScanCode)
                {
                    settings.keyType |= DS4KeyType.ScanCode;
                }
            }
            else
            {
                settings.shiftAction.actionMacro = outmac;
                settings.shiftActionType = DS4ControlSettings.ActionType.Macro;
                settings.shiftKeyType = DS4KeyType.Macro;
                if (macroModeIndex == 1)
                {
                    settings.shiftKeyType |= DS4KeyType.HoldMacro;
                }
                if (useScanCode)
                {
                    settings.shiftKeyType |= DS4KeyType.ScanCode;
                }
            }
        }

        public void WriteCycleProgramsPreset()
        {
            MacroStep step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(1300, $"Wait 1000ms",
                MacroStep.StepType.Wait, MacroStep.StepOutput.None);
            macroSteps.Add(new MacroStepItem(step));
        }

        public void LoadPresetFromFile(string filepath)
        {
            string[] macs = File.ReadAllText(filepath).Split('/');
            List<int> tmpmacro = new List<int>();
            int temp;
            foreach (string s in macs)
            {
                if (int.TryParse(s, out temp))
                    tmpmacro.Add(temp);
            }

            MacroParser macroParser = new MacroParser(tmpmacro.ToArray());
            macroParser.LoadMacro();
            foreach (MacroStep step in macroParser.MacroSteps)
            {
                MacroStepItem item = new MacroStepItem(step);
                macroSteps.Add(item);
            }
        }

        public void SavePreset(string filepath)
        {
            int[] outmac = new int[macroSteps.Count];
            int index = 0;
            foreach (MacroStepItem step in macroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            string macro = string.Join("/", outmac);
            StreamWriter sw = new StreamWriter(filepath);
            sw.Write(macro);
            sw.Close();
        }

        public void AddMacroStep(MacroStep step, bool ignoreDelay = false)
        {
            if (recordDelays && macroSteps.Count > 0 && !ignoreDelay)
            {
                int elapsed = (int)sw.ElapsedMilliseconds + 300;
                MacroStep waitstep = new MacroStep(elapsed, $"Wait {elapsed - 300}ms",
                    MacroStep.StepType.Wait, MacroStep.StepOutput.None);
                MacroStepItem waititem = new MacroStepItem(waitstep);
                if (appendIndex == -1)
                {
                    macroSteps.Add(waititem);
                }
                else
                {
                    macroSteps.Insert(appendIndex, waititem);
                    appendIndex++;
                }
            }

            sw.Restart();
            MacroStepItem item = new MacroStepItem(step);
            if (appendIndex == -1)
            {
                macroSteps.Add(item);
            }
            else
            {
                macroSteps.Insert(appendIndex, item);
                appendIndex++;
            }

            // If recording and not using delays already, check action type
            // and insert small wait period on Key ActDown. Needed
            // for some apps so events are not skipped in an event queue
            if (recording && !recordDelays &&
                step.ActType == MacroStep.StepType.ActDown &&
                step.OutputType == MacroStep.StepOutput.Key)
            {
                // 300 + WaitTime (ms)
                MacroStep waitstep = new MacroStep(MacroStep.WAIT_OFFSET + 50, $"Wait 50ms",
                    MacroStep.StepType.Wait, MacroStep.StepOutput.None);
                item = new MacroStepItem(waitstep);
                if (appendIndex == -1)
                {
                    macroSteps.Add(item);
                }
                else
                {
                    macroSteps.Insert(appendIndex, item);
                    appendIndex++;
                }
            }
        }

        public void InsertMacroStep(int index, MacroStep step)
        {
            MacroStepItem item = new MacroStepItem(step);
            macroSteps.Insert(index, item);
        }

        public void StartForcedColor(Color color)
        {
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Color dcolor = new DS4Color() { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[deviceNum] = dcolor;
                DS4LightBar.forcedFlash[deviceNum] = 0;
                DS4LightBar.forcelight[deviceNum] = true;
            }
        }

        public void EndForcedColor()
        {
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBar.forcedColor[deviceNum] = new DS4Color(0, 0, 0);
                DS4LightBar.forcedFlash[deviceNum] = 0;
                DS4LightBar.forcelight[deviceNum] = false;
            }
        }

        public void UpdateForcedColor(Color color)
        {
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4Color dcolor = new DS4Color() { red = color.R, green = color.G, blue = color.B };
                DS4LightBar.forcedColor[deviceNum] = dcolor;
                DS4LightBar.forcedFlash[deviceNum] = 0;
                DS4LightBar.forcelight[deviceNum] = true;
            }
        }

        public void ProcessDS4Tick()
        {
            if (Program.rootHub.DS4Controllers[0] != null)
            {
                DS4Device dev = Program.rootHub.DS4Controllers[0];
                DS4State cState = dev.getCurrentStateRef();
                DS4Windows.Mouse tp = Program.rootHub.touchPad[0];
                for (DS4Controls dc = DS4Controls.LXNeg; dc < DS4Controls.Mute; dc++)
                {
                    int macroValue = Global.macroDS4Values[dc];
                    ds4InputMap.TryGetValue((int)dc, out bool isdown);
                    keysdownMap.TryGetValue(macroValue, out bool outputExists);
                    if (!isdown && Mapping.GetBoolMappingExternal(0, dc, cState, null, tp))
                    {
                        MacroStep step = new MacroStep(macroValue, MacroParser.macroInputNames[macroValue],
                                MacroStep.StepType.ActDown, MacroStep.StepOutput.Button);
                        AddMacroStep(step);
                        ds4InputMap.Add((int)dc, true);
                        if (!outputExists)
                        {
                            keysdownMap.Add(macroValue, true);
                        }
                    }
                    else if (isdown && !Mapping.GetBoolMappingExternal(0, dc, cState, null, tp))
                    {
                        MacroStep step = new MacroStep(macroValue, MacroParser.macroInputNames[macroValue],
                                MacroStep.StepType.ActUp, MacroStep.StepOutput.Button);
                        AddMacroStep(step);
                        ds4InputMap.Remove((int)dc);
                        if (outputExists)
                        {
                            keysdownMap.Remove(macroValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Revert any necessary outside 
        /// </summary>
        public void RevertControlsSettings()
        {
            Global.TouchOutMode[deviceNum] = oldTouchpadMode;
            oldTouchpadMode = TouchpadOutMode.None;
        }
    }

    public class MacroStepItem
    {
        private static string[] imageSources = new string[]
        {
            $"{Global.RESOURCES_PREFIX}/{(string)App.Current.FindResource("KeyDownImg")}",
            $"{Global.RESOURCES_PREFIX}/{(string)App.Current.FindResource("KeyUpImg")}",
            $"{Global.RESOURCES_PREFIX}/{(string)App.Current.FindResource("ClockImg")}",
        };

        public static void CacheImgLocations()
        {
            imageSources = new string[]
            {
                $"{Global.RESOURCES_PREFIX}/{(string)App.Current.FindResource("KeyDownImg")}",
                $"{Global.RESOURCES_PREFIX}/{(string)App.Current.FindResource("KeyUpImg")}",
                $"{Global.RESOURCES_PREFIX}/{(string)App.Current.FindResource("ClockImg")}",
            };
        }

        private MacroStep step;
        private string image;

        public string Image { get => image; }
        public MacroStep Step { get => step; }
        public int DisplayValue
        {
            get
            {
                int result = step.Value;
                if (step.ActType == MacroStep.StepType.Wait)
                {
                    result -= 300;
                }

                return result;
            }
            set
            {
                int result = value;
                if (step.ActType == MacroStep.StepType.Wait)
                {
                    result += 300;
                }

                step.Value = result;
            }
        }

        public int RumbleHeavy
        {
            get
            {
                int result = step.Value;
                result -= 1000000;
                string temp = result.ToString();
                result = int.Parse(temp.Substring(0, 3));
                return result;
            }
            set
            {
                int result = step.Value;
                result -= 1000000;
                int curHeavy = result / 1000;
                int curLight = result - (curHeavy * 1000);
                result = curLight + (value * 1000) + 1000000;
                step.Value = result;
            }
        }

        public int RumbleLight
        {
            get
            {
                int result = step.Value;
                result -= 1000000;
                string temp = result.ToString();
                result = int.Parse(temp.Substring(3, 3));
                return result;
            }
            set
            {
                int result = step.Value;
                result -= 1000000;
                int curHeavy = result / 1000;
                result = value + (curHeavy * 1000) + 1000000;
                step.Value = result;
            }
        }

        public MacroStepItem(MacroStep step)
        {
            this.step = step;
            image = imageSources[(int)step.ActType];
        }

        public void UpdateLightbarValue(Color color)
        {
            step.Value = 1000000000 + (color.R*1000000)+(color.G*1000)+color.B;
        }

        public Color LightbarColorValue()
        {
            int temp = step.Value - 1000000000;
            int r = temp / 1000000;
            temp -= (r * 1000000);
            int g = temp / 1000;
            temp -= (g * 1000);
            int b = temp;
            return new Color() { A = 255, R = (byte)r, G = (byte)g, B = (byte)b };
        }
    }
}
