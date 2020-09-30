using JetBrains.Annotations;

namespace hw.Helper
{
    sealed class Box<TTarget>
    {
        [PublicAPI]
        public TTarget Content;
        public Box(TTarget content) => Content = content;
        public Box() => Content = default(TTarget);
    }
}