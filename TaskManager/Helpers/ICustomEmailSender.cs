namespace TaskManager.Helpers
{
    public interface ICustomEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
