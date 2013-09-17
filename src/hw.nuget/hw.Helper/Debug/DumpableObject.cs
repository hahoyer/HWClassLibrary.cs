#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using System.Diagnostics;
using System.Linq;
using hw.Helper;
using hw.TreeStructure;

namespace hw.Debug
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
        internal string NodeDump
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
        internal void StopByObjectId(int objectId) { StopByObjectId(1, objectId); }

        [DebuggerHidden]
        internal void StopByObjectId(int stackFrameDepth, int objectId)
        {
            var isStopByObjectIdActive = IsStopByObjectIdActive;
            IsStopByObjectIdActive = true;
            if(ObjectId == objectId)
                Tracer.ConditionalBreak
                    ("", () => @"_objectId==" + objectId + "\n" + Dump(), stackFrameDepth + 1);
            IsStopByObjectIdActive = isStopByObjectIdActive;
        }
    }
}