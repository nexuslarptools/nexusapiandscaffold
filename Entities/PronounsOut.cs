using System;

namespace NEXUSDataLayerScaffold.Entities;

public class PronounsOut
{
    public PronounsOut(Guid guid, string pronouns)
    {
        Guid = guid;
        Pronouns1 = pronouns;
    }

    public Guid Guid { get; set; }
    public string Pronouns1 { get; set; }
}