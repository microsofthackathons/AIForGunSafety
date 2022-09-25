using System;
using System.Threading.Tasks;
using AIForGunSafetyFunctionApp.Handlers;
using AIForGunSafetyFunctionApp.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AIForGunSafetyFunctionApp
{
    /// <summary>
    /// Function add to read social media posts.
    /// </summary>
    public class RecentSocialMediaPosts
    {
        [FunctionName("RecentSocialMediaPosts")]
        public async Task Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            SocialMediaPostHandler socialMediaPostHandler = new SocialMediaPostHandler();
            await socialMediaPostHandler.ConsumeRecentSocialMediaPosts();
            log.LogInformation($"Social media post pulled successfully");
        }
    }
}
