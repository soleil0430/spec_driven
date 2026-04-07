namespace Codenoob.Common
{
    public abstract class LazySingletonBase<T> where T : LazySingletonBase<T>, new()
    {
        static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();
                
                return _instance;
            }
        }

        protected LazySingletonBase()
        {
            _Init();
        }

        public void Release()
        {
            _Release();

            _instance = null;
        }

        protected abstract void _Init();

        protected abstract void _Release();
    }
}
