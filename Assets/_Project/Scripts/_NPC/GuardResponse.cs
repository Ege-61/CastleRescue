using System;

namespace CastleRescue.NPC
{
    /// <summary>
    /// Gardiyanın oyun durumunu etkileyen kararları.
    /// </summary>
    public enum GuardAction
    {
        None,     // konuşma devam ediyor
        GiveKey,  // anahtarı verdi, kapı açılır
        Attack    // sinirlendi, saldırıyor
    }

    /// <summary>
    /// Modelin döndürdüğü JSON'un C# karşılığı:
    /// { "reply": "...", "trust": 45, "action": "none" }
    /// Alan adları JSON'daki adlarla birebir aynı olmak zorunda (JsonUtility kuralı).
    /// </summary>
    [Serializable]
    public class GuardResponse
    {
        public string reply;
        public int trust;
        public string action;

        /// <summary>Metin halindeki action alanını enum'a çevirir.</summary>
        public GuardAction ParsedAction
        {
            get
            {
                switch (action)
                {
                    case "give_key": return GuardAction.GiveKey;
                    case "attack": return GuardAction.Attack;
                    default: return GuardAction.None;
                }
            }
        }
    }
}
