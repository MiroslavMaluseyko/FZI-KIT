using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityBrowserBridge
{
    public class UnityBrowserBridge : MonoBehaviour
    {
        private UnityBrowserBridge _instance;
        
        public static event Action<bool> OnTabVisibilityChanged;
        
        [DllImport("__Internal")]
        public static extern void Alert(string param);
        
        [DllImport("__Internal")]
        public static extern void SetAspectRatio(int width, int height);

        private void Awake()
        {
            if(_instance != null && _instance != this)Destroy(gameObject);
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        
        private void TabVisibilityChanged(int isVisibleInt)
        {
            bool isVisible = isVisibleInt != 0;
            OnTabVisibilityChanged?.Invoke(isVisible);
        }
    }
}