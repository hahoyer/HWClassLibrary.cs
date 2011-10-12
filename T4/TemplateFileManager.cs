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
using JetBrains.Annotations;

namespace HWClassLibrary.T4
{
    public abstract class TemplateFileManagerBase
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

        protected TemplateFileManagerBase(object textTransformation)
        {
            _textTransformation = DynamicTextTransformation.Create(textTransformation);
            _generationEnvironment = _textTransformation.GenerationEnvironment;
        }

        internal virtual string[] Process(bool split)
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

        protected abstract void CreateFile(string fileName, string content);

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
    }

    /// <summary>
    ///     Responsible for marking the various sections of the generation,
    ///     so they can be split up into separate files
    /// </summary>
    public sealed class TemplateFileManager : TemplateFileManagerBase
    {
        /// <summary>
        ///     Creates the VsTemplateFileManager if VS is detected, otherwise
        ///     creates the file system version.
        /// </summary>
        internal static TemplateFileManagerBase Create(DynamicTextTransformation transformation)
        {
            var host = transformation.Host;

#if !PREPROCESSED_TEMPLATE
            if(host.AsIServiceProvider() != null)
                return new VsTemplateFileManager(host.AsIServiceProvider(), host.TemplateFile);
#endif
            return new TemplateFileManager(transformation);
        }

        /// <summary>
        ///     Initializes an TemplateFileManager Instance  with the
        ///     TextTransformation (T4 generated class) that is currently running
        /// </summary>
        TemplateFileManager([NotNull] object textTransformation)
            : base(textTransformation) { }

        /// <summary>
        ///     Produce the template output files.
        /// </summary>
        protected override void CreateFile(string fileName, string content)
        {
            if(IsFileContentDifferent(fileName, content))
                File.WriteAllText(fileName, content);
        }

        static bool IsFileContentDifferent(String fileName, string newContent) { return !(File.Exists(fileName) && File.ReadAllText(fileName) == newContent); }

#if !PREPROCESSED_TEMPLATE
        sealed class VsTemplateFileManager : TemplateFileManagerBase
        {
            readonly ProjectItem _templateProjectItem;
            readonly DTE _dte;
            readonly Action<string> _checkOutAction;
            readonly Action<string[]> _projectSyncAction;

            /// <summary>
            ///     Creates an instance of the VsTemplateFileManager class with the IDynamicHost instance
            /// </summary>
            public VsTemplateFileManager(IServiceProvider hostServiceProvider, string templateFile)
                : base(null)
            {
                _dte = (DTE) hostServiceProvider.GetService(typeof(DTE));
                if(_dte == null)
                    throw new ArgumentNullException("hostServiceProvider");

                _templateProjectItem = _dte.Solution.FindProjectItem(templateFile);

                _checkOutAction = fileName => _dte.SourceControl.CheckOutItem(fileName);
                _projectSyncAction = keepFileNames => ProjectSync(_templateProjectItem, keepFileNames);
            }

            internal override string[] Process(bool split)
            {
                if(_templateProjectItem.ProjectItems == null)
                    return new string[0];

                var generatedFileNames = base.Process(split);

                _projectSyncAction.EndInvoke(_projectSyncAction.BeginInvoke(generatedFileNames, null, null));

                return generatedFileNames;
            }

            protected override void CreateFile(string fileName, string content)
            {
                if(IsFileContentDifferent(fileName, content))
                {
                    CheckoutFileIfRequired(fileName);
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

            void CheckoutFileIfRequired(string fileName)
            {
                if(_dte.SourceControl == null
                   || !_dte.SourceControl.IsItemUnderSCC(fileName)
                   || _dte.SourceControl.IsItemCheckedOut(fileName))
                    return;

                // run on worker thread to prevent T4 calling back into VS
                _checkOutAction.EndInvoke(_checkOutAction.BeginInvoke(fileName, null, null));
            }
        }
#endif
    }
}