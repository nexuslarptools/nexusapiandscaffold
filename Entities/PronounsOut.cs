using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class PronounsOut
    {
        public PronounsOut(Guid guid, string pronouns)
        {
            this.Guid = guid;
            this.Pronouns1 = pronouns;
        }

        public Guid Guid { get; set; }
        public string Pronouns1 { get; set; }
    }
}
