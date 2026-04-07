namespace Codenoob.Util
{
    public interface IPoolBase
    {
        bool Contains(object item);
        void Return(object item);
        void Terminate(object item);
    }
}