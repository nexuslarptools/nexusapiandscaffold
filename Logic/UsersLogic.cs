using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NEXUSDataLayerScaffold.Enums;

namespace NEXUSDataLayerScaffold.Logic
{
    public class UsersLogic
    {
        private readonly NexusLARPContextBase _context;

        public UsersLogic(NexusLARPContextBase context)
        {
            _context = context;
        }

        public static bool IsUserAuthed(string authIdValue, string accessToken, string authLevel, NexusLARPContextBase _context)
        {
            var foundUser = _context.Users.Where(u => u.Authid == authIdValue).FirstOrDefault();

            if (foundUser == null)
            {
                var autheduser = GetUserInfo(accessToken, _context);

                Users newUsers = new Users
                {
                    Email = autheduser.Result.email,
                    Preferredname = autheduser.Result.name,
                    Authid = autheduser.Result.authid
                };

                try
                {
                    _context.Users.Add(newUsers);
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    var huh = e;
                }

                return false;
            }

            var roleNum = _context.Roles.Where(r => r.Rolename == authLevel).Select(r => r.Id).FirstOrDefault();

            var foundrole = _context.UserLarproles.Where(ulr => ulr.Userguid == foundUser.Guid && ulr.Role.Id >= roleNum && ulr.Isactive == true).FirstOrDefault();

            if (foundrole == null)
            {
                if (authIdValue == "auth0|5eb6c2556b69bc0c120737e9")
                {
                    foundrole = _context.UserLarproles.Where(ulr => ulr.Userguid == foundUser.Guid && ulr.Role.Rolename == authLevel && ulr.Isactive == false).FirstOrDefault();

                    if (foundrole == null)
                    {
                        foundrole = new UserLarproles()
                        {
                            Userguid = foundUser.Guid,
                            Larpguid = _context.Larps.Where(l => l.Isactive == true && l.Name == "Default").Select(l => l.Guid).FirstOrDefault(),
                            Roleid = _context.Roles.Where(r => r.Rolename == "Wizard").Select(l => l.Id).FirstOrDefault(),
                        };

                        _context.UserLarproles.Add(foundrole);
                    }
                    else
                    {
                        foundrole.Isactive = true;
                        _context.UserLarproles.Update(foundrole);
                    }

                    _context.SaveChanges();
                }

                return false;
            }

            return true;
        }


        public static async Task<AuthUser> GetUserInfo(string accessToken2, NexusLARPContextBase _context)
        {
            // Get the access token.

            AuthUser returnuser = new AuthUser();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);

            HttpResponseMessage responseMessage = await client.GetAsync("https://dev-3xazewbu.auth0.com/userinfo");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var JsonHoldingCell = JsonDocument.Parse(responseData);

                returnuser.name = JsonHoldingCell.RootElement.GetProperty("name").ToString();
                returnuser.authid = JsonHoldingCell.RootElement.GetProperty("sub").ToString();
                returnuser.email = JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/email").ToString();

                List<string> permissionsholder = new List<string>();
                for (int i = 0; i < JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/permissions").GetArrayLength(); i++)
                {
                    permissionsholder.Add(JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/permissions")[i].ToString());
                }
                returnuser.permissions = permissionsholder;

                permissionsholder = new List<string>();
                for (int i = 0; i < JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/roles").GetArrayLength(); i++)
                {
                    permissionsholder.Add(JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/roles")[i].ToString());
                }
                returnuser.roles = permissionsholder;

            }
            return returnuser;

        }

        public static List<Guid?> GetUserTagsList(string authIdValue, NexusLARPContextBase _context, string level, bool isWizard)
        {
            if (isWizard)
            {
               return _context.Larptags.Where(lt => lt.Isactive == true && lt.Larpguid != null).Select(lt => lt.Tagguid).ToList();
            }

            List<Guid?> returnlist = new List<Guid?>();

            var querylevel =  _context.Roles.Where(r => r.Rolename == level).FirstOrDefault();

            List<Guid?> userLarps = _context.UserLarproles.Where(ulr => ulr.Isactive == true && ulr.Usergu.Authid == authIdValue && ulr.Role.Id >= querylevel.Id)
                .Select(ulr => ulr.Larpguid).ToList();

            returnlist = _context.Larptags.Where(lt => lt.Isactive == true && userLarps.Contains(lt.Larpguid)).Select(lt => lt.Tagguid).ToList();

            return returnlist;
        }

        public static Guid GetUserGuid(string authIdValue, NexusLARPContextBase _context)
        {
            return _context.Users.Where(u => u.Authid == authIdValue).Select(u => u.Guid).FirstOrDefault();
        }
    }
}
