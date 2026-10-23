using System;
using UnityEngine;

namespace CastleRescue.UI
{
    /// <summary>
    /// Önceden yazılmış (senaryolu) diyalogdaki tek bir satır.
    /// Inspector'dan düzenlenebilsin diye [Serializable].
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        public string speaker;

        [TextArea(1, 3)]
        public string text;

        public DialogueLine() { } // Unity'nin Inspector'da yeni satır oluşturabilmesi için

        public DialogueLine(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }
}
