
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using RegistrationProject.Data;
using RegistrationProject.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationProject.Services.DiscordBot
{
    public class Commands : BaseCommandModule
    {

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {


            await ctx.Channel.SendMessageAsync($"Pong! `{ctx.Client.Ping}ms` :3");

        }
        [Command("whois")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task WhoIs(CommandContext ctx, DiscordMember dMember)
        {

            using MemberDbContext db = new(BotService.Config.ConnectionString);
            Member member = db.Members.SingleOrDefault(x => x.DiscordId == dMember.Id);
            DiscordEmbedBuilder embedBuilder = new() { Title = "WhoIs", Description = "All relevant information about this member can be found below" };
            embedBuilder.AddField("Member Id", member.Id.ToString());
            embedBuilder.AddField("Member Name", member.Name);
            embedBuilder.AddField("Class", member.Class);
            embedBuilder.AddField("Admission Number", member.AdmissionNumber);
            embedBuilder.AddField("Registered At", member.RegisteredAt.ToString());
            embedBuilder.AddField("WhatsApp", member.WhatsApp ?? "Not found");
            embedBuilder.AddField("IsApproved", member.IsApproved.ToString());
            await ctx.Channel.SendMessageAsync(embed: embedBuilder);

        }
        [Command("register")]
        public async Task Register(CommandContext ctx)
        {
            DSharpPlus.Interactivity.InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            await ctx.Member.SendMessageAsync("Hi there! Please provide the following information to become a member of the Royal College Computer Society. (Just type it in)");

            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder() { Title = "Please check your DMs", Description = "I have sent you a direct message. If you did not get it, please make sure you're accepting direct messages. You can adjust your DM setting in User Settings>Privacy & Safety>Toggle “Allow DMs from server members” this lets anyone in your servers DM you!" });


            await ctx.Member.SendMessageAsync("Your Full Name?");
            string name = (await interactivity.WaitForMessageAsync(x => x.Channel.IsPrivate && x.Author.Id == ctx.Member.Id, TimeSpan.FromMinutes(15))).Result.Content;
            if (string.IsNullOrEmpty(name))
            {
                await ctx.Member.SendMessageAsync("Name cannot be empty. Please use the command again!");
                return;

            }
            await ctx.Member.SendMessageAsync("Your Class? (ex: 11-J)");
            string studentClass = (await interactivity.WaitForMessageAsync(x => x.Channel.IsPrivate && x.Author.Id == ctx.Member.Id, TimeSpan.FromMinutes(15))).Result.Content;
            if(string.IsNullOrEmpty(studentClass) || studentClass.Length != 4 || studentClass[2] != '-')
            {
                await ctx.Member.SendMessageAsync("The class cannot be empty nor can it be not in the correct format (An example for the correct format :- ``11-J``). Please use the command again!");
                return;

            }
            await ctx.Member.SendMessageAsync("Your School Admission Number? (ex: 4509/8793)");
            string admissionNumber = (await interactivity.WaitForMessageAsync(x => x.Channel.IsPrivate && x.Author.Id == ctx.Member.Id, TimeSpan.FromMinutes(15))).Result.Content;
            if (string.IsNullOrEmpty(admissionNumber) || admissionNumber.Length != 9 || !new char[] { '/', '-' }.Contains(admissionNumber[4]))
            {
                await ctx.Member.SendMessageAsync("The admission number cannot be empty nor can it be not in the correct format (An example for the correct format :- ``4509/8793``). Please use the command again!");
                return;

            }
            await ctx.Member.SendMessageAsync("WhatsApp Number? (Optional) (type `null` if you do not wish to provide the number)");
            string whatsapp = (await interactivity.WaitForMessageAsync(x => x.Channel.IsPrivate && x.Author.Id == ctx.Member.Id, TimeSpan.FromMinutes(15))).Result.Content;
            await ctx.Member.SendMessageAsync("Your request is pending approval. It may take upto a day for us to review your application.");

            Member member = new(false)
            {
                Name = name.ToUpper(),
                Class = studentClass.ToUpper(),
                AdmissionNumber = admissionNumber,
                DiscordId = ctx.Member.Id,
                WhatsApp = whatsapp.ToLower() == "null" ? null : whatsapp
            };
            DiscordChannel requestChannel = ctx.Guild.GetChannel(BotService.Config.RequestsChannel);
            DiscordEmbedBuilder requestBuilder = new()
            {
                Title = "A student just registered!",
                Description = $"Please check if the provided information is correct. User: {ctx.Member.Mention}"
            };
            requestBuilder.AddField("Full Name", member.Name);
            requestBuilder.AddField("Class", member.Class);
            requestBuilder.AddField("Admission Number", member.AdmissionNumber);
            requestBuilder.AddField("WhatsApp", member.WhatsApp ?? "Not provided");
            requestBuilder.AddField("Registered At", member.RegisteredAt.ToString());


            DiscordMessage requestMessage = await requestChannel.SendMessageAsync(embed: requestBuilder);
            DiscordEmoji checkMarkEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            DiscordEmoji negativeCheckMarkEmoji = DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:");

            await requestMessage.CreateReactionAsync(checkMarkEmoji);
            await requestMessage.CreateReactionAsync(negativeCheckMarkEmoji);

            try
            {
                DSharpPlus.Interactivity.InteractivityResult<DSharpPlus.EventArgs.MessageReactionAddEventArgs> resultEmoji = await interactivity.WaitForReactionAsync(x => x.Message.Id == requestMessage.Id && ((DiscordMember)x.User).Roles.Any(x => x.Name == "Moderator" || x.Name == "Board Member"), TimeSpan.FromDays(1));
                if (resultEmoji.Result.Emoji == negativeCheckMarkEmoji)
                {
                    await requestChannel.SendMessageAsync("Not Approved!");

                }
                else if (resultEmoji.Result.Emoji == checkMarkEmoji)
                {
                    member.IsApproved = true;
                    using MemberDbContext db = new(BotService.Config.ConnectionString);
                    db.Database.EnsureCreated();
                    if (db.Members.SingleOrDefault(x => x.Name == member.Name || x.DiscordId == member.DiscordId) is null)
                    {
                        db.Add(member);
                        db.SaveChanges();
                        DiscordRole role = ctx.Guild.GetRole(BotService.Config.MemberRoleId);
                        await ctx.Member.GrantRoleAsync(role);
                        await requestChannel.SendMessageAsync($"Approved!");
                        await ctx.Member.SendMessageAsync("You have been approved as a member!");
                    }
                    else
                    {
                        DiscordRole role = ctx.Guild.GetRole(BotService.Config.MemberRoleId);
                        await ctx.Member.GrantRoleAsync(role);
                        await requestChannel.SendMessageAsync($"User was already registered!");
                        await ctx.Member.SendMessageAsync("You were already registered!");
                    }

                }
            }
            catch (Exception)
            {
                using MemberDbContext db = new(BotService.Config.ConnectionString);
                db.Database.EnsureCreated();
                if (db.Members.SingleOrDefault(x => x.Name == member.Name || x.DiscordId == member.DiscordId) is null)
                {
                    db.Add(member);
                    db.SaveChanges();
                    await requestChannel.SendMessageAsync($"Member with Name: {member.Name} was registered but not approved");
                    await ctx.Member.SendMessageAsync("You were registered as a member but was not approved by anyone yet. We'll get back to you when we approve");
                }
                else
                {
                    DiscordRole role = ctx.Guild.GetRole(BotService.Config.MemberRoleId);
                    await ctx.Member.GrantRoleAsync(role);
                    await requestChannel.SendMessageAsync($"User was already registered!");
                    await ctx.Member.SendMessageAsync("You were already registered!");
                }

            }




        }
    }
}
