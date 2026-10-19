namespace CastleRescue.LLM
{
    /// <summary>
    /// LLM isteğinde olabilecek hata türleri.
    /// Her biri için oyuncuya farklı, anlamlı bir mesaj göstereceğiz.
    /// </summary>
    public enum LLMErrorType
    {
        None,
        ServerUnreachable, // Ollama kapalı / adres yanlış
        Timeout,           // Model süre içinde cevap vermedi
        ModelNotFound,     // Model indirilmemiş (ollama pull yapılmamış)
        HttpError,         // Sunucu başka bir hata döndürdü
        InvalidResponse,   // Cevap geldi ama okunamadı / boş
        Busy               // Önceki istek hâlâ bitmedi
    }

    /// <summary>
    /// Bir LLM isteğinin sonucu. Ya başarılıdır (Content dolu) ya da hatalıdır (ErrorType dolu).
    /// Hata durumunda exception fırlatmak yerine bu nesneyi döndürüyoruz;
    /// böylece oyun hiçbir koşulda çökmez, çağıran taraf sadece IsSuccess'e bakar.
    /// </summary>
    public class LLMResult
    {
        public bool IsSuccess { get; }
        public string Content { get; }
        public LLMErrorType ErrorType { get; }
        public string ErrorMessage { get; }

        /// <summary>İsteğin kaç saniye sürdüğü. Rapordaki gecikme (latency) ölçümü için.</summary>
        public float DurationSeconds { get; }

        // Kurucu private: nesne sadece aşağıdaki iki metotla oluşturulabilir.
        private LLMResult(bool isSuccess, string content, LLMErrorType errorType, string errorMessage, float duration)
        {
            IsSuccess = isSuccess;
            Content = content;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
            DurationSeconds = duration;
        }

        public static LLMResult Success(string content, float duration)
        {
            return new LLMResult(true, content, LLMErrorType.None, null, duration);
        }

        public static LLMResult Failure(LLMErrorType errorType, string errorMessage, float duration)
        {
            return new LLMResult(false, null, errorType, errorMessage, duration);
        }
    }
}
