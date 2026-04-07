using UnityEngine;

namespace Codenoob.SimpleUI.TEST.JOYPAD
{
    public class TEST : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] Transform _target;
        [SerializeField] float _moveSpeed;

        [SerializeField] JoypadView _joypad;

        void Start() 
        {
            _joypad.Init();
            _joypad.Activate();

            _joypad.OnBegin += OnBeginJoypad;
            _joypad.OnDrag += OnDragJoypad;
            _joypad.OnFinish += OnFinishJoypad;
        }


        void OnBeginJoypad() 
        { 
            _target.localScale = Vector3.one * 1.2f;
        }
        void OnDragJoypad(float radian, float power) 
        { 
            //radius와 power를 사용하여 2차원 벡터 계산
            var direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

            var moveSpeed = _moveSpeed * power;

            _target.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
        void OnFinishJoypad()
        {
            _target.localScale = Vector3.one;
        }

        public void OnClick_Reset()
        {
            _target.position = Vector3.zero;
        }



#endif //UNITY_EDITOR
    }
}