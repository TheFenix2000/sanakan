﻿#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Sanakan.Database.Models;
using Sanakan.Extensions;
using Sanakan.Preconditions;
using Sanakan.Services;
using Sanakan.Services.Commands;
using Sanakan.Services.Session;
using Sanakan.Services.Session.Models;
using Sanakan.Services.Time;
using Z.EntityFramework.Plus;

namespace Sanakan.Modules
{
    [Name("Profil"), RequireUserRole]
    public class Profile : SanakanModuleBase<SocketCommandContext>
    {
        private Services.Profile _profile;
        private SessionManager _session;
        private ISystemTime _time;

        public Profile(Services.Profile prof, SessionManager session, ISystemTime time)
        {
            _time = time;
            _profile = prof;
            _session = session;
        }

        [Command("portfel", RunMode = RunMode.Async)]
        [Alias("wallet")]
        [Summary("wyświetla portfel użytkownika")]
        [Remarks("Karna")]
        public async Task ShowWalletAsync([Summary("nazwa użytkownika")]SocketUser user = null)
        {
            var usr = user ?? Context.User;
            if (usr == null) return;

            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetCachedFullUserAsync(usr.Id);
                if (botuser == null)
                {
                    await ReplyAsync("", embed: "Ta osoba nie ma profilu bota.".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                await ReplyAsync("", embed: ($"**Portfel** {usr.Mention}:\n\n"
                    + $"<:msc:1208499475631312906> {botuser?.ScCnt}\n"
                    + $"<:mtc:1208499476965236847> {botuser?.TcCnt}\n"
                    + $"<:mac:1208499478097830069> {botuser?.AcCnt}\n\n"
                    + $"**PW**:\n"
                    + $"<:mct:1208499474343665724> {botuser?.GameDeck?.CTCnt}\n"
                    + $"<:mpc:1208499455242801262> {botuser?.GameDeck?.PVPCoins}").ToEmbedMessage(EMType.Info).Build());
            }
        }

        [Command("subskrypcje", RunMode = RunMode.Async)]
        [Alias("sub")]
        [Summary("wyświetla daty zakończenia subskrypcji")]
        [Remarks(""), RequireCommandChannel]
        public async Task ShowSubsAsync()
        {
            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetCachedFullUserAsync(Context.User.Id);
                var rsubs = botuser.TimeStatuses.Where(x => x.Type.IsSubType());

                string subs = "brak";
                if (rsubs.Count() > 0)
                {
                    subs = "";
                    foreach (var sub in rsubs)
                        subs += $"{sub.ToView(_time.Now())}\n";
                }

                await ReplyAsync("", embed: $"**Subskrypcje** {Context.User.Mention}:\n\n{subs.TrimToLength()}".ToEmbedMessage(EMType.Info).Build());
            }
        }

        [Command("przyznaj role", RunMode = RunMode.Async)]
        [Alias("add role")]
        [Summary("dodaje samo zarządzaną role")]
        [Remarks("newsy"), RequireCommandChannel]
        public async Task AddRoleAsync([Summary("nazwa roli z wypisz role")]string name)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null) return;

            using (var db = new Database.DatabaseContext(Config))
            {
                var config = await db.GetCachedGuildFullConfigAsync(Context.Guild.Id);
                var selfRole = config.SelfRoles.FirstOrDefault(x => x.Name == name);
                var gRole = Context.Guild.GetRole(selfRole?.Role ?? 0);

                if (gRole == null)
                {
                    await ReplyAsync("", embed: $"Nie odnaleziono roli `{name}`".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                if (!user.Roles.Contains(gRole))
                    await user.AddRoleAsync(gRole);

                await ReplyAsync("", embed: $"{user.Mention} przyznano rolę: `{name}`".ToEmbedMessage(EMType.Success).Build());
            }
        }

        [Command("zdejmij role", RunMode = RunMode.Async)]
        [Alias("remove role")]
        [Summary("zdejmuje samo zarządzaną role")]
        [Remarks("newsy"), RequireCommandChannel]
        public async Task RemoveRoleAsync([Summary("nazwa roli z wypisz role")]string name)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null) return;

