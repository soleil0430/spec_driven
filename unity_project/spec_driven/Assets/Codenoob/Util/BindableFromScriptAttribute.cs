using UnityEngine;

namespace Codenoob.Util
{
    public class BindableFromScriptAttribute : PropertyAttribute
    {
        public string Tooltip;

        public BindableFromScriptAttribute()
        {
            Tooltip = "에디터 코드 혹은 에디터 GUI를 통해 바인딩 할 수 있습니다.\n" + 
                      "(Bind Serialized Field 버튼 등)";
        }
    }
}
