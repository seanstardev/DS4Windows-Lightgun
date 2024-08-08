using DS4Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ss7
namespace DS4WinWPF {
    public static class SeanstarHelper {

        public enum LightgunBtnEnum {
            NOT_SET
            , CROSS_A
            , CIRCLE_B
            , SQUARE_X
            , TRIANGLE_Y
            , L1
            , R1
        }

        public static LightgunBtnEnum GetBtn(int btn) {
            switch (btn) {
                case 0: return LightgunBtnEnum.NOT_SET;
                case 1: return LightgunBtnEnum.CROSS_A;
                case 2: return LightgunBtnEnum.CIRCLE_B;
                case 3: return LightgunBtnEnum.SQUARE_X;
                case 4: return LightgunBtnEnum.TRIANGLE_Y;
                case 5: return LightgunBtnEnum.L1;
                case 6: return LightgunBtnEnum.R1;
            }
            return LightgunBtnEnum.NOT_SET;
        }

        public static bool GetLightgunBtnPressed(int btn, DS4State cState) {
            LightgunBtnEnum btnEnum = GetBtn(btn);

            switch (btnEnum) {
                case LightgunBtnEnum.CROSS_A:
                    return cState.Cross;
                case LightgunBtnEnum.CIRCLE_B:
                    return cState.Circle;
                case LightgunBtnEnum.SQUARE_X:
                    return cState.Square;
                case LightgunBtnEnum.TRIANGLE_Y:
                    return cState.Triangle;
                case LightgunBtnEnum.L1:
                    return cState.L1;
                case LightgunBtnEnum.R1:
                    return cState.R1;
                default:
                    return false;
            }
        }

        public static void SetLightgunBtnPressed(int btn, DS4State cState) {
            LightgunBtnEnum btnEnum = GetBtn(btn);
            switch (btnEnum) {
                case LightgunBtnEnum.CROSS_A:
                    cState.Cross = true;
                    break;
                case LightgunBtnEnum.CIRCLE_B:
                    cState.Circle = true; 
                    break;
                case LightgunBtnEnum.SQUARE_X:
                    cState.Square = true;
                    break;
                case LightgunBtnEnum.TRIANGLE_Y:
                    cState.Triangle = true;
                    break;
                case LightgunBtnEnum.L1:
                    cState.L1 = true;
                    break;
                case LightgunBtnEnum.R1:
                    cState.R1 = true;
                    break;
                default:
                    break;
            }
        }
    }
}
