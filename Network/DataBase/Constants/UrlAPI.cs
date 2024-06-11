using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Network
{
    public static class UrlAPI
    {
        private const string API = "";
        public const string Ping = "https://google.com";

        private static string GetAPI()
        {
            return API;
        }

        public static string GetUrlAPIWithId(string urlAPI, string id) => $"{urlAPI}/{id}";

        public static string GetUrlAPIWithId(string urlAPI, int id) => GetUrlAPIWithId(urlAPI, id.ToString());

        public static string WithQuery(string url, params string[] additional) =>
            $"{url}?{string.Join("&", additional)}";
    }
}