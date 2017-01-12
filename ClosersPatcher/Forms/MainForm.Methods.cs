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

using ClosersPatcher.General;
using ClosersPatcher.Helpers;
using ClosersPatcher.Helpers.GlobalVariables;
using MadMilkman.Ini;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ClosersPatcher.Forms
{
    public partial class MainForm
    {
        public void RestoreFromTray()
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Show();

            this.NotifyIcon.Visible = false;
        }

        private static void StartupBackupCheck(Language language)
        {
            if (Directory.Exists(Strings.FolderName.Backup))
            {
                if (Directory.GetFiles(Strings.FolderName.Backup, "*", SearchOption.AllDirectories).Length > 0)
                {
                    DialogResult result = MsgBox.Question(StringLoader.GetText("question_backup_files_found", language.Name));

                    if (result == DialogResult.Yes)
                    {
                        RestoreBackup(language);
                    }
                    else
                    {
                        Directory.Delete(Strings.FolderName.Backup, true);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(Strings.FolderName.Backup);
            }
        }

        private static void RestoreBackup(Language language)
        {
            if (!Directory.Exists(Strings.FolderName.Backup))
            {
                return;
            }

            string[] filePaths = Directory.GetFiles(Strings.FolderName.Backup, "*", SearchOption.AllDirectories);

            foreach (var file in filePaths)
            {
                string path = Path.Combine(UserSettings.GamePath, file.Substring(Strings.FolderName.Backup.Length + 1));
                Logger.Info($"Restoring file original=[{path}] backup=[{file}]");

                if (File.Exists(path))
                {
                    string langPath = Path.Combine(language.Name, path.Substring(UserSettings.GamePath.Length + 1));

                    File.Delete(langPath);
                    File.Move(path, langPath);
                }

                try
                {
                    File.Move(file, path);
                }
                catch (DirectoryNotFoundException)
                {
                    MsgBox.Error(StringLoader.GetText("exception_cannot_restore_file", Path.GetFullPath(file)));
                    Logger.Error($"Cannot restore file=[{file}]");
                    File.Delete(file);
                }
            }
        }

        private static string GetClosersPathFromRegistry()
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                using (RegistryKey key32 = Registry.LocalMachine.OpenSubKey(Strings.Registry.Key32))
                {
                    if (key32 != null)
                    {
                        return Convert.ToString(key32.GetValue(Strings.Registry.Name, String.Empty));
                    }
                    else
                    {
                        throw new Exception(StringLoader.GetText("exception_game_install_not_found"));
                    }
                }
            }
            else
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Strings.Registry.Key64))
                {
                    if (key != null)
                    {
                        return Convert.ToString(key.GetValue(Strings.Registry.Name, String.Empty));
                    }
                    else
                    {
                        using (RegistryKey key32 = Registry.LocalMachine.OpenSubKey(Strings.Registry.Key32))
                        {
                            if (key32 != null)
                            {
                                return Convert.ToString(key32.GetValue(Strings.Registry.Name, String.Empty));
                            }
                            else
                            {
                                throw new Exception(StringLoader.GetText("exception_game_install_not_found"));
                            }
                        }
                    }
                }
            }
        }

        private static Language[] GetAvailableLanguages()
        {
            List<Language> langs = new List<Language>();

            using (var client = new WebClient())
            {
                byte[] fileBytes = client.DownloadData(Urls.TranslationHome + Strings.IniName.LanguagePack);
                IniFile ini = new IniFile(new IniOptions
                {
                    Encoding = Encoding.UTF8
                });
                using (var ms = new MemoryStream(fileBytes))
                {
                    ini.Load(ms);
                }

                foreach (IniSection section in ini.Sections)
                {
                    langs.Add(new Language(section.Name, Methods.ParseDate(section.Keys[Strings.IniName.Pack.KeyDate].Value)));
                }
            }

            return langs.ToArray();
        }

        private static void DeleteTranslationIni(Language language)
        {
            string iniPath = Path.Combine(language.Name, Strings.IniName.Translation);
            if (Directory.Exists(Path.GetDirectoryName(iniPath)))
			{
                File.Delete(iniPath);
			}
        }

        private static string GetSHA256(string filename)
        {
            using (var sha256 = SHA256.Create())
            using (FileStream fs = File.OpenRead(filename))
            {
                return BitConverter.ToString(sha256.ComputeHash(fs)).Replace("-", "");
            }
        }

        private string UploadToPasteBin(string title, string text, PasteBinExpiration expiration, bool isPrivate, string format)
        {
            var client = new PasteBinClient(Strings.PasteBin.DevKey);

            try
            {
                client.Login(Strings.PasteBin.Username, Strings.PasteBin.Password);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            var entry = new PasteBinEntry
            {
                Title = title,
                Text = text,
                Expiration = expiration,
                Private = isPrivate,
                Format = format
            };

            try
            {
                return client.Paste(entry);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MsgBox.Error(StringLoader.GetText("exception_log_file_failed"));
            }
            finally
            {
                client.Logout();
            }

            return null;
        }

        private static byte[] TrimArrayIfNecessary(byte[] array)
        {
            int limit = 512000 / 2;

            if (array.Length > limit)
            {
                byte[] trimmedArray = new byte[limit];
                Array.Copy(array, array.Length - limit, trimmedArray, 0, limit);

                return trimmedArray;
            }

            return array;
        }
    }
}
