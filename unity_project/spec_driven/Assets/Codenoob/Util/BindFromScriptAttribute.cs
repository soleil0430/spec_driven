using UnityEngine;

namespace Codenoob.Util
{
    public class BindFromScriptAttribute : PropertyAttribute
    {
        public string Tooltip;

        public BindFromScriptAttribute()
        {
            Tooltip = "에디터 코드 혹은 에디터 GUI를 통한 바인딩이 반드시 필요합니다 \n" + 
                      "(Bind Serialized Field 버튼, Reset 등)";
        }
    }
}
