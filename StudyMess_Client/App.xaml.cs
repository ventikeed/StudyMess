using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudyMess_Client.Services;
using StudyMess_Client.Views;
using static StudyMess_Client.Services.UrlStorage;

namespace StudyMess_Client
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            if (TokenStorage.IsAuthenticated)
            {
                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            else
            {
                var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddSingleton<ChatService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<GroupService>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<AdminService>();
            services.AddSingleton<ChatHubService>(provider =>
                new ChatHubService($"{BaseUrl}/chathub"));

            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<CreateChatWindow>();
            services.AddTransient<RegisterWindow>();
        }
    }
}
