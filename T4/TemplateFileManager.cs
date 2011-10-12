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

namespace HWClassLibrary.T4
{
    sealed class TemplateFileManager
    {
        sealed class Block
        {
            public String Name;
            public int Start, Length;
        }

        // reference to the GenerationEnvironment StringBuilder on the
        // TextTransformation object
        readonly StringBuilder _generationEnvironment;

        Block _currentBlock;
        readonly List<Block> _files = new List<Block>();
        readonly Block _footer = new Block();
        readonly Block _header = new Block();
        readonly DynamicTextTransformation _textTransformation;

        internal TemplateFileManager(DynamicTextTransformation textTransformation)
        {
            _textTransformation = textTransformation;
            _generationEnvironment = _textTransformation.GenerationEnvironment;
        }

        internal string[] ProcessBase(bool split)
        {
            var generatedFileNames = new List<string>();

            if(split)
            {
                EndBlock();

                var headerText = _generationEnvironment.ToString(_header.Start, _header.Length);
                var footerText = _generationEnvironment.ToString(_footer.Start, _footer.Length);
                var outputPath = Path.GetDirectoryName(_textTransformation.Host.TemplateFile);

                _files.Reverse();

                foreach(var block in _files)
                {
                    var fileName = Path.Combine(outputPath, block.Name);
                    var content = headerText + _generationEnvironment.ToString(block.Start, block.Length) + footerText;

                    generatedFileNames.Add(fileName);
                    CreateFile(fileName, content);
                    _generationEnvironment.Remove(block.Start, block.Length);
                }
            }

            return generatedFileNames.ToArray();
        }

        /// <summary>
        ///     Marks the end of the last file if there was one, and starts a new
        ///     and marks this point in generation as a new file.
        /// </summary>
        public void StartNewFile(string name)
        {
            if(name == null)
                throw new ArgumentNullException("name");

            CurrentBlock = new Block {Name = name};
        }

        Block CurrentBlock
        {
            get { return _currentBlock; }
            set
            {
                if(CurrentBlock != null)
                    EndBlock();

                if(value != null)
                    value.Start = _generationEnvironment.Length;

                _currentBlock = value;
            }
        }

        public void StartFooter() { CurrentBlock = _footer; }

        public void StartHeader() { CurrentBlock = _header; }

        public void EndBlock()
        {
            if(CurrentBlock == null)
                return;

            CurrentBlock.Length = _generationEnvironment.Length - CurrentBlock.Start;

            if(CurrentBlock != _header && CurrentBlock != _footer)
                _files.Add(CurrentBlock);

            _currentBlock = null;
        }

        internal string[] Process(bool split)
        {
            var templateProjectItem = _textTransformation.TemplateProjectItem();
            if(templateProjectItem != null && templateProjectItem.ProjectItems == null)
                return new string[0];

            var generatedFileNames = ProcessBase(split);

            if(templateProjectItem != null )
            {
                Action<string[]> projectSyncAction = keepFileNames => ProjectSync(templateProjectItem, keepFileNames);
                projectSyncAction.EndInvoke(projectSyncAction.BeginInvoke(generatedFileNames, null, null));
            }
            return generatedFileNames;
        }

        internal void CreateFile(string fileName, string content)
        {
            if(Extender.IsFileContentDifferent(fileName, content))
            {
                _textTransformation.CheckoutFileIfRequired(fileName);
                File.WriteAllText(fileName, content);
            }
        }

        static void ProjectSync(ProjectItem templateProjectItem, string[] keepFileNames)
        {
            var keepFileNameSet = new HashSet<string>(keepFileNames);
            var originalOutput = Path.GetFileNameWithoutExtension(templateProjectItem.FileNames[0]);

            var projectFiles = templateProjectItem
                .ProjectItems
                .Cast<ProjectItem>()
                .ToDictionary(projectItem => projectItem.FileNames[0]);

            // Remove unused items from the project
            var pairs = projectFiles
                .Where(pair => !keepFileNames.Contains(pair.Key) && !(Path.GetFileNameWithoutExtension(pair.Key) + ".").StartsWith(originalOutput + "."));
            foreach(var pair in pairs)
                pair.Value.Delete();

            // Add missing files to the project
            foreach(var fileName in keepFileNameSet.Where(fileName => !projectFiles.ContainsKey(fileName)))
                templateProjectItem.ProjectItems.AddFromFile(fileName);
        }
    }
}