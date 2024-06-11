using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public static class DataBase
    {
        public static string Role;
        public static int UserId;

        private const float TimeoutTime = 10f;

        private enum RequestType
        {
            Get,
            Post,
        }

        private static string s_token = null;

        public static string Token
        {
            get => s_token;
            set => s_token = $"Bearer {value}";
        }

        #region Get

        public static void SendGet(string url, Action<UnityWebRequest> requestHandler = null,
            Action<UnityWebRequest> errorHandler = null, bool needToLogError = true, bool checkToken = true) =>
            Get(url, requestHandler, errorHandler, needToLogError, checkToken);

        public static async UniTask SendGetTask(
            string url, Action<UnityWebRequest> requestHandler = null, Action<UnityWebRequest> errorHandler = null,
            bool needToLogError = true, bool tryUntilSuccess = false, bool checkToken = true) =>
            await Send(RequestType.Get, url, requestHandler, errorHandler, 0, needToLogError, tryUntilSuccess,
                checkToken);

        private static async UniTask Get(string url, Action<UnityWebRequest> requestHandler = null,
            Action<UnityWebRequest> errorHandler = null, bool needToLogError = true, bool checkToken = true)
        {
            var www = new UnityWebRequest(url);
            www.downloadHandler = new DownloadHandlerBuffer();
#if UNITY_SERVER || DEVELOP
            Debug.Log(url);
#endif
            if (checkToken) CheckToken(www);

            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
#if DEVELOP
                Debug.Log($"Connection lost while trying to send request to {url}");
#endif
                while (!await APIServerReachableBool()) await UniTask.Delay(1000);
#if DEVELOP
                Debug.Log($"Connection restored, trying to send request to {url}");
#endif
                await Get(url, requestHandler, errorHandler, needToLogError, checkToken);
                return;
            }

            ProcessResponse(www, requestHandler, errorHandler, needToLogError);
        }

        #endregion

        #region Post

        public static void SendPost<TData>(TData data, string url,
            Action<UnityWebRequest> requestHandler = null, Action<UnityWebRequest> errorHandler = null,
            bool needToLogError = true, bool checkToken = true) =>
            Post(data, url, requestHandler, errorHandler, needToLogError, checkToken);

        public static async UniTask SendPostTask<TData>(TData data, string url,
            Action<UnityWebRequest> requestHandler = null,
            Action<UnityWebRequest> errorHandler = null, bool needToLogError = true, bool tryUntilSuccess = false,
            bool checkToken = true) =>
            await Send(RequestType.Post, url, requestHandler, errorHandler, data, needToLogError, tryUntilSuccess,
                checkToken);

        private static async UniTask Post<TData>(TData data, string url,
            Action<UnityWebRequest> requestHandler = null, Action<UnityWebRequest> errorHandler = null,
            bool needToLogError = true, bool checkToken = true)
        {
            var form = new WWWForm();
            var json = JsonConvert.SerializeObject(data);
#if UNITY_SERVER || DEVELOP
            Debug.Log($"Sent data: {json}, to {url}");
#endif
            var www = UnityWebRequest.Post(url, form);

            var bytes = Encoding.UTF8.GetBytes(json);

            www.uploadHandler = new UploadHandlerRaw(bytes);
            www.SetRequestHeader(RequestHeaders.ContentType, RequestHeaders.ApplicationJson);

            if (checkToken) CheckToken(www);

            await www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                while (!await APIServerReachableBool()) await UniTask.Delay(1000);
                await Post(data, url, requestHandler, errorHandler, needToLogError, checkToken);
                return;
            }

            ProcessResponse(www, requestHandler, errorHandler, needToLogError);
        }

        #endregion


        #region Common

        private static async UniTask Send<TData>(RequestType type, string url, Action<UnityWebRequest> requestHandler,
            Action<UnityWebRequest> errorHandler, TData data, bool needToLogError = true,
            bool tryUntilSuccess = false, bool checkToken = true)
        {
            var success = false;
            var loading = true;
            // var startTime = Time.time;

            do
            {
                loading = true;
                switch (type)
                {
                    case RequestType.Get:
                        SendGet(url, OnSuccess, OnFail, needToLogError, checkToken);
                        break;
                    case RequestType.Post:
                        SendPost(data, url, OnSuccess, OnFail, needToLogError, checkToken);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                var timeout = false;
                while (loading && !timeout)
                {
                    // timeout = Timeout();
                    await UniTask.Yield();
                }

                if (timeout)
                {
                    errorHandler?.Invoke(new UnityWebRequest { url = url });
                }
            } while (tryUntilSuccess && !success);

            // bool Timeout() => Time.time - startTime > TimeoutTime;

            void OnSuccess(UnityWebRequest www)
            {
                requestHandler?.Invoke(www);
                loading = false;
                success = true;
            }

            void OnFail(UnityWebRequest www)
            {
                errorHandler?.Invoke(www);
                loading = false;
            }
        }

        private static void ProcessResponse(UnityWebRequest www, Action<UnityWebRequest> requestHandler,
            Action<UnityWebRequest> errorHandler = null, bool needToLogError = true)
        {
            if (www.result == UnityWebRequest.Result.Success)
            {
                requestHandler?.Invoke(www);
#if UNITY_SERVER || DEVELOP
                Debug.Log($"Request: {www.url} " +
                          $"{www.result} " +
                          $"{www.responseCode} " +
                          $"{www.downloadHandler.text} ");
#endif
                return;
            }

            errorHandler?.Invoke(www);
            if (needToLogError)
            {
#if UNITY_SERVER || DEVELOP
                Debug.LogError($"Error Request: {www.error} " +
                               $"{www.result} " +
                               $"{www.responseCode} " +
                               $"{www.downloadHandler.text} " +
                               $"{www.url} ");
#endif
            }
        }

        private static void CheckToken(UnityWebRequest www)
        {
#if SERVER
            //If you have server - insert token here
            //Token =
#endif

            if (string.IsNullOrEmpty(Token))
            {
                Debug.LogError("Authorization Token Not Set! 401: Unauthorized");
                return;
            }

            www.SetRequestHeader(RequestHeaders.Authorization, Token);
        }

        public static async UniTask<UnityWebRequest> APIServerReachable(bool log = true)
        {
#if DEVELOP
            if (log) Debug.Log("Check network connection");
#endif
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
#if DEVELOP
                if (log) Debug.Log("Application.internetReachability == NetworkReachability.NotReachable");
#endif
                return null;
            }

            var url = UrlAPI.Ping;
#if DEVELOP
            if (log) Debug.Log($"try {url}");
#endif
            var www = UnityWebRequest.Get($"{url}");
            www.timeout = 1;
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
#if DEVELOP
                if (log)
                {
                    Debug.Log($"www.SendWebRequest {url} failed");
                    Debug.Log(www.error);
                }
#endif
                return www;
            }

#if DEVELOP
            if (log) Debug.Log($"www.SendWebRequest {url} success");
#endif
            return www;
        }

        private static async UniTask<bool> APIServerReachableBool()
        {
            var www = await APIServerReachable();
            var result = www is { result: UnityWebRequest.Result.Success };
            www?.Dispose();
            return result;
        }

        #endregion
    }
}

public static class UnityWebRequestExtension
{
    public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
    {
        TaskCompletionSource<UnityWebRequest.Result> tsc = new();
        reqOp.completed += asyncOp => tsc.TrySetResult(reqOp.webRequest.result);

        if (reqOp.isDone)
            tsc.TrySetResult(reqOp.webRequest.result);

        return tsc.Task.GetAwaiter();
    }
}