﻿/*
 * This file is part of Closers Patcher.
 * Copyright (C) 2016-2017 Miyu, Dramiel Leayal
 * 
 * Closers Patcher is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Closers Patcher is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Closers Patcher. If not, see <http://www.gnu.org/licenses/>.
 */

using ClosersPatcher.Properties;
using System;
using System.IO;
using System.Reflection;

namespace ClosersPatcher.Helpers.GlobalVariables
{
    public static class UserSettings
    {
        public static string PatcherPath
        {
            get
            {
                if (String.IsNullOrEmpty(Settings.Default.PatcherWorkingDirectory))
                {
                    return PatcherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name);
                }
                else
                {
                    return Settings.Default.PatcherWorkingDirectory.Replace("\\\\", "\\");
                }
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    value = value.Replace("\\\\", "\\");
                    Directory.CreateDirectory(value);
                    Directory.SetCurrentDirectory(value);
                }

                Settings.Default.PatcherWorkingDirectory = value;
                Settings.Default.Save();
                Logger.Info($"Patcher path set to [{value}]");
            }
        }

        public static string GamePath
        {
            get
            {
                return Settings.Default.GameDirectory.Replace("\\\\", "\\");
            }
            set
            {
                value = value.Replace("\\\\", "\\");
                Settings.Default.GameDirectory = value;
                Settings.Default.Save();
                Logger.Info($"Soulworker path set to [{value}]");
            }
        }

        public static string LanguageName
        {
            get
            {
                return Settings.Default.LanguageName;
            }
            set
            {
                Settings.Default.LanguageName = value;
                Settings.Default.Save();
            }
        }

        public static string UILanguageCode
        {
            get
            {
                return Settings.Default.UILanguage;
            }
            set
            {
                Settings.Default.UILanguage = value;
                Settings.Default.Save();
                Logger.Info($"UI Language set to [{value}]");
            }
        }

        public static bool HasSound
        {
            get
            {
                return Settings.Default.Sound;
            }
            set
            {
                Settings.Default.Sound = value;
                Settings.Default.Save();
            }
        }
    }
}
