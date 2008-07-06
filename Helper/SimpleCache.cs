namespace HWClassLibrary.Helper
{
    public class SimpleCache<Value> where Value : class
    {
        private Value _value;

        public delegate Value CreateValue();
        public Value Find(CreateValue createValue)
        {
            if (_value == null)
                _value = createValue();
            return _value;
        }
    }
}