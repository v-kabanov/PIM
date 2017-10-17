using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using PimIdentity.Models;

namespace PimIdentity
{
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }

        public static void Configure()
        {
            var builder = new ContainerBuilder();

            var container = builder.Build();

            IServiceLocator autofacServiceLocator = new AutofacServiceLocator(container) as IServiceLocator;
            ServiceLocator.SetLocatorProvider(() => autofacServiceLocator);
        }
    }
}