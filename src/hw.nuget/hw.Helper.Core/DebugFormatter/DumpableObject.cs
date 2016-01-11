using System;
using System.Collections.Generic;
using System.Diagnostics;
using hw.Helper;
using System.Linq;

namespace hw.DebugFormatter
{
    [AdditionalNodeInfo("NodeDump")]
    [DebuggerDisplay("{NodeDump}")]
    public abstract class DumpableObject : Dumpable
    {
        static int _nextObjectId;
        readonly int? _objectId;

        protected DumpableObject()
            : this(_nextObjectId++) { }

        protected DumpableObject(int? nextObjectId) { _objectId = nextObjectId; }

        [DisableDump]
        internal int ObjectId
        {
            get
            {
                Tracer.Assert(_objectId != null);
                return _objectId.Value;
            }
        }

        [DisableDump]
        public string NodeDump
        {
            get
            {
                var result = GetNodeDump();
                if(_objectId == null)
                    return result;
                return result + "." + ObjectId + "i";
            }
        }

        protected virtual string GetNodeDump() { return GetType().PrettyName(); }

        internal string NodeDumpForDebug()
        {
            if(Debugger.IsAttached)
                return GetNodeDump();
            return "";
        }

        [DisableDump]
        internal bool IsStopByObjectIdActive { get; private set; }

        protected static string CallingMethodName
        {
            get
            {
                if(Debugger.IsAttached)
                    return Tracer.CallingMethodName(2);
                return "";
            }
        }

        protected override string Dump(bool isRecursion)
        {
            var result = NodeDump;
            if(!isRecursion)
                result += DumpData().Surround("{", "}");
            return result;
        }

        public override string ToString() { return base.ToString() + " ObjectId=" + ObjectId; }

        public override string DebuggerDump() { return base.DebuggerDump() + " ObjectId=" + ObjectId; }

        [DebuggerHidden]
        internal void StopByObjectIds(params int[] objectIds)
        {
            foreach(var objectId in objectIds)
                StopByObjectId(1, objectId);
        }

        [DebuggerHidden]
        void StopByObjectId(int stackFrameDepth, int objectId)
        {
            var isStopByObjectIdActive = IsStopByObjectIdActive;
            IsStopByObjectIdActive = true;
            if(ObjectId == objectId)
                Tracer.ConditionalBreak
                    ("", () => @"_objectId==" + objectId + "\n" + Dump(), stackFrameDepth + 1);
            IsStopByObjectIdActive = isStopByObjectIdActive;
        }
    }

    /// <summary>
    ///     Class attribute to define additional node info property, displayed after node title
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class AdditionalNodeInfoAttribute : Attribute
    {
        readonly string _property;

        /// <summary>
        ///     Initializes a new instance of the AdditionalNodeInfoAttribute class.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// created 07.02.2007 00:47
        public AdditionalNodeInfoAttribute(string property) { _property = property; }

        /// <summary>
        ///     Property to obtain additional node info
        /// </summary>
        public string Property { get { return _property; } }
    }
}