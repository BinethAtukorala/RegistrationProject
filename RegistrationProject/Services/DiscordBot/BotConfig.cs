namespace RegistrationProject.Services.DiscordBot
{




    public record BotConfig(string Token, string Prefix, string ConnectionString, ulong RequestsChannel, ulong MemberRoleId);



}
