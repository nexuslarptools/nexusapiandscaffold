using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class AuthUser
    {
    public string name { get; set; }
    public Guid userGuid { get; set; }
    public string picture { get; set; }
    public string email { get; set; }
    public string authid { get; set; }
    public List<string> roles { get; set; }
    public List<string> permissions { get; set; }
    }
    

}
