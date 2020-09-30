using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Helper
{
    sealed class Box<T>
    {
        public T Content;
        public Box(T content) { Content = content; }
        public Box() { Content = default(T); }
    }
}