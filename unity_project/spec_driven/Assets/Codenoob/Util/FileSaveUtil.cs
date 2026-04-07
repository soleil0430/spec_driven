using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Codenoob.Util
{
    public static class ExtensionMethod
    {
        public static string EncodeToBase64(this string txt)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(txt));
        }

        public static string DecodeFromBase64(this string base64)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(base64));
        }
    }
}

namespace Codenoob.Util
{
    public static class FileSaveUtil
    {
    #if UNITY_EDITOR
        public const bool DEFAULT_ENCODE_OPTION = false;
    #else
        public const bool DEFAULT_ENCODE_OPTION = true;
    #endif

        /// <summary>
        /// 제거 (string)
        /// </summary>
        public static void Delete(string fileName, bool useEncodeFileName = DEFAULT_ENCODE_OPTION)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[Supercent.Core.FileSaveUtil.Delete] 파일명이 비어있습니다.");
                return;
            }

            var path = GetFullPath(fileName, useEncodeFileName);

            try                 { File.Delete(path); }
            catch (Exception e) { Debug.LogError($"[FileSaveUtil - Delete] {e.Message}\n\n{e.StackTrace}"); }
        }

        /// <summary>
        /// 저장 (string)
        /// </summary>
        public static bool Save(string fileName, string data, bool useEncodeFileName = DEFAULT_ENCODE_OPTION, bool useEncodeData = DEFAULT_ENCODE_OPTION)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[Supercent.Core.FileSaveUtil.Save] 파일명이 비어있습니다.");
                return false;
            }

            if (string.IsNullOrEmpty(data))
            {
                Debug.LogWarning("[Supercent.Core.FileSaveUtil.Save] 저장할 정보가 비어있습니다.");
                data = string.Empty;
            }

            var path = GetFullPath(fileName, useEncodeFileName);

            try 
            { 
                File.WriteAllText(path, useEncodeData ? data.EncodeToBase64() : data); 
                return true;
            }
            catch (Exception e) 
            { 
                Debug.LogError($"[FileSaveUtil - Save] {e.Message}\n\n{e.StackTrace}"); 
                return false;
            }
        }

        /// <summary>
        /// 저장 (byte)
        /// </summary>
        public static bool SaveBytes(string fileName, byte[] data, bool useEncodeFileName = DEFAULT_ENCODE_OPTION)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[Supercent.Core.FileSaveUtil.Save] 파일명이 비어있습니다.");
                return false;
            }

            if (null == data)
            {
                Debug.LogWarning("[Supercent.Core.FileSaveUtil.Save] 저장할 정보가 비어있습니다.");
                return false;
            }

            var path = GetFullPath(fileName, useEncodeFileName);

            try                 
            { 
                File.WriteAllBytes(path, data); 
                return true; 
            }
            catch (Exception e) 
            { 
                Debug.LogError($"[FileSaveUtil - Save] {e.Message}\n\n{e.StackTrace}"); 
                return false; 
            }
        }

        /// <summary>
        /// 파일 존재하는지 확인
        /// </summary>
        public static bool Exists(string fileName, bool useEncodeFileName = DEFAULT_ENCODE_OPTION)
        {
            if (true == string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[Supercent.Core.FileSaveUtil.Exists] 파일명이 비어있습니다.");
                return false;
            }

            var path = GetFullPath(fileName, useEncodeFileName);
            return File.Exists(path);
        }

        /// <summary>
        /// 불러오기 (string)
        /// </summary>
        public static string Load(string fileName, bool useEncodeFileName = DEFAULT_ENCODE_OPTION, bool useEncodeData = DEFAULT_ENCODE_OPTION)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[Supercent.Core.FileSaveUtil.Load] 파일명이 비어있습니다.");
                return string.Empty;
            }

            var path = GetFullPath(fileName, useEncodeFileName);
            if (!File.Exists(path))
                return string.Empty;

            try
            {
                var token = File.ReadAllText(path);
                if (useEncodeData)
                    token = token.DecodeFromBase64();

                return token;
            }
            catch (Exception e) 
            { 
                Debug.LogError($"[FileSaveUtil - Load] {e.Message}\n\n{e.StackTrace}"); 
            }

            return string.Empty;
        }

        /// <summary>
        /// 불러오기 (byte)
        /// </summary>
        public static byte[] LoadBytes(string fileName, bool useEncodeFileName = DEFAULT_ENCODE_OPTION)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[Supercent.Core.FileSaveUtil.Load] 파일명이 비어있습니다.");
                return null;
            }

            var path = GetFullPath(fileName, useEncodeFileName);
            if (!File.Exists(path))
                return null;

            try                 { return File.ReadAllBytes(path); }
            catch (Exception e) { Debug.LogError($"[FileSaveUtil - Load] {e.Message}\n\n{e.StackTrace}"); }

            return null;
        }

        /// <summary>
        /// 실제 경로 가져오기
        /// </summary>
        private static string GetFullPath(string fileName, bool useEncodeFileName)
        {
            return $"{Application.persistentDataPath}/{(useEncodeFileName ? fileName.EncodeToBase64() : fileName)}";
        }
    }
}