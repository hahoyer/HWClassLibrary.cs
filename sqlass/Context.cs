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

using System.Data.Common;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.sqlass
{
    public class Context
    {
        public DbConnection Connection;
        List<IPendingChange> _pending;

        public void SaveChanges()
        {
            if(_pending == null)
                return;

            Connection.Open();
            try
            {
                var transaction = Connection.BeginTransaction();
                try
                {
                    foreach(var pendingChange in _pending)
                        pendingChange.Apply(Connection);
                    transaction.Commit();
                    _pending = null;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            finally
            {
                Connection.Close();
            }
        }

        internal void AddPendingChange(IPendingChange data)
        {
            if(_pending == null)
                _pending = new List<IPendingChange>();
            _pending.Add(data);
        }

        protected void UpdateDatabase(object container)
        {
            if (_pending != null)
                throw new UnsavedChangedException();
            
            
            var methods = container
                .GetType()
                .GetMembers()
                .ToArray();


        }
    }

    sealed class UnsavedChangedException : Exception
    {}
}