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
using System.Linq;
using System.Windows.Forms;

namespace hw.Forms
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SmartNodeAttribute : Attribute
    {
        public static TreeNode Process(TreeNode treeNode)
        {
            treeNode.CreateNodeList();
            switch(treeNode.Nodes.Count)
            {
                case 0:
                    return null;
                case 1:
                    var node = treeNode.Nodes[0];
                    node.Text = treeNode.Text + " \\ " + node.Text;
                    return Process(node);
            }
            return treeNode;
        }
    }
}