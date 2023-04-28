using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
 using System.Text.Json;
using System.Text.Json.Serialization;
using KeepAwakeServer.Utils;
 
namespace KeepAwakeServer
{
    public class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddControllersWithViews();
             services.AddSingleton<KeepAwakeService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, KeepAwakeService _keepAwakeService)
        {

 
            app.UseRouting();
             app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                    
                endpoints.MapGet("/health", async context =>
                {
                    await context.Response.WriteAsync("Keep Awake Server is running.");
                });

                endpoints.MapPost("/awake", async context =>
                {
                    // Set awake time from request body
                    using StreamReader reader = new StreamReader (context.Request.Body, System.Text.Encoding.UTF8);
                    string body = await reader.ReadToEndAsync (); 
                    int minutes = int.Parse(body);
                    Console.WriteLine($"Keeping computer awake for {minutes} minutes...");

                    // Set awake state
                    EXECUTION_STATE state = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;

                    // Call SetThreadExecutionState periodically to maintain awake state
                    for (int i = 0; i < minutes * 6; i++)
                    {
                        SetThreadExecutionState(state);
                        await Task.Delay(10000); // call every 10 seconds to update the state
                    }

                    // Restore default execution state
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                    Console.WriteLine("Awake time expired.");

                    // Return response
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Computer kept awake.");
                });


                endpoints.MapGet("/isrunning", async context =>
                {
                    bool isRunning = _keepAwakeService.IsRunning();
                    await context.Response.WriteAsync(isRunning.ToString());
                });

                endpoints.MapGet("/timeleft", async context =>
                {
                    TimeSpan timeLeft = _keepAwakeService.TimeLeft();
                    await context.Response.WriteAsync(new ReadableTimespan().GetReadableTimespan(timeLeft) );
                });

                endpoints.MapPost("/start", async context =>
                {
                    // Read the minutes from the request body
                    int minutes = await JsonSerializer.DeserializeAsync<int>(context.Request.Body);

                    // Start the "keep awake" functionality
                    _keepAwakeService.Start(TimeSpan.FromMinutes(minutes));

                    await context.Response.WriteAsync("KeepAwake started.");
                });

                endpoints.MapPost("/stop", async context =>
                {
                    // Stop the "keep awake" functionality
                      _keepAwakeService.Stop();

                    await context.Response.WriteAsync("KeepAwake stopped.");
                });

            });



        }
    }
}