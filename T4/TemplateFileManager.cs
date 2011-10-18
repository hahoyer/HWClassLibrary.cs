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

namespace HWClassLibrary.T4
{
    sealed class TemplateFileManager
    {
        readonly DictionaryEx<string, Box<string>> _fileItems;

        // reference to the GenerationEnvironment StringBuilder on the
        // TextTransformation object
        readonly StringBuilder _text;

        readonly DynamicTextTransformation _textTransformation;

        internal TemplateFileManager(DynamicTextTransformation textTransformation)
        {
            _fileItems = new DictionaryEx<string, Box<string>>(fileName => new Box<string>(""));
            _textTransformation = textTransformation;
            _text = _textTransformation.GenerationEnvironment;
        }


        string[] _currentFiles;
        int _currentStart;

        public void Files(string[] files)
        {
            OnFilesChanging();
            Tracer.Assert(_currentFiles == null);
            // Special treatment of empty file list: send text to default target
            if(files.Length > 0)
                _currentFiles = files;
        }

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

        internal void Process()
        {
            OnFilesChanging();

            //Tracer.LaunchDebugger();
            var outputPath = Path.GetDirectoryName(_textTransformation.Host.TemplateFile) ?? "";
            foreach(var fileItem in _fileItems)
                CreateFile(Path.Combine(outputPath, fileItem.Key), fileItem.Value.Content);

            var newFiles = _fileItems.Keys.Select(name => Path.Combine(outputPath, name)).ToArray();

            Action projectSyncAction = () => ProjectSync(newFiles);
            projectSyncAction.EndInvoke(projectSyncAction.BeginInvoke(null, null));
        }

        void CreateFile(string fileName, string content)
        {
            if(Extender.IsFileContentDifferent(fileName, content))
            {
                _textTransformation.CheckoutFileIfRequired(fileName);
                File.WriteAllText(fileName, content);
            }
        }

        void ProjectSync(string[] newFiles)
        {
            var item = _textTransformation.TemplateProjectItem();
            if (item == null || item.ProjectItems == null)
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
    }

}