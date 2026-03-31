namespace AIS.Services
    {
    public interface IStaticAssetVersionTokenProvider
        {
        string GetToken();
        void Invalidate();
        }
    }