            using (var db = new Database.DatabaseContext(Config))
            {
                var config = await db.GetCachedGuildFullConfigAsync(Context.Guild.Id);
                var selfRole = config.SelfRoles.FirstOrDefault(x => x.Name == name);
                var gRole = Context.Guild.GetRole(selfRole?.Role ?? 0);

                if (gRole == null)
                {
                    await ReplyAsync("", embed: $"Nie odnaleziono roli `{name}`".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                if (user.Roles.Contains(gRole))
                    await user.RemoveRoleAsync(gRole);

                await ReplyAsync("", embed: $"{user.Mention} zdjęto rolę: `{name}`".ToEmbedMessage(EMType.Success).Build());
            }
        }

        [Command("wypisz role", RunMode = RunMode.Async)]
        [Summary("wypisuje samozarządzane role")]
        [Remarks(""), RequireCommandChannel]
        public async Task ShowRolesAsync()
        {
            using (var db = new Database.DatabaseContext(Config))
            {
                var config = await db.GetCachedGuildFullConfigAsync(Context.Guild.Id);
                if (config.SelfRoles.Count < 1)
                {
                    await ReplyAsync("", embed: "Nie odnaleziono roli.".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                string stringRole = "";
                foreach (var selfRole in config.SelfRoles)
                {
                    var gRole = Context.Guild.GetRole(selfRole?.Role ?? 0);
                    stringRole += $" `{selfRole.Name}` ";
                }

                await ReplyAsync($"**Dostępne role:**\n{stringRole}\n\nUżyj `s.przyznaj role [nazwa]` aby dodać lub `s.zdejmij role [nazwa]` odebrać sobie role.");
            }
        }

        [Command("statystyki", RunMode = RunMode.Async)]
        [Alias("stats")]
        [Summary("wyświetla statystyki użytkownika")]
        [Remarks("karna")]
        public async Task ShowStatsAsync([Summary("nazwa użytkownika")]SocketUser user = null)
        {
            var usr = user ?? Context.User;
            if (usr == null) return;

            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetBaseUserAndDontTrackAsync(usr.Id);
                if (botuser == null)
                {
                    await ReplyAsync("", embed: "Ta osoba nie ma profilu bota.".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                await ReplyAsync("", embed: botuser.GetStatsView(usr).Build());
            }
        }

        [Command("idp", RunMode = RunMode.Async)]
        [Alias("iledopoziomu", "howmuchtolevelup", "hmtlup")]
        [Summary("wyświetla ile pozostało punktów doświadczenia do następnego poziomu")]
        [Remarks("karna")]
        public async Task ShowHowMuchToLevelUpAsync([Summary("nazwa użytkownika")]SocketUser user = null)
        {
            var usr = user ?? Context.User;
            if (usr == null) return;

            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.Users.AsQueryable().AsSplitQuery().Where(x => x.Id == usr.Id).AsNoTracking().FirstOrDefaultAsync();
                if (botuser == null)
                {
                    await ReplyAsync("", embed: "Ta osoba nie ma profilu bota.".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                await ReplyAsync("", embed: $"{usr.Mention} potrzebuje **{botuser.GetRemainingExp()}** punktów doświadczenia do następnego poziomu."
                    .ToEmbedMessage(EMType.Info).Build());
            }
        }

        [Command("topka", RunMode = RunMode.Async)]
        [Alias("top")]
        [Summary("wyświetla topke użytkowników")]
        [Remarks(""), RequireAnyCommandChannel]
        public async Task ShowTopAsync([Summary("rodzaj topki (poziom/sc/tc/pc/ac/posty(m/ms)/kart(a/y/ym)/karma(-))/pvp(s)")]TopType type = TopType.Level)
        {
            var session = new ListSession<string>(Context.User, Context.Client.CurrentUser);
            await _session.KillSessionIfExistAsync(session);

            var building = await ReplyAsync("", embed: $"🔨 Trwa budowanie topki...".ToEmbedMessage(EMType.Bot).Build());
            using (var db = new Database.DatabaseContext(Config))
            {
                session.ListItems = _profile.BuildListView(await _profile.GetTopUsers(db.GetQueryableAllUsers(), type), type, Context.Guild);
            }

            session.Event = ExecuteOn.ReactionAdded;
            session.Embed = new EmbedBuilder
            {
                Color = EMType.Info.Color(),
                Title = $"Topka {type.Name()}"
            };

            await building.DeleteAsync();
            var msg = await ReplyAsync("", embed: session.BuildPage(0));
            await msg.AddReactionsAsync(new[] { new Emoji("⬅"), new Emoji("➡") });

            session.Message = msg;
            await _session.TryAddSession(session);
        }

        [Command("widok waifu")]
        [Alias("waifu view")]
        [Summary("przełącza widoczność waifu na pasku bocznym profilu użytkownika")]
        [Remarks(""), RequireAnyCommandChannel]
        public async Task ToggleWaifuViewInProfileAsync()
        {
            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetUserOrCreateSimpleAsync(Context.User.Id);
                botuser.ShowWaifuInProfile = !botuser.ShowWaifuInProfile;

                string result = botuser.ShowWaifuInProfile ? "załączony" : "wyłączony";

                await db.SaveChangesAsync();

                QueryCacheManager.ExpireTag(new string[] { $"user-{botuser.Id}", "users" });

                await ReplyAsync("", embed: $"Podgląd waifu w profilu {Context.User.Mention} został {result}.".ToEmbedMessage(EMType.Success).Build());
            }
        }

        [Command("wersja profilu")]
        [Alias("profile version")]
        [Summary("zmienia wersje profilu użytkownika")]
        [Remarks("1"), RequireAnyCommandChannel]
        public async Task ChangeProfileVersioneAsync([Summary("wersja (0 - stary, 1 - nowy z paskiem na górze, 2 - nowy z paskiem na dole)")]ProfileVersion version)
        {
            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetUserOrCreateSimpleAsync(Context.User.Id);
                var wasOld = botuser.ProfileVersion == ProfileVersion.Old;
                var isOld = version == ProfileVersion.Old;

                botuser.ProfileVersion = version;

                if (wasOld != isOld)
                {
                    botuser.BackgroundProfileUri = isOld ? "./Pictures/defBg.png" : "./Pictures/np/pbg.png";
                    botuser.StatsReplacementProfileUri = "none";
                }

                await db.SaveChangesAsync();

                QueryCacheManager.ExpireTag(new string[] { $"user-{botuser.Id}", "users" });

                await ReplyAsync("", embed: $"{Context.User.Mention} zmieniono wersje profilu.".ToEmbedMessage(EMType.Success).Build());
            }
        }

        [Command("profil", RunMode = RunMode.Async)]
        [Alias("profile")]
        [Summary("wyświetla profil użytkownika")]
        [Remarks("karna"), DelayNextUseBy(15)]
        public async Task ShowUserProfileAsync([Summary("nazwa użytkownika")]SocketGuildUser user = null)
        {
            var usr = user ?? Context.User as SocketGuildUser;
            if (usr == null) return;

            ulong searchId = usr.Id == Context.Client.CurrentUser.Id ? 1 : usr.Id;
            using (var db = new Database.DatabaseContext(Config))
            {
                var allUsers = await db.GetCachedAllUsersLiteAsync();
                var botUser = allUsers.FirstOrDefault(x => x.Id == searchId);
                if (botUser == null)
                {
                    await ReplyAsync("", embed: "Ta osoba nie ma profilu bota.".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                var dataUser = db.Users.AsQueryable().Include(x => x.GameDeck).AsNoTracking().FirstOrDefault(x => x.Id == searchId);
                dataUser.GameDeck.Cards = (await db.GetCachedUserGameDeckAsync(searchId)).Cards;
                using (var stream = await _profile.GetProfileImageAsync(usr, dataUser, allUsers.OrderByDescending(x => x.ExpCnt).ToList().IndexOf(botUser) + 1))
                {
                    await Context.Channel.SendFileAsync(stream, $"{usr.Id}.png");
                }
            }
        }

        [Command("misje")]
        [Alias("quest")]
        [Summary("wyświetla postęp misji użytkownika")]
        [Remarks("tak"), RequireAnyCommandChannel]
        public async Task ShowUserQuestsProgressAsync([Summary("czy odebrać nagrody?")]bool claim = false)
        {
            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetUserOrCreateSimpleAsync(Context.User.Id);
                var weeklyQuests = botuser.CreateOrGetAllWeeklyQuests();
                var dailyQuests = botuser.CreateOrGetAllDailyQuests();

                if (claim)
                {
                    var rewards = new List<string>();
                    var allClaimedBefore = dailyQuests.Count(x => x.IsClaimed(_time.Now())) == dailyQuests.Count;
                    foreach(var d in dailyQuests)
                    {
                        if (d.CanClaim(_time.Now()))
                        {
                            d.Claim(botuser);
                            rewards.Add(d.Type.GetRewardString());
                        }
                    }

                    if (!allClaimedBefore && dailyQuests.Count(x => x.IsClaimed(_time.Now())) == dailyQuests.Count)
                    {
                        botuser.AcCnt += 10;
                        rewards.Add("10 AC");
                    }

                    foreach(var w in weeklyQuests)
                    {
                        if (w.CanClaim(_time.Now()))
                        {
                            w.Claim(botuser);
                            rewards.Add(w.Type.GetRewardString());
                        }
                    }

                    if (rewards.Count > 0)
                    {
                        QueryCacheManager.ExpireTag(new string[] { $"user-{botuser.Id}", "users" });

                        await ReplyAsync("", embed: $"**Odebrane nagrody:**\n\n{string.Join("\n", rewards)}".ToEmbedMessage(EMType.Success).WithUser(Context.User).Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    await ReplyAsync("", embed: "Nie masz nic do odebrania.".ToEmbedMessage(EMType.Error).WithUser(Context.User).Build());
                    return;
                }

                string dailyTip = "Za wykonanie wszystkich dziennych misji można otrzymać 10 AC.";
                string totalTip = "Dzienne misje odświeżają się o północy, a tygodniowe co niedzielę.";
                string daily = $"**Dzienne misje:**\n\n{string.Join("\n", dailyQuests.Select(x => x.ToView(_time.Now())))}";
                string weekly = $"**Tygodniowe misje:**\n\n{string.Join("\n", weeklyQuests.Select(x => x.ToView(_time.Now())))}";

                await ReplyAsync("", embed: $"{daily}\n\n{dailyTip}\n\n\n{weekly}\n\n{totalTip}".ToEmbedMessage(EMType.Bot).WithUser(Context.User).Build());
            }
        }

        [Command("styl")]
        [Alias("style")]
        [Summary("zmienia styl profilu (koszt 3000 SC/1000 TC)")]
        [Remarks("1 https://sanakan.pl/i/example_style_1.png sc"), RequireCommandChannel]
        public async Task ChangeStyleAsync([Summary("typ stylu (statystyki(0), obrazek(1), brzydkie(2), karcianka(3))")]ProfileType type, [Summary("bezpośredni adres do obrazka gdy wybrany styl 1 lub 2 (325 x 272 dla starego, lub 750 x 340 dla nowego)")]string imgUrl = null, [Summary("waluta (SC/TC)")]SCurrency currency = SCurrency.Sc)
        {
            var scCost = 3000;
            var tcCost = 1000;

            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetUserOrCreateSimpleAsync(Context.User.Id);
                if (botuser.ScCnt < scCost && currency == SCurrency.Sc)
                {
                    await ReplyAsync("", embed: $"{Context.User.Mention} nie posiadasz wystarczającej liczby SC!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }
                if (botuser.TcCnt < tcCost && currency == SCurrency.Tc)
                {
                    await ReplyAsync("", embed: $"{Context.User.Mention} nie posiadasz wystarczającej liczby TC!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                switch (type)
                {
                    case ProfileType.CardsOnImg:
                    case ProfileType.StatsOnImg:
                        if (botuser.ProfileVersion == ProfileVersion.Old)
                        {
                            await ReplyAsync("", embed: $"{Context.User.Mention} te style nie sa wspierana na starym profilu!".ToEmbedMessage(EMType.Error).Build());
                            return;
                        }
                        goto case ProfileType.StatsWithImg;
                    case ProfileType.Img:
                    case ProfileType.StatsWithImg:
                        var res = botuser.ProfileVersion switch
                        {
                            ProfileVersion.Old => await _profile.SaveProfileImageAsync(imgUrl, $"{Dir.SavedData}/SR{botuser.Id}.png", 325, 272),
                            _ => await _profile.SaveProfileImageAsync(imgUrl, $"{Dir.SavedData}/SR{botuser.Id}.png", 750, 340)
                        };
                        if (res == SaveResult.Success)
                        {
                            botuser.StatsReplacementProfileUri = $"{Dir.SavedData}/SR{botuser.Id}.png";
                            break;
                        }
                        else if (res == SaveResult.BadUrl)
                        {
                            await ReplyAsync("", embed: "Nie wykryto obrazka! Upewnij się, że podałeś poprawny adres!".ToEmbedMessage(EMType.Error).Build());
                            return;
                        }
                        await ReplyAsync("", embed: "Coś poszło nie tak, prawdopodobnie nie mam uprawnień do zapisu!".ToEmbedMessage(EMType.Error).Build());
                        return;

                    default:
                        break;
                }

                if (currency == SCurrency.Sc)
                {
                    botuser.ScCnt -= scCost;
                }
                else
                {
                    botuser.TcCnt -= tcCost;
                }
                botuser.ProfileType = type;

                await db.SaveChangesAsync();

                QueryCacheManager.ExpireTag(new string[] { $"user-{botuser.Id}", "users" });

                await ReplyAsync("", embed: $"Zmieniono styl profilu użytkownika: {Context.User.Mention}!".ToEmbedMessage(EMType.Success).Build());
            }
        }

        [Command("tło")]
        [Alias("tlo", "bg", "background")]
        [Summary("zmienia obrazek tła profilu (koszt 5000 SC/2500 TC)")]
        [Remarks("https://sanakan.pl/i/example_profile_bg.png sc"), RequireCommandChannel]
        public async Task ChangeBackgroundAsync([Summary("bezpośredni adres do obrazka (450 x 145 dla starego, lub 750 x 160 dla nowego)")]string imgUrl, [Summary("waluta (SC/TC)")]SCurrency currency = SCurrency.Sc)
        {
            var tcCost = 2500;
            var scCost = 5000;

            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetUserOrCreateSimpleAsync(Context.User.Id);
                if (botuser.ScCnt < scCost && currency == SCurrency.Sc)
                {
                    await ReplyAsync("", embed: $"{Context.User.Mention} nie posiadasz wystarczającej liczby SC!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }
                if (botuser.TcCnt < tcCost && currency == SCurrency.Tc)
                {
                    await ReplyAsync("", embed: $"{Context.User.Mention} nie posiadasz wystarczającej liczby TC!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                var res = botuser.ProfileVersion switch
                {
                    ProfileVersion.Old => await _profile.SaveProfileImageAsync(imgUrl, $"{Dir.SavedData}/BG{botuser.Id}.png", 450, 145, true),
                    _ => await _profile.SaveProfileImageAsync(imgUrl, $"{Dir.SavedData}/BG{botuser.Id}.png", 750, 160, true)
                };

                if (res == SaveResult.Success)
                {
                    botuser.BackgroundProfileUri = $"{Dir.SavedData}/BG{botuser.Id}.png";
                }
                else if (res == SaveResult.BadUrl)
                {
                    await ReplyAsync("", embed: "Nie wykryto obrazka! Upewnij się, że podałeś poprawny adres!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }
                else
                {
                    await ReplyAsync("", embed: "Coś poszło nie tak, prawdopodobnie nie mam uprawnień do zapisu!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                if (currency == SCurrency.Sc)
                {
                    botuser.ScCnt -= scCost;
                }
                else
                {
                    botuser.TcCnt -= tcCost;
                }

                await db.SaveChangesAsync();

                QueryCacheManager.ExpireTag(new string[] { $"user-{botuser.Id}", "users" });

                await ReplyAsync("", embed: $"Zmieniono tło profilu użytkownika: {Context.User.Mention}!".ToEmbedMessage(EMType.Success).Build());
            }
        }

        [Command("globalki")]
        [Alias("global")]
        [Summary("nadaje na miesiąc rangę od globalnych emotek (1000 TC)")]
        [Remarks(""), RequireCommandChannel]
        public async Task AddGlobalEmotesAsync()
        {
            var cost = 1000;
            var user = Context.User as SocketGuildUser;
            if (user == null) return;

            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetUserOrCreateSimpleAsync(user.Id);
                if (botuser.TcCnt < cost)
                {
                    await ReplyAsync("", embed: $"{user.Mention} nie posiadasz wystarczającej liczby TC!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                var gConfig = await db.GetCachedGuildFullConfigAsync(Context.Guild.Id);
                var gRole = Context.Guild.GetRole(gConfig.GlobalEmotesRole);
                if (gRole == null)
                {
                    await ReplyAsync("", embed: "Serwer nie ma ustawionej roli globalnych emotek.".ToEmbedMessage(EMType.Bot).Build());
                    return;
                }

                var global = botuser.TimeStatuses.FirstOrDefault(x => x.Type == Database.Models.StatusType.Globals && x.Guild == Context.Guild.Id);
                if (global == null)
                {
                    global = StatusType.Globals.NewTimeStatus(Context.Guild.Id);
                    botuser.TimeStatuses.Add(global);
                }

                if (!user.Roles.Contains(gRole))
                    await user.AddRoleAsync(gRole);

                global.BValue = true;
                global.EndsAt = global.EndsAt.AddMonths(1);
                botuser.TcCnt -= cost;

                await db.SaveChangesAsync();

                QueryCacheManager.ExpireTag(new string[] { $"user-{botuser.Id}", "users" });

                await ReplyAsync("", embed: $"{user.Mention} wykupił miesiąc globalnych emotek!".ToEmbedMessage(EMType.Success).Build());
            }
        }

        [Command("kolor")]
        [Alias("color", "colour")]
        [Summary("zmienia kolor użytkownika (koszt TC/SC na liście)")]
        [Remarks("pink sc"), RequireCommandChannel]
        public async Task ToggleColorRoleAsync([Summary("kolor z listy (none - lista)")]FColor color = FColor.None, [Summary("waluta (SC/TC)")]SCurrency currency = SCurrency.Tc)
        {
            var user = Context.User as SocketGuildUser;
            if (user == null) return;

            if (color == FColor.None)
            {
                using (var img = _profile.GetColorList(currency))
                {
                    await Context.Channel.SendFileAsync(img, "list.png");
                    return;
                }
            }

            using (var db = new Database.DatabaseContext(Config))
            {
                var botuser = await db.GetUserOrCreateSimpleAsync(user.Id);
                var points = currency == SCurrency.Tc ? botuser.TcCnt : botuser.ScCnt;
                var gConfig = await db.GetCachedGuildFullConfigAsync(Context.Guild.Id);
                var hasNitro = gConfig.NitroRole != 0 && ((Context.User as SocketGuildUser)?.Roles?.Any(x => x.Id == gConfig.NitroRole) ?? false);
                if (!hasNitro && points < color.Price(currency))
                {
                    await ReplyAsync("", embed: $"{user.Mention} nie posiadasz wystarczającej liczby {currency.ToString().ToUpper()}!".ToEmbedMessage(EMType.Error).Build());
                    return;
                }

                var colort = botuser.TimeStatuses.FirstOrDefault(x => x.Type == Database.Models.StatusType.Color && x.Guild == Context.Guild.Id);
                if (colort == null)
                {
                    colort = StatusType.Color.NewTimeStatus(Context.Guild.Id);
                    botuser.TimeStatuses.Add(colort);
                }

                if (color == FColor.CleanColor)
                {
                    colort.BValue = false;
                    colort.EndsAt = _time.Now();
                    await _profile.RomoveUserColorAsync(user);
                }
                else
                {
                    if (!hasNitro && _profile.HasSameColor(user, color) && colort.IsActive(_time.Now()))
                    {
                        colort.EndsAt = colort.EndsAt.AddMonths(1);
                    }
                    else
                    {
                        await _profile.RomoveUserColorAsync(user, color);
                        colort.EndsAt = _time.Now().AddMonths(1);
                    }

                    colort.BValue = true;
                    if (!await _profile.SetUserColorAsync(user, gConfig.MuteRole, color))
                    {
                        await ReplyAsync("", embed: $"Coś poszło nie tak!".ToEmbedMessage(EMType.Error).Build());
                        return;
                    }

                    if (!hasNitro)
                    {
                        if (currency == SCurrency.Tc)
                        {
                            botuser.TcCnt -= color.Price(currency);
                        }
                        else
                        {
                            botuser.ScCnt -= color.Price(currency);
                        }
                    }
                }

                await db.SaveChangesAsync();

                QueryCacheManager.ExpireTag(new string[] { $"user-{botuser.Id}", "users" });

                await ReplyAsync("", embed: $"{user.Mention} wykupił kolor!".ToEmbedMessage(EMType.Success).Build());
            }
        }
    }
}
