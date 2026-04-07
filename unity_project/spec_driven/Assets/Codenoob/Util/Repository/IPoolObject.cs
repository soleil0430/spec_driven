namespace Codenoob.Util
{
    public interface IPoolObject
    {
        IPoolBase OwnPool { set; get; }
        void OnGenerate();
        void OnGet();
        void OnReturn();
        void OnTerminate();
    }
}