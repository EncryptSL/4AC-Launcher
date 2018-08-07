using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Reflection;

namespace MCAC_Launcher
{
    class HttpApi
    {
        private static readonly WebClient client = new WebClient();
        public static readonly string Launcher = "AAL_Windows_Launcher.exe";
        private static readonly string LauncherHash = "AAL_Windows_Launcher.hash";

        public static bool IsValidSession(string session)
        {
            try
            {
                var _client = new WebClient();
                _client.Headers.Add(HttpRequestHeader.Authorization, session);
                return _client.DownloadString("https://alphaantileak.net/api/v3/users/login/valid") == "{}";
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static void AddApp(string session)
        {
            try
            {
                var _client = new WebClient();
                _client.Headers.Add(HttpRequestHeader.Authorization, session);
                _client.UploadString("https://alphaantileak.net/api/v3/apps/f1b31908-fbc7-4b65-b781-3cbb2f2f189d/add", "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static string CreateAccount()
        {
            var _client = new WebClient();
            return _client.DownloadString("https://mcac.alphaantileak.net/api/v1/account");
        }

        public static bool IsLauncherUpToDate()
        {
            if (!File.Exists(Launcher)) return false;

            var sha256 = new SHA256Managed();
            var fileData = File.ReadAllBytes(Launcher);

            var fileHash = sha256.ComputeHash(fileData);
            var remoteHash = client.DownloadData("http://cdn.alphaantileak.net/AAL/" + LauncherHash);

            return fileHash.SequenceEqual(remoteHash);
        }

        public static bool IsThisUpToDate()
        {
            var sha256 = new SHA256Managed();
            var fileData = File.ReadAllBytes(Assembly.GetExecutingAssembly().Location);

            var fileHash = sha256.ComputeHash(fileData);
            var remoteHash = client.DownloadData("https://mcac.alphaantileak.net/dl/Launcher.hash");

            return fileHash.SequenceEqual(remoteHash);
        }

        public static void DownloadLauncher()
        {
            client.DownloadFile("http://cdn.alphaantileak.net/AAL/" + Launcher, Launcher);
        }
    }
}
