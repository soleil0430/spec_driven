using UnityEngine;
using Codenoob.Util;

namespace Codenoob.SimpleUI
{
    [RequireComponent(typeof(Camera))]
    public class UICamera : BehaviourBase
    {
        //------------------------------------------------------------------------------
        // Static Variables
        //------------------------------------------------------------------------------
        static UICamera _instance;
        
        //------------------------------------------------------------------------------
        // Static Properties
        //------------------------------------------------------------------------------
        public static Camera Camera
        {
            get
            {
                if (!Application.isPlaying)
                    return FindOrCreate()._camera;

                if (_instance == null)
                    _instance = FindOrCreate();

                return _instance._camera;
            }
        }
        
        //------------------------------------------------------------------------------
        // Static Methods
        //------------------------------------------------------------------------------
        static UICamera FindOrCreate()
        { 
            var goCamera = GameObject.Find("UICamera");
            if (goCamera == null)
                goCamera = new GameObject("UICamera");

            var uiCamera = goCamera.GetComponent<UICamera>();
            if (uiCamera == null)
                uiCamera = goCamera.AddComponent<UICamera>();

            uiCamera.Init();
            return uiCamera;
        }

        //------------------------------------------------------------------------------
        // Serialized Variables
        //------------------------------------------------------------------------------
        [Header("# Components")]
        [SerializeField, BindFromScript] Camera _camera;

        //------------------------------------------------------------------------------
        // Methods
        //------------------------------------------------------------------------------
        void Init()
        {
            _camera = this.GetComponent<Camera>();

            _camera.clearFlags = CameraClearFlags.Depth;
            _camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            
            _camera.orthographic = false;
            _camera.orthographicSize = 10;

            _camera.nearClipPlane = 0.3f;
            _camera.farClipPlane = 200;

            _camera.depth = 1;
            _camera.targetDisplay = 0;

            _camera.useOcclusionCulling = false;

            transform.position = Vector3.one * 10000;
        }

        void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}