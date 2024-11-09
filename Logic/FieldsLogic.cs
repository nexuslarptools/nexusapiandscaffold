using System.Linq;
using Newtonsoft.Json.Linq;

namespace NEXUSDataLayerScaffold.Logic;

public class FieldsLogic
{
    public static JObject AddInitative(JObject FieldsList)
    {
        if (FieldsList.ContainsKey("TYPE"))
            if (FieldsList["TYPE"].ToString() != "Companion")
                return FieldsList;

        var Initative = "See Special Skills";
        var good = true;
        var newInit = "";
        var slash = "/";

        if (FieldsList.ContainsKey("Athletics") && FieldsList.ContainsKey("Brains"))
        {
            if (FieldsList["Athletics"].ToString().Contains("\\") ||
                FieldsList["Brains"].ToString().Contains("\\")) slash = "\\";

            var AthleticsList = FieldsList["Athletics"].ToString().Replace(" ", "").Replace(")", "").Replace("(", "/")
                .Replace("\\", "/").Split("/");
            var BrainsList = FieldsList["Brains"].ToString().Replace(" ", "").Replace(")", "").Replace("(", "/")
                .Replace("\\", "/").Split("/");

            if (AthleticsList.Count() > 1 && BrainsList.Count() > 1 && AthleticsList.Count() == BrainsList.Count())
            {
                for (var i = 0; i < AthleticsList.Count(); i++)
                {
                    if (int.TryParse(AthleticsList[i], out var AthVal) && int.TryParse(BrainsList[i], out var BrainVal))
                        newInit = newInit + (AthVal + BrainVal);
                    else
                        good = false;

                    if (i < AthleticsList.Count() - 1) newInit = newInit + slash;
                }
            }
            else if (AthleticsList.Count() > 1 && AthleticsList.Count() > BrainsList.Count())
            {
                for (var i = 0; i < AthleticsList.Count(); i++)
                {
                    if (int.TryParse(AthleticsList[i], out var AthVal) && int.TryParse(BrainsList[0], out var BrainVal))
                        newInit = newInit + (AthVal + BrainVal);
                    else
                        good = false;

                    if (i < AthleticsList.Count() - 1) newInit = newInit + slash;
                }
            }
            else if (BrainsList.Count() > 1 && AthleticsList.Count() < BrainsList.Count())
            {
                for (var i = 0; i < BrainsList.Count(); i++)
                {
                    if (int.TryParse(AthleticsList[0], out var AthVal) && int.TryParse(BrainsList[i], out var BrainVal))
                        newInit = newInit + (AthVal + BrainVal);
                    else
                        good = false;

                    if (i < BrainsList.Count() - 1) newInit = newInit + slash;
                }
            }
            else
            {
                if (int.TryParse(AthleticsList[0], out var AthVal) && int.TryParse(BrainsList[0], out var BrainVal))
                    newInit = newInit + (AthVal + BrainVal);
                else
                    good = false;
            }
        }

        if (good || newInit != "") Initative = newInit;

        FieldsList.Add("Initiative", Initative);

        return FieldsList;
    }
}