// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2011 Harald Hoyer
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

using System.IO;
using System.Text;
using EnvDTE;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TextTemplating;

namespace HWClassLibrary.T4
{
    public class Context
    {
        readonly DictionaryEx<string, Box<string>> _fileItems;
        readonly StringBuilder _text;
        readonly ITextTemplatingEngineHost _host;
        readonly SimpleCache<DTE> _dte;
        string[] _currentFiles;
        int _currentStart;

        internal Context(StringBuilder text, ITextTemplatingEngineHost host)
        {
            _text = text;
            _host = host;
            _fileItems = new DictionaryEx<string, Box<string>>(fileName => new Box<string>(""));
            _dte = new SimpleCache<DTE>(ObtainDTE);
        }

        [UsedImplicitly]
        public string NameSpace
        {
            get
            {
                return _host.ResolveParameterValue("directiveId", "namespaceDirectiveProcessor", "namespaceHint")
                       ?? "";
            }
        }

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

        [UsedImplicitly]
        public void ProcessFiles()
        {
            OnFilesChanging();

            //Tracer.LaunchDebugger();
            var outputPath = Path.GetDirectoryName(_host.TemplateFile) ?? "";
            foreach(var fileItem in _fileItems)
                CreateFile(Path.Combine(outputPath, fileItem.Key), fileItem.Value.Content);

            var newFiles = _fileItems.Keys.Select(name => Path.Combine(outputPath, name)).ToArray();

            Action projectSyncAction = () => ProjectSync(newFiles);
            projectSyncAction.EndInvoke(projectSyncAction.BeginInvoke(null, null));
        }

        DTE DTE { get { return _dte.Value; } }

        void OnFilesChanging()
        {
            if(_currentFiles != null)
            {
                foreach(var file in _currentFiles)
                {
                    var box = _fileItems.Find(file);
                    box.Content = box.Content + _text.ToString(_currentStart, _text.Length - _currentStart);
                }
                _text.Remove(_currentStart, _text.Length - _currentStart);
            }

            _currentFiles = null;
            _currentStart = _text.Length;
        }

        void CreateFile(string fileName, string content)
        {
            if(!Extender.IsFileContentDifferent(fileName, content))
                return;

            CheckoutFileIfRequired(fileName);
            System.IO.File.WriteAllText(fileName, content);
        }

        void CheckoutFileIfRequired(string fileName)
        {
            if(DTE == null
               || DTE.SourceControl == null
               || !DTE.SourceControl.IsItemUnderSCC(fileName)
               || DTE.SourceControl.IsItemCheckedOut(fileName))
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

            var defaultFile = Path.GetFileNameWithoutExtension(item.FileNames[0]);

            var projectFiles = item
                .ProjectItems
                .Cast<ProjectItem>()
                .ToDictionary(projectItem => projectItem.FileNames[0]);

            // Remove unused items from the project
            var toDelete = projectFiles
                .Where(pair => !newFiles.Contains(pair.Key) && !(Path.GetFileNameWithoutExtension(pair.Key) + ".").StartsWith(defaultFile + "."));
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

        protected void AppendText(string text) { _text.Append(text); }
    }
}