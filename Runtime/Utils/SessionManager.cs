using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public static class SessionManager
    {
        private const string SessionFileExtension = ".session.json";

        private static string GetSessionFilePath(string environmentKey)
        {
            return Path.Combine(Application.persistentDataPath, environmentKey.ToLower() + SessionFileExtension);
        }

        public static SessionData LoadSession(string environmentKey)
        {
            string filePath = GetSessionFilePath(environmentKey);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<SessionData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load session from file: {e.Message}");
                    return null;
                }
            }

            return null;
        }

        public static SessionData MigrateFromPlayerPrefs(string environmentKey)
        {
            SessionData session = new SessionData();
            bool hasData = false;

            string authTokenKey = $"{environmentKey}.apiKey";
            if (PlayerPrefs.HasKey(authTokenKey))
            {
                session.AuthToken = PlayerPrefs.GetString(authTokenKey);
                hasData = true;
            }
            else if (PlayerPrefs.HasKey("apiKey"))
            {
                session.AuthToken = PlayerPrefs.GetString("apiKey");
                hasData = true;
            }

            string userKey = $"{environmentKey}.user";
            if (PlayerPrefs.HasKey(userKey))
            {
                try
                {
                    session.User = JsonConvert.DeserializeObject<UserData>(PlayerPrefs.GetString(userKey));
                    hasData = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to deserialize user data from PlayerPrefs: {e.Message}");
                }
            }
            else if (PlayerPrefs.HasKey("userid"))
            {
                try
                {
                    UserData user = new UserData
                    {
                        UserId = int.Parse(PlayerPrefs.GetString("userid")),
                        DisplayName = PlayerPrefs.GetString("displayname"),
                        UserName = PlayerPrefs.GetString("username")
                    };

                    string roles = PlayerPrefs.GetString("userroles");
                    if (!string.IsNullOrEmpty(roles))
                    {
                        user.Roles = new System.Collections.Generic.List<string>(roles.Split(','));
                    }

                    string permissions = PlayerPrefs.GetString("userpermissions");
                    if (!string.IsNullOrEmpty(permissions))
                    {
                        user.Permissions = new System.Collections.Generic.List<string>(permissions.Split(','));
                    }

                    session.User = user;
                    hasData = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to migrate legacy user data from PlayerPrefs: {e.Message}");
                }
            }

            if (PlayerPrefs.HasKey("userCode"))
            {
                session.UserCode = PlayerPrefs.GetString("userCode");
                hasData = true;
            }

            if (PlayerPrefs.HasKey("OfflineState"))
            {
                session.OfflineState = PlayerPrefs.GetString("OfflineState");
                hasData = true;
            }

            if (PlayerPrefs.HasKey("defaultSkybox"))
            {
                session.DefaultSkybox = PlayerPrefs.GetString("defaultSkybox");
                hasData = true;
            }

            return hasData ? session : null;
        }

        public static SessionData LoadOrMigrateSession(string environmentKey)
        {
            SessionData session = LoadSession(environmentKey);

            if (session == null)
            {
                session = MigrateFromPlayerPrefs(environmentKey);
                if (session != null)
                {
                    Debug.Log($"Migrated session data from PlayerPrefs for environment: {environmentKey}");
                    SaveSession(environmentKey, session);
                }
                else
                {
                    session = new SessionData();
                }
            }

            return session;
        }

        public static void SaveSession(string environmentKey, SessionData session)
        {
            if (session == null)
            {
                Debug.LogWarning("Attempted to save null session");
                return;
            }

            try
            {
                string filePath = GetSessionFilePath(environmentKey);
                string json = JsonConvert.SerializeObject(session, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save session to file: {e.Message}");
            }
        }

        public static void DeleteSession(string environmentKey)
        {
            try
            {
                string filePath = GetSessionFilePath(environmentKey);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted session file for environment: {environmentKey}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete session file: {e.Message}");
            }
        }

        public static bool HasSession(string environmentKey)
        {
            string filePath = GetSessionFilePath(environmentKey);
            return File.Exists(filePath);
        }
    }
}
