using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.RNG
{
    public class MarkovWordBuilder
    {
        Dictionary<String, Dictionary<char,float>> Weights;
        int ChainLength=3;
        public MarkovWordBuilder(){}

        public List<Tuple<char,float>> GetCandidates(string key)
        {
            List<Tuple<char,float>> candidates = new List<Tuple<char,float>>();
            for (int i = 0; i < key.Length; i++)
            {
                var S = key.Length - (key.Length - i);
                var cKey = key.Substring(S);
                if (Weights.ContainsKey(cKey))
                {
                    foreach (var v in Weights[cKey])
                        candidates.Add(new Tuple<char, float>(v.Key, v.Value * S*10));
                }

            }
            //Increment(temp,, curr);
            return candidates;
        }
        public String BuildString(IPrng rng, int minLength, int maxLength)
        {
            StringBuilder sb = new StringBuilder();
            char[] chars = new char[ChainLength];
            for (int i = 0; i < ChainLength; i++) chars[i] = ' ';
            int cIndx = 0;

            while (sb.Length<maxLength)
            {
                if (sb.Length == maxLength)
                    return sb.ToString();
                var P = rng.Next();
                float t = 0;
                var key =createKey(chars, cIndx);
                char L = ' ';

                var Candadites = GetCandidates(key);
                P = P * Candadites.Sum(x => x.Item1);
                foreach(var v in Candadites)
                {
                    L = v.Item1;
                    if (t + v.Item2 > P)
                        break;
                    t += v.Item1;
                }
                if (L == '.')
                {
                    if (sb.Length >= minLength) return sb.ToString();
                }
                else
                {
                    sb.Append(L);
                    chars[cIndx % chars.Length] = L;
                    cIndx++;
                }

            }
            return sb.ToString();
        }

        /// <summary>
        /// Source should be a list of words, not sentences or paragraphs whitespace deliminates words.
        /// </summary>
        /// <param name="rdr"></param>
        /// <returns></returns>
        public void IngestStream(StreamReader rdr)
        {
            var temp = new Dictionary<String, Dictionary<char, int>>();
            ReadWhiteSpace(rdr);
            while (!rdr.EndOfStream)
            {
                char[] chars = new char[ChainLength];
                for (int i = 0; i < ChainLength; i++) chars[i] = ' ';
                int cIndx = 0;

                var curr = (char)rdr.Read();
                while(!curr.isWhitespaceChar() &&!curr.isNewLineChar() && !rdr.EndOfStream)
                {
                    var key = createKey(chars, cIndx);
                    for(int i=0;i<key.Length;i++)
                        Increment(temp, key.Substring(key.Length-(key.Length - i)), curr);
                    chars[cIndx%3] = curr;
                    cIndx++;
                    curr=(char)rdr.Read();
                }
                Increment(temp, createKey(chars, cIndx), '.');
                ReadWhiteSpace(rdr);
            }
            var ret=new Dictionary<String, Dictionary<char, float>>();
            foreach(var v in temp.Keys)
            {
                ret.Add(v, new Dictionary<char, float>());
                float total = temp[v].Values.Sum();
                foreach(var k in temp[v].Keys)
                {
                    ret[v].Add(k, temp[v][k] / total);
                }
            }
            Weights=ret;
        }
        private void Increment(Dictionary<String, Dictionary<char,int>> codex,String key, char toIncrement)
        {
            if (!codex.ContainsKey(key))
                codex.Add(key, new Dictionary<char, int>());
            if (!codex[key].ContainsKey(toIncrement))
                codex[key].Add(toIncrement, 1);
            else codex[key][toIncrement]++;
        }
        private string createKey(char[] chars, int o)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < chars.Length; i++) sb.Append(chars[(i + o) % chars.Length]);
            return sb.ToString().ToLower();
        }
        private void ReadWhiteSpace(StreamReader rdr)
        {
            while(!rdr.EndOfStream)
            {
                if (((char)rdr.Peek()).isWhitespaceChar())
                    rdr.Read();
                else break;
            }
            return;
        }

    }
}
