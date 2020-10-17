#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using hw.Debug;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TextTemplating;
// ReSharper disable CheckNamespace

namespace hw.Helper
{
    public sealed class T4Context
    {
        const string RegionName = "Generated Code";
        const string RegionFrame = "#region {0}\n\n{1}#endregion {0}\n";
        const string HeaderText = "// Generated by {0}\n// Timestamp: {1}\n";

        readonly FunctionCache<string, Box<string>> _fileItems;
        readonly StringBuilder _text;
        readonly ITextTemplatingEngineHost _host;
        readonly ValueCache<DTE> _dte;
        string[] _currentFiles;
        int _currentStart;

        internal T4Context(StringBuilder text, ITextTemplatingEngineHost host)
        {
            _text = text;
            _host = host;
            _fileItems = new FunctionCache<string, Box<string>>(fileName => new Box<string>(""));
            _dte = new ValueCache<DTE>(ObtainDTE);
        }

        [UsedImplicitly]
        public string NameSpace { get { return _host.ResolveParameterValue("directiveId", "namespaceDirectiveProcessor", "namespaceHint") ?? ""; } }

        [UsedImplicitly]
        public string File
        {
            set
            {
                if(value == null)
                    SetFiles();
                else
                    SetFiles(value);
            }
        }

        [UsedImplicitly]
        public void SetFiles(params string[] files)
        {
            OnFilesChanging();
            Tracer.Assert(_currentFiles == null);
            // Special treatment of empty file list: send text to default target
            if(files.Length > 0)
                _currentFiles = files;
        }

        string MainFileName { get { return Path.GetFileNameWithoutExtension(_host.TemplateFile); } }
        string MainPath { get { return Path.GetDirectoryName(_host.TemplateFile); } }
        static string Extension { get { return ".cs"; } }
        string FullMainFileName { get { return MainPath.PathCombine(MainFileName) + Extension; } }

        [UsedImplicitly]
        public void ProcessFiles()
        {
            OnFilesChanging();
            //Tracer.LaunchDebugger();
            TreatMainFile();
            foreach(var fileItem in _fileItems)
                CreateFile(MainPath.PathCombine(fileItem.Key), fileItem.Value.Content);

            var newFiles = _fileItems.Keys.Select(name => MainPath.PathCombine(name)).ToArray();

            Action projectSyncAction = () => ProjectSync(newFiles);
            projectSyncAction.EndInvoke(projectSyncAction.BeginInvoke(null, null));
        }

        void TreatMainFile()
        {
            var text = CreateFile(FullMainFileName, _text.ToString());
            _text.Clear();
            _text.Append(text);
        }

        DTE DTE { get { return _dte.Value; } }

        void OnFilesChanging()
        {
            if(_currentFiles != null)
            {
                foreach(var box in _currentFiles.Select(file => _fileItems[file]))
                    box.Content += _text.ToString(_currentStart, _text.Length - _currentStart);
                _text.Remove(_currentStart, _text.Length - _currentStart);
            }

            _currentFiles = null;
            _currentStart = _text.Length;
        }

        string CreateFile(string fileName, string content)
        {
            var regioned = ModificationSensitiveContent(content);

            var oldFile = fileName.FileHandle();
            var result = oldFile.String;
            if(oldFile.Exists && result.EndsWith(regioned))
                return result;

            CheckoutFileIfRequired(fileName);
            result = TimeDependantHeader + regioned;
            fileName.FileHandle().String = result;
            return result;
        }

        string TimeDependantHeader { get { return string.Format(HeaderText, GetType().FullName, DateTime.Now.ToString("o")); } }
        static string ModificationSensitiveContent(string content) { return string.Format(RegionFrame, RegionName, content); }

        void CheckoutFileIfRequired(string fileName)
        {
            if(DTE == null || DTE.SourceControl == null || !DTE.SourceControl.IsItemUnderSCC(fileName) || DTE.SourceControl.IsItemCheckedOut(fileName))
                return;

            Action<string> checkOutAction = name => DTE.SourceControl.CheckOutItem(name);
            checkOutAction.EndInvoke(checkOutAction.BeginInvoke(fileName, null, null));
        }

        void ProjectSync(string[] newFiles)
        {
            if(DTE == null)
                return;

            var item = DTE.Solution.FindProjectItem(_host.TemplateFile);
            if(item == null || item.ProjectItems == null)
                return;

            var projectFiles = item.ProjectItems.Cast<ProjectItem>().ToDictionary(projectItem => projectItem.FileNames[0]);

            // Remove unused items from the project
            var toDelete = projectFiles.Where(pair => !newFiles.Contains(pair.Key) && pair.Key != FullMainFileName);
            foreach(var pair in toDelete)
                pair.Value.Delete();

            // Add missing files to the project
            foreach(var fileName in newFiles.Where(fileName => !projectFiles.ContainsKey(fileName)))
                item.ProjectItems.AddFromFile(fileName);
        }

        DTE ObtainDTE()
        {
            var provider = _host as IServiceProvider;
            if(provider == null)
                return null;
            return (DTE) provider.GetService(typeof(DTE));
        }

        void AppendText(string text) { _text.Append(text); }
    }
}