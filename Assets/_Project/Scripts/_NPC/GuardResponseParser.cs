using System;
using CastleRescue.LLM;
using UnityEngine;

namespace CastleRescue.NPC
{
    /// <summary>
    /// Modelden gelen metni GuardResponse nesnesine çevirir.
    /// Bozuk, eksik ya da fazlalık içeren çıktılara karşı dayanıklıdır:
    ///  - ```json ... ``` gibi kod bloğu işaretlerini temizler
    ///  - JSON'dan önce/sonra yazılmış fazladan metni atar
    ///  - Eksik alanları ve geçersiz değerleri yakalar
    /// Hiçbir durumda exception fırlatmaz.
    /// </summary>
    public class GuardResponseParser : IResponseParser<GuardResponse>
    {
        public bool TryParse(string raw, out GuardResponse result, out string error)
        {
            result = null;
            error = null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "Model boş cevap döndürdü.";
                return false;
            }

            // 1) Metnin içindeki ilk '{' ile son '}' arasını al.
            //    Böylece "İşte cevabım: {...} Umarım beğenirsin" gibi çıktılar da okunabilir.
            int start = raw.IndexOf('{');
            int end = raw.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                error = "Cevapta JSON nesnesi bulunamadı.";
                return false;
            }

            string json = raw.Substring(start, end - start + 1);

            // 2) JSON'u nesneye çevirmeyi dene.
            GuardResponse parsed;
            try
            {
                parsed = JsonUtility.FromJson<GuardResponse>(json);
            }
            catch (Exception e)
            {
                error = $"JSON ayrıştırılamadı: {e.Message}";
                return false;
            }

            // 3) İçeriği doğrula.
            if (parsed == null)
            {
                error = "JSON boş nesne üretti.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(parsed.reply))
            {
                error = "JSON'da 'reply' alanı eksik ya da boş.";
                return false;
            }

            // action tanımadığımız bir değerse oyunu bozmasın, "none" kabul et.
            if (parsed.action != "none" && parsed.action != "give_key" && parsed.action != "attack")
            {
                Debug.LogWarning($"[GuardParser] Bilinmeyen action '{parsed.action}', 'none' kabul edildi.");
                parsed.action = "none";
            }

            // trust 0-100 aralığında kalsın.
            parsed.trust = Mathf.Clamp(parsed.trust, 0, 100);
            parsed.reply = parsed.reply.Trim();

            result = parsed;
            return true;
        }
    }
}
