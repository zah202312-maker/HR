using System.Collections.Concurrent;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Emailing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Infrastructure;

/// <summary>Keeps sent mail in memory, so a test can read the link a real recipient would get.</summary>
public class CapturingEmailSender : EmailSenderBase
{
    private static readonly ConcurrentQueue<MailMessage> Sent = new();

    public CapturingEmailSender(ICurrentTenant currentTenant, IEmailSenderConfiguration configuration, IBackgroundJobManager backgroundJobManager)
        : base(currentTenant, configuration, backgroundJobManager)
    {
    }

    public static string? LastBodyTo(string address) =>
        Sent.LastOrDefault(x => x.To.Any(to => to.Address == address))?.Body;

    protected override Task SendEmailAsync(MailMessage mail)
    {
        Sent.Enqueue(mail);
        return Task.CompletedTask;
    }
}
