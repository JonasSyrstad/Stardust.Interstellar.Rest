namespace Stardust.Interstellar
{
    public interface IOAuthTokenProvider
    {
        string GetBearerToken();

        string GetBearerToken(string resourceUrl);
    }
}