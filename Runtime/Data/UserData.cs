using System.Collections.Generic;

namespace AccessVR.OrchestrateVR.SDK
{

    public static class Role
    {
        public static readonly string Admin = "Admin";
        public static readonly string Author = "Author";
        public static readonly string Learner = "Learner";
    }

    public class UserData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public bool IsAnonymous { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Permissions { get; set; }

        public bool HasRole(string role)
        {
            return Roles.Contains(role);
        }
    }

}