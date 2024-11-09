using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class Pronoun
{
    public Guid Guid { get; set; }

    public string Pronouns { get; set; }

    public virtual ICollection<User> Users { get; } = new List<User>();
}