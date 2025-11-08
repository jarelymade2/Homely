namespace StayGo.Services.AI
{
    public interface IChatAiService
    {
        Task<string> GetReplyAsync(string userMessage);
    }
}