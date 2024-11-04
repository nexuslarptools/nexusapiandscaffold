using System.Collections.Generic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class CharacterSheetDO
{
    public CharacterSheet Sheet { get; set; }
    public List<Tag> TagList { get; set; }

    public User Createdbyuser { get; set; }
    public User EditbyUser { get; set; }
    public User Firstapprovalbyuser { get; set; }
    public User Secondapprovalbyuser { get; set; }
    public Series Series { get; set; }
    public List<CharacterSheetReviewMessage> CharacterSheetReviewMessages { get; set; }
}