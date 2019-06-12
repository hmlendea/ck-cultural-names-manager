using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public sealed class NameTransliterator : INameTransliterator
    {
        readonly IDictionary<string, string> cache;

        public NameTransliterator()
        {
            this.cache = new Dictionary<string, string>();
        }

        public string Transliterate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (cache.ContainsKey(name))
            {
                return name;
            }

            string transliteratedName = Transliterate(name, greekTransliterationTable);

            cache.Add(name, transliteratedName);

            return transliteratedName;
        }

        string Transliterate(string name, IDictionary<string, string> tansliterationTable)
        {
            string transliteratedName = name;

            foreach (string originalPair in tansliterationTable.Keys)
            {
                transliteratedName = Regex.Replace(transliteratedName, originalPair, tansliterationTable[originalPair]);
            }

            return transliteratedName;
        }

        // ALA-LC 2010
        readonly IDictionary<string, string> greekTransliterationTable = new Dictionary<string, string>
        {
            { "Αι", "Ai" },
            { "Αί", "Aí" },
            { "Αϊ", "Aï" },
            { "Άι", "Ái" },
            { "Άί", "Áí" },
            { "Άϊ", "Áï" },
            { "Χ", "Ch" },
            { "Ει", "Ei" },
            { "Εί", "Eí" },
            { "Εϊ", "Eï" },
            { "Έι", "Éi" },
            { "Έί", "Éí" },
            { "Έϊ", "Éï" },
            { "Οι", "Oi" },
            { "Οί", "Oí" },
            { "Οϊ", "Oï" },
            { "Όι", "Ói" },
            { "Όί", "Óí" },
            { "Όϊ", "Óï" },
            { "Ου", "Ou" },
            { "Ού", "Oý" },
            { "Όυ", "Óu" },
            { "Όύ", "Óý" },
            { "Φ", "Ph" },
            { "Ψ", "Ps" },
            { "Θ", "Th" },
            { "Υι", "Ui" },
            { "Ύι", "Úi" },
            { "Υί", "Uí" },
            { "Ύί", "Úí" },
            { "Υϊ", "Uï" },
            { "Ύϊ", "Úï" },

            { "αι", "ai" },
            { "αί", "aí" },
            { "αϊ", "aï" },
            { "άι", "ái" },
            { "άί", "áí" },
            { "άϊ", "áï" },
            { "χ", "ch" },
            { "ει", "ei" },
            { "εί", "eí" },
            { "εϊ", "eï" },
            { "έι", "éi" },
            { "έί", "éí" },
            { "έϊ", "éï" },
            { "γχ", "nch" },
            { "γγ", "ng" },
            { "γκ", "nk" },
            { "γξ", "nx" },
            { "οι", "oi" },
            { "οί", "oí" },
            { "οϊ", "oï" },
            { "όι", "ói" },
            { "όί", "óí" },
            { "όϊ", "óï" },
            { "ου", "ou" },
            { "ού", "oý" },
            { "όυ", "óu" },
            { "όύ", "óý" },
            { "φ", "ph" },
            { "ψ", "ps" },
            { "θ", "th" },
            { "υι", "ui" },
            { "ύι", "ýi" },
            { "υί", "uí" },
            { "ύί", "ýí" },
            { "υϊ", "uï" },
            { "ύϊ", "ýï" },

            // Word-initial rho
            { "^Ρ", "Rh" },
            { " Ρ", "Rh" },
            { "^ρ", "rh" },
            { " ρ", "rh" },

            // Medial rho's
            { "Ρρ", "Rrh" },
            { "ρρ", "rrh" },

            // υ as u in some diphtongs
            { "αυ", "au" },
            { "αύ", "aý" },
            { "άυ", "áu" },
            { "άύ", "áý" },
            { "ευ", "eu" },
            { "εύ", "eý" },
            { "έυ", "éu" },
            { "έύ", "éý" },
            { "ηυ", "eu" },
            { "ηύ", "eý" },
            { "ῆυ", "êu" },
            { "ῆύ", "êý" },
            { "ωυ", "ou" },
            { "ωύ", "oý" },

            { "Α", "A" },
            { "Ά", "Á" },
            { "Β", "B" },
            { "Δ", "D" },
            { "Ε", "E" },
            { "Έ", "É" },
            { "Η", "E" },
            { "Γ", "G" },
            { "Ι", "I" },
            { "Κ", "K" },
            { "Λ", "L" },
            { "Μ", "M" },
            { "Ν", "N" },
            { "Ο", "O" },
            { "Ό", "Ó" },
            { "Ω", "Ô" },
            { "Π", "P" },
            { "Ρ", "R" },
            { "Σ", "S" },
            { "Τ", "T" },
            { "Ξ", "X" },
            { "Υ", "Y" },
            { "Ύ", "Ý" },
            { "Ζ", "Z" },

            { "α", "a" },
            { "ά", "á" },
            { "β", "b" },
            { "δ", "d" },
            { "ε", "e" },
            { "έ", "é" },
            { "η", "e" },
            { "ῆ", "ê" },
            { "γ", "g" },
            { "ι", "i" },
            { "ί", "í" },
            { "ϊ", "ï" },
            { "κ", "k" },
            { "λ", "l" },
            { "μ", "m" },
            { "ν", "n" },
            { "ο", "o" },
            { "ό", "ó" },
            { "ω", "ô" },
            { "π", "p" },
            { "ρ", "r" },
            { "σ", "s" },
            { "ς", "s" },
            { "τ", "t" },
            { "ξ", "x" },
            { "υ", "y" },
            { "ύ", "ý" },
            { "ζ", "z" },
        };
    }
}
