namespace CastleRescue.LLM
{
    /// <summary>
    /// Modelden gelen ham metni (string) bir C# nesnesine çeviren sınıfların ortak arayüzü.
    /// Generic (T) olduğu için her NPC kendi cevap sınıfı için ayrı bir parser yazabilir.
    /// (SOLID - Single Responsibility: JSON okuma işi NPC'nin değil, parser'ın sorumluluğu.)
    /// </summary>
    public interface IResponseParser<T>
    {
        /// <summary>
        /// Ham metni ayrıştırmaya çalışır. Exception fırlatmaz.
        /// </summary>
        /// <param name="raw">Modelin döndürdüğü metin.</param>
        /// <param name="result">Başarılıysa doldurulan nesne.</param>
        /// <param name="error">Başarısızsa nedenini açıklayan mesaj.</param>
        /// <returns>Ayrıştırma başarılıysa true.</returns>
        bool TryParse(string raw, out T result, out string error);
    }
}
