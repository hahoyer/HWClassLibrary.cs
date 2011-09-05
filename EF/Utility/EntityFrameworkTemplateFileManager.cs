using System.IO;
using System.Text;
using EnvDTE;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.EF.Utility
{
    /// <summary>
    ///     Responsible for marking the various sections of the generation,
    ///     so they can be split up into separate files
    /// </summary>
    public class EntityFrameworkTemplateFileManager
    {
        /// <summary>
        ///     Creates the VsEntityFrameworkTemplateFileManager if VS is detected, otherwise
        ///     creates the file system version.
        /// </summary>
        public static EntityFrameworkTemplateFileManager Create(object textTransformation)
        {
            var transformation = DynamicTextTransformation.Create(textTransformation);
            var host = transformation.Host;

#if !PREPROCESSED_TEMPLATE
            if(host.AsIServiceProvider() != null)
                return new VsEntityFrameworkTemplateFileManager(transformation);
#endif
            return new EntityFrameworkTemplateFileManager(transformation);
        }

        sealed class Block
        {
            public String Name;
            public int Start, Length;
        }

        readonly List<Block> files = new List<Block>();
        readonly Block footer = new Block();
        readonly Block header = new Block();
        readonly DynamicTextTransformation _textTransformation;

        // reference to the GenerationEnvironment StringBuilder on the
        // TextTransformation object
        readonly StringBuilder _generationEnvironment;

        Block currentBlock;

        /// <summary>
        ///     Initializes an EntityFrameworkTemplateFileManager Instance  with the
        ///     TextTransformation (T4 generated class) that is currently running
        /// </summary>
        EntityFrameworkTemplateFileManager(object textTransformation)
        {
            if(textTransformation == null)
                throw new ArgumentNullException("textTransformation");

            _textTransformation = DynamicTextTransformation.Create(textTransformation);
            _generationEnvironment = _textTransformation.GenerationEnvironment;
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

        public void StartFooter() { CurrentBlock = footer; }

        public void StartHeader() { CurrentBlock = header; }

        public void EndBlock()
        {
            if(CurrentBlock == null)
                return;

            CurrentBlock.Length = _generationEnvironment.Length - CurrentBlock.Start;

            if(CurrentBlock != header && CurrentBlock != footer)
                files.Add(CurrentBlock);

            currentBlock = null;
        }

        /// <summary>
        ///     Produce the template output files.
        /// </summary>
        public virtual IEnumerable<string> Process(bool split = true)
        {
            var generatedFileNames = new List<string>();

            if(split)
            {
                EndBlock();

                var headerText = _generationEnvironment.ToString(header.Start, header.Length);
                var footerText = _generationEnvironment.ToString(footer.Start, footer.Length);
                var outputPath = Path.GetDirectoryName(_textTransformation.Host.TemplateFile);

                files.Reverse();

                foreach(var block in files)
                {
                    var fileName = Path.Combine(outputPath, block.Name);
                    var content = headerText + _generationEnvironment.ToString(block.Start, block.Length) + footerText;

                    generatedFileNames.Add(fileName);
                    CreateFile(fileName, content);
                    _generationEnvironment.Remove(block.Start, block.Length);
                }
            }

            return generatedFileNames;
        }

        protected virtual void CreateFile(string fileName, string content)
        {
            if(IsFileContentDifferent(fileName, content))
                File.WriteAllText(fileName, content);
        }

        protected bool IsFileContentDifferent(String fileName, string newContent) { return !(File.Exists(fileName) && File.ReadAllText(fileName) == newContent); }

        Block CurrentBlock
        {
            get { return currentBlock; }
            set
            {
                if(CurrentBlock != null)
                    EndBlock();

                if(value != null)
                    value.Start = _generationEnvironment.Length;

                currentBlock = value;
            }
        }

#if !PREPROCESSED_TEMPLATE
        sealed class VsEntityFrameworkTemplateFileManager : EntityFrameworkTemplateFileManager
        {
            readonly ProjectItem templateProjectItem;
            readonly DTE dte;
            readonly Action<string> checkOutAction;
            readonly Action<IEnumerable<string>> projectSyncAction;

            /// <summary>
            ///     Creates an instance of the VsEntityFrameworkTemplateFileManager class with the IDynamicHost instance
            /// </summary>
            public VsEntityFrameworkTemplateFileManager(object textTemplating)
                : base(textTemplating)
            {
                var hostServiceProvider = _textTransformation.Host.AsIServiceProvider();
                if(hostServiceProvider == null)
                    throw new ArgumentNullException("Could not obtain hostServiceProvider");

                dte = (DTE) hostServiceProvider.GetService(typeof(DTE));
                if(dte == null)
                    throw new ArgumentNullException("Could not obtain DTE from host");

                templateProjectItem = dte.Solution.FindProjectItem(_textTransformation.Host.TemplateFile);

                checkOutAction = fileName => dte.SourceControl.CheckOutItem(fileName);
                projectSyncAction = keepFileNames => ProjectSync(templateProjectItem, keepFileNames);
            }

            public override IEnumerable<string> Process(bool split)
            {
                if(templateProjectItem.ProjectItems == null)
                    return new List<string>();

                var generatedFileNames = base.Process(split);

                projectSyncAction.EndInvoke(projectSyncAction.BeginInvoke(generatedFileNames, null, null));

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

            static void ProjectSync(ProjectItem templateProjectItem, IEnumerable<string> keepFileNames)
            {
                var keepFileNameSet = new HashSet<string>(keepFileNames);
                var projectFiles = new Dictionary<string, ProjectItem>();
                var originalOutput = Path.GetFileNameWithoutExtension(templateProjectItem.FileNames[0]);

                foreach(ProjectItem projectItem in templateProjectItem.ProjectItems)
                    projectFiles.Add(projectItem.FileNames[0], projectItem);

                // Remove unused items from the project
                foreach(var pair in projectFiles)
                    if(!keepFileNames.Contains(pair.Key)
                       && !(Path.GetFileNameWithoutExtension(pair.Key) + ".").StartsWith(originalOutput + "."))
                        pair.Value.Delete();

                // Add missing files to the project
                foreach(var fileName in keepFileNameSet)
                    if(!projectFiles.ContainsKey(fileName))
                        templateProjectItem.ProjectItems.AddFromFile(fileName);
            }

            void CheckoutFileIfRequired(string fileName)
            {
                if(dte.SourceControl == null
                   || !dte.SourceControl.IsItemUnderSCC(fileName)
                   || dte.SourceControl.IsItemCheckedOut(fileName))
                    return;

                // run on worker thread to prevent T4 calling back into VS
                checkOutAction.EndInvoke(checkOutAction.BeginInvoke(fileName, null, null));
            }
        }
#endif
    }
}