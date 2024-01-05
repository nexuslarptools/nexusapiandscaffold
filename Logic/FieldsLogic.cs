using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEXUSDataLayerScaffold.Logic
{
    public class FieldsLogic
    {
        public static JObject AddInitative(JObject FieldsList)
        {
            if (FieldsList.ContainsKey("TYPE"))
            {
                if (FieldsList["TYPE"].ToString() != "Companion")
                {
                    return FieldsList;
                }
            }

            string Initative = "See Special Skills";
            bool good = true;
            string newInit = "";
            string slash = "/";

            if (FieldsList.ContainsKey("Athletics") && FieldsList.ContainsKey("Brains"))
            {
                if (FieldsList["Athletics"].ToString().Contains("\\") || FieldsList["Brains"].ToString().Contains("\\"))
                {
                    slash = "\\";
                }

                var AthleticsList = FieldsList["Athletics"].ToString().Replace(" ", "").Replace(")", "").Replace("(", "/").Replace("\\", "/").Split("/");
                var BrainsList = FieldsList["Brains"].ToString().Replace(" ", "").Replace(")", "").Replace("(", "/").Replace("\\", "/").Split("/");

                if (AthleticsList.Count() > 1 && BrainsList.Count() > 1 && AthleticsList.Count() == BrainsList.Count())
                {
                    for (int i = 0; i < AthleticsList.Count(); i++)
                    {
                        if (int.TryParse(AthleticsList[i], out int AthVal) && int.TryParse(BrainsList[i], out int BrainVal))
                        {
                            newInit = newInit + (AthVal + BrainVal).ToString();
                        }
                        else
                        {
                            good = false;
                        }

                        if (i < AthleticsList.Count() - 1)
                        {
                            newInit = newInit + slash;
                        }
                    }
                }
                else if (AthleticsList.Count() > 1 && AthleticsList.Count() > BrainsList.Count())
                {
                    for (int i = 0; i < AthleticsList.Count(); i++)
                    {
                        if (int.TryParse(AthleticsList[i], out int AthVal) && int.TryParse(BrainsList[0], out int BrainVal))
                        {
                            newInit = newInit + (AthVal + BrainVal).ToString();
                        }
                        else
                        {
                            good = false;
                        }

                        if (i < AthleticsList.Count() - 1)
                        {
                            newInit = newInit + slash;
                        }
                    }

                }
                else if (BrainsList.Count() > 1 && AthleticsList.Count() < BrainsList.Count())
                {
                    for (int i = 0; i < BrainsList.Count(); i++)
                    {
                        if (int.TryParse(AthleticsList[0], out int AthVal) && int.TryParse(BrainsList[i], out int BrainVal))
                        {
                            newInit = newInit + (AthVal + BrainVal).ToString();
                        }
                        else
                        {
                            good = false;
                        }

                        if (i < BrainsList.Count() - 1)
                        {
                            newInit = newInit + slash;
                        }
                    }

                }
                else
                {
                    if (int.TryParse(AthleticsList[0], out int AthVal) && int.TryParse(BrainsList[0], out int BrainVal))
                    {
                        newInit = newInit + (AthVal + BrainVal).ToString();
                    }
                    else
                    {
                        good = false;
                    }
                }
            }

            if (good || newInit != "")
            {
                Initative = newInit;
            }

            FieldsList.Add("Initiative", Initative);

            return FieldsList; 
        }
    }
}
