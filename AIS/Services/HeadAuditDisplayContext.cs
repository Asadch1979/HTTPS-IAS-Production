using AIS.Models;
using System.Security.Cryptography;
using System.Text;

namespace AIS.Services
{
    public static class HeadAuditDisplayContext
    {
        // Opaque consistency marker only; never grants authority or replaces server scope checks.
        public static string Key(SessionUser user) => System.Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{user.ID}|{user.SessionId}|{user.UserRoleID}|{user.UserEntityID}|{user.UserContextAssignmentId}")));
    }
}
