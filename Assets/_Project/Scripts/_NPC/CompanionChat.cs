using System;
using System.Collections.Generic;
using CastleRescue.Core;
using CastleRescue.LLM;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CastleRescue.NPC
{
    /// <summary>
    /// Fareyle serbest sohbet. Gardiyan gibi LLM ile cevap verir ama oyun durumunu değiştirmez;
    /// bulunulan bölüme göre oyuncuya ipucu verir.
    ///
    /// Hangi bölümde olunduğunu sahnenin adından anlar ve modele o bölümün bilgisini gönderir.
    /// Gardiyanla aynı konuşma ekranını kullanır, çünkü ikisi de IConversationPartner'ı uygular.
    /// </summary>
    public class CompanionChat : MonoBehaviour, IConversationPartner
    {
        /// <summary>Bir bölümün adı ve o bölümde farenin bilmesi gerekenler.</summary>
        [Serializable]
        public class LevelContext
        {
            public string sceneName;
            [TextArea(3, 8)] public string context;
        }

        [Header("LLM Bağlantısı")]
        [Tooltip("ILLMClient uygulayan bileşen (OllamaClient). Boş bırakılırsa aynı objede aranır.")]
        [SerializeField] private MonoBehaviour llmClientComponent;

        [Header("Kişilik")]
        [SerializeField] private string companionName = "Fındık";

        [SerializeField, TextArea(4, 12)]
        private string persona =
            "Sen Fındık adında küçük, cesur ve biraz geveze bir faresin. Oyuncunun en yakın dostusun ve " +
            "onunla birlikte Kuzgun Kalesi'ne, hapsedilmiş prensesi kurtarmaya geldin. Kalenin her köşesini " +
            "bilirsin çünkü yıllardır duvarların içindeki deliklerde yaşıyorsun. Konuşurken arada 'Cik!' dersin.";

        [SerializeField, TextArea(2, 4)]
        private string openingLine = "Cik! Ne oldu dostum, bir şey mi soracaksın?";

        [SerializeField]
        private List<LevelContext> levelContexts = new List<LevelContext>
        {
            new LevelContext
            {
                sceneName = "Level1_Labirent",
                context = "Şu an karanlık bir labirentteyiz. Labirentte üç kapı var. Batıdaki kapının (1) arkasında " +
                          "dikenli bir çukur, doğudaki kapının (2) arkasında tehlikeli bir yaratık, kuzeydeki kapının (3) " +
                          "arkasında çıkış var. Bunu hemen söyleme, önce ipucu ver (örn. 'kuzeyden temiz hava geliyor'). " +
                          "Oyuncu ısrar ederse açıkça söyle. Yaratıktan Shift ile koşarak kaçılabilir."
            },
            new LevelContext
            {
                sceneName = "Level2_Salon",
                context = "Şu an kalenin yıkık ziyafet salonundayız ve sen oyuncunun omzundasın. Zemin çökmüş, " +
                          "aşağısı kazıklarla dolu. Geriye kalan karolardan bazıları çürük; üzerine basılınca bir " +
                          "saniye sonra düşüyor. Oyuncuya durmadan, seri zıplamasını ve karşıdaki balkona gitmesini söyle."
            },
            new LevelContext
            {
                sceneName = "Level3_Avlu",
                context = "Şu an kalenin avlusundayız. Yüksek bir çıkıntının üstünde bir şalter var; oyuncu oraya " +
                          "çıkamaz ama sen tırmanabilirsin. Oyuncu şaltere bakıp F tuşuna basarsa sen gidip indirirsin, " +
                          "yerden basamaklar yükselir. Yukarıda Gardiyan Hugo prensesin kapısını koruyor. Hugo'nun " +
                          "altına, şaraba ve emekli olma hayaline zaafı var; kaba davranılırsa sinirlenir."
            }
        };

        [Tooltip("Modele gönderilecek en fazla geçmiş mesaj sayısı.")]
        [SerializeField, Range(2, 20)] private int maxHistoryMessages = 8;

        // ---- IConversationPartner ----
        public string DisplayName => companionName;
        public string OpeningLine => openingLine;
        public bool HasTrust => false;
        public int Trust => 0;
        public bool IsThinking => llmClient != null && llmClient.IsBusy;
        public bool IsConversationOver => false; // fareyle sohbet hiç bitmez

        public event Action<string, int> OnReplied;
        public event Action<string> OnErrorOccurred;

        private ILLMClient llmClient;
        private readonly List<ChatMessage> history = new List<ChatMessage>();

        private void Awake()
        {
            llmClient = llmClientComponent as ILLMClient;
            if (llmClient == null) llmClient = GetComponent<ILLMClient>();

            if (llmClient == null)
            {
                Debug.LogError($"{name}: ILLMClient bulunamadı! Aynı objeye OllamaClient ekleyin.");
                enabled = false;
                return;
            }

            history.Add(ChatMessage.Assistant(openingLine));
        }

        public void SendPlayerMessage(string playerText)
        {
            if (IsThinking || string.IsNullOrWhiteSpace(playerText)) return;

            history.Add(ChatMessage.User(playerText.Trim()));
            TrimHistory();

            List<ChatMessage> messages = new List<ChatMessage> { ChatMessage.SystemPrompt(BuildSystemPrompt()) };
            messages.AddRange(history);

            // Fare serbest metinle cevap verir, JSON şemasına gerek yok.
            llmClient.SendChat(messages, null, HandleResult);
        }

        private string BuildSystemPrompt()
        {
            return persona + "\n\n" +
                   "BULUNDUĞUMUZ YER:\n" + GetCurrentLevelContext() + "\n\n" +
                   "KURALLAR:\n" +
                   "- Her zaman Türkçe konuş. Cevabın en fazla 2-3 kısa cümle olsun.\n" +
                   "- Asla rolden çıkma, yapay zeka olduğunu söyleme. Sen bir faresin.\n" +
                   "- Oyuncuya yardım et ve cesaret ver. Bilmediğin bir şey sorulursa uydurma, 'bilmiyorum' de.\n" +
                   "- Sadece söyleyeceğin sözü yaz, başına isim ya da tırnak koyma.";
        }

        private string GetCurrentLevelContext()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            foreach (LevelContext level in levelContexts)
            {
                if (level.sceneName == sceneName) return level.context;
            }
            return "Kalenin içindeyiz. Amacımız hapsedilmiş prensesi kurtarmak.";
        }

        private void HandleResult(LLMResult result)
        {
            if (!result.IsSuccess)
            {
                RemoveLastPlayerMessage();
                Debug.LogWarning($"[CompanionChat] LLM hatası [{result.ErrorType}]: {result.ErrorMessage}");
                OnErrorOccurred?.Invoke($"{companionName} kafasını kaşıyor, ne diyeceğini bilemedi... ({result.ErrorMessage})");
                return;
            }

            string reply = CleanReply(result.Content);
            if (string.IsNullOrEmpty(reply))
            {
                RemoveLastPlayerMessage();
                OnErrorOccurred?.Invoke($"{companionName} sadece 'cik' dedi. Tekrar sormayı dene.");
                return;
            }

            history.Add(ChatMessage.Assistant(reply));
            TrimHistory();

            Debug.Log($"[CompanionChat] {result.DurationSeconds:F2} sn\n{reply}");
            OnReplied?.Invoke(reply, 0);
        }

        /// <summary>
        /// Küçük modeller bazen cevabın başına "Fındık:" yazar ya da tırnak içine alır; bunları temizler.
        /// </summary>
        private string CleanReply(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            string reply = raw.Trim();
            string prefix = companionName + ":";
            if (reply.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                reply = reply.Substring(prefix.Length).Trim();
            }
            return reply.Trim('"', '“', '”', ' ');
        }

        private void RemoveLastPlayerMessage()
        {
            int last = history.Count - 1;
            if (last >= 0 && history[last].role == "user") history.RemoveAt(last);
        }

        private void TrimHistory()
        {
            while (history.Count > maxHistoryMessages) history.RemoveAt(0);
        }
    }
}
