// ShowPlay - Show what song is playing
//
// Copyright (C) 2020-2021 VacuityBox
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
// SPDX-License-Identifier: GPL-3.0-only 

using System.Windows.Media;

namespace ShowPlay
{
    public static class LogColors
    {
        #region Colors

        public static Color DebugForegroundStandard    = Color.FromArgb(255,  48,  48, 208);
        public static Color DebugBackgroundStandard    = Color.FromArgb(128,   8,   8,  16);
        public static Color DebugForegroundHighlight   = Color.FromArgb(255,  64,  64, 255);
        public static Color DebugBackgroundHighlight   = Color.FromArgb(255,   8,   8,  64);

        public static Color InfoForegroundStandard     = Color.FromArgb(255, 208, 208, 208);
        public static Color InfoBackgroundStandard     = Color.FromArgb(255,   0,   0,   0);
        public static Color InfoForegroundHighlight    = Color.FromArgb(255, 255, 255, 255);
        public static Color InfoBackgroundHighlight    = Color.FromArgb(255,  32,  32,  32);

        public static Color SuccessForegroundStandard  = Color.FromArgb(255,  48, 144,  48);
        public static Color SuccessBackgroundStandard  = Color.FromArgb(128,   8,  32,   8);
        public static Color SuccessForegroundHighlight = Color.FromArgb(255,  64, 255,  64);
        public static Color SuccessBackgroundHighlight = Color.FromArgb(255,  16,  64,  16);

        public static Color WarningForegroundStandard  = Color.FromArgb(255, 144, 144,  48);
        public static Color WarningBackgroundStandard  = Color.FromArgb(128,  16,  16,   8);
        public static Color WarningForegroundHighlight = Color.FromArgb(255, 255, 255,  64);
        public static Color WarningBackgroundHighlight = Color.FromArgb(255,  48,  48,  16);

        public static Color ErrorForegroundStandard    = Color.FromArgb(255, 192,  32,  32);
        public static Color ErrorBackgroundStandard    = Color.FromArgb(128,  32,   8,   8);
        public static Color ErrorForegroundHighlight   = Color.FromArgb(255, 255,  48,  48);
        public static Color ErrorBackgroundHighlight   = Color.FromArgb(255,  64,  16,  16);

        public static Color FatalForegroundStandard    = Color.FromArgb(255, 240, 240, 240);
        public static Color FatalBackgroundStandard    = Color.FromArgb(255, 240,  16,  16);
        public static Color FatalForegroundHighlight   = Color.FromArgb(255, 255,  32,  32);
        public static Color FatalBackgroundHighlight   = Color.FromArgb(  0, 240,  16,  16);

        #endregion

        #region Public Methods

        public static Color GetForegroundColor(LogType type, bool highlight = false)
        {
            if (!highlight)
            {
                switch (type)
                {
                    case LogType.Debug:   return DebugForegroundStandard;
                    case LogType.Info:    return InfoForegroundStandard;
                    case LogType.Success: return SuccessForegroundStandard;
                    case LogType.Warning: return WarningForegroundStandard;
                    case LogType.Error:   return ErrorForegroundStandard;
                    case LogType.Fatal:   return FatalForegroundStandard;
                }
            }
            else
            {
                switch (type)
                {
                    case LogType.Debug:   return DebugForegroundHighlight;
                    case LogType.Info:    return InfoForegroundHighlight;
                    case LogType.Success: return SuccessForegroundHighlight;
                    case LogType.Warning: return WarningForegroundHighlight;
                    case LogType.Error:   return ErrorForegroundHighlight;
                    case LogType.Fatal:   return FatalForegroundHighlight;
                }
            }

            return Colors.Transparent;
        }

        public static Color GetBackgroundColor(LogType type, bool highlight = false)
        {
            if (!highlight)
            {
                switch (type)
                {
                    case LogType.Debug:   return DebugBackgroundStandard;
                    case LogType.Info:    return InfoBackgroundStandard;
                    case LogType.Success: return SuccessBackgroundStandard;
                    case LogType.Warning: return WarningBackgroundStandard;
                    case LogType.Error:   return ErrorBackgroundStandard;
                    case LogType.Fatal:   return FatalBackgroundStandard;
                }
            }
            else
            {
                switch (type)
                {
                    case LogType.Debug:   return DebugBackgroundHighlight;
                    case LogType.Info:    return InfoBackgroundHighlight;
                    case LogType.Success: return SuccessBackgroundHighlight;
                    case LogType.Warning: return WarningBackgroundHighlight;
                    case LogType.Error:   return ErrorBackgroundHighlight;
                    case LogType.Fatal:   return FatalBackgroundHighlight;
                }
            }

            return Colors.Transparent;
        }

        #endregion
    }
}
