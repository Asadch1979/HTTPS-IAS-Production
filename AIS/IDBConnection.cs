namespace AIS
{
    public interface IDBConnection
    {
        void CreateSession(string token, int ppno, string ip, string agent);
        void KillSessions(int ppno);
        bool IsSessionValid(string token);
        void InvalidateSession(string token);
    }
}
