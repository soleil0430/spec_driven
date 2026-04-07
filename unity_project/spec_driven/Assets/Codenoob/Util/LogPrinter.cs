using System.Collections.Generic;
using UnityEngine;

namespace Codenoob.Util
{
    public class LogClassPrinter
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Dictionary<string, FuncLogPrinter> _funcPrinterSet = new Dictionary<string, FuncLogPrinter>();

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public const string SupercentSign = "Supercent";

        public string ClassName { get; private set; } = string.Empty;
        public string ColorHex  { get; private set; } = string.Empty;

        public bool Enable { get; set; } = true;
        
        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public LogClassPrinter(string className, string colorHex)
        {
            ClassName = className;
            ColorHex  = colorHex[0] == '#' ? colorHex : "#" + colorHex;                        
        }

        public void Log(string funcName, string log)
        {
            if (false == Enable)
                return;

#if UNITY_EDITOR
            Debug.Log($"<color={ColorHex}>[{SupercentSign}.{ClassName}.{funcName}]</color> {log}");
#else
            Debug.Log($"[{SupercentSign}.{ClassName}.{funcName}] {log}");
#endif
        }

        public void Warning(string funcName, string log)
        {
            if (false == Enable)
                return;

#if UNITY_EDITOR
            Debug.LogWarning($"<color={ColorHex}>[{SupercentSign}.{ClassName}.{funcName}]</color> {log}");
#else
            Debug.LogWarning($"[{SupercentSign}.{ClassName}.{funcName}] {log}");
#endif
        }

        public void Error(string funcName, string log)
        {
            if (false == Enable)
                return;

#if UNITY_EDITOR
            Debug.LogError($"<color={ColorHex}>[{SupercentSign}.{ClassName}.{funcName}]</color> {log}");
#else
            Debug.LogError($"[{SupercentSign}.{ClassName}.{funcName}] {log}");
#endif
        }

        public FuncLogPrinter GetFuncPrinter(string funcName)
        {
            if (false == _funcPrinterSet.TryGetValue(funcName, out var printer))
            {
                printer = new FuncLogPrinter(this, funcName);
                _funcPrinterSet.Add(funcName, printer);
            }

            return printer;
        }
    }

    public class FuncLogPrinter
    {
        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private LogClassPrinter _parent = null;

        //------------------------------------------------------------------------------
        // get, set
        //------------------------------------------------------------------------------
        public string FuncName { get; private set; } = string.Empty;

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public FuncLogPrinter(LogClassPrinter parent, string funcName = null)
        {
            _parent  = parent;
            FuncName = funcName;
        }

        public void Begin(string log = "")
        {
            Log("BEGIN", log);
        }

        public void End(string log = "")
        {
            Log("END", log);
        }

        public void Stop(string log = "")
        {
            Log("STOP", log);
        }

        public void Proc(string log = "")
        {
            Log("PROC", log);
        }

        public void Call(string callFuncName, string log = "")
        {
            Log($"CALL | {callFuncName}", log);
        }

        public void Callback(string callFuncName, string log = "")
        {
            Log($"CALLBACK | {callFuncName}", log);
        }

        public void Log(string log)
        {
            _parent?.Log(FuncName, log);
        }

        public void Warning(string log)
        {
            _parent?.Warning(FuncName, log);
        }

        public void Error(string log)
        {
            _parent?.Error(FuncName, log);
        }

        private void Log(string type, string log = "")
        {
            if (string.IsNullOrEmpty(log))
                _parent?.Log(FuncName, $"{type}");
            else
                _parent?.Log(FuncName, $"{type} | {log}");
        }
    }
}