using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CzomPack.Network;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using RouteDataCollector.Services.Location;
using RouteDataCollector.Utils;
using Serilog;
#if IOS
using UIKit;
#else
using Android.Content;
using Android.OS;
#endif
namespace RouteDataCollector
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Status.Text = "";
            StopButton.IsChecked = false;
            StopButton.IsEnabled = false;
            StartButton.IsEnabled = true;
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            List<string> csv = new();
            Guid workerId = Guid.NewGuid();
            Status.Text = "Thread started";
            int waitSeconds = 1;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            //string csv_header = $"WorkerId;Name;Platform;Model;Idiom;VersionString;Type;Latitude;Longitude;CreateDate";

            BackgroundWorker bw = new();
            bw.DoWork += (sender, x) =>
            {
                while (true)
                {
                    Thread.Sleep(1 * 60 * 1000);

                    var compressed = $"{string.Join("\r\n", csv)}".Compress();
                    //Log.Verbose($"{csv_header}\r\n");
                    try
                    {
                        // Send data to server
                        string ua = $"RouteDataCollector.{DeviceInfo.Platform}/{Assembly.GetExecutingAssembly().GetName().Version}";
                        Log.Information(ua);
                        var rd = NetHandler.SendRequest("https://api-dev.hunluxstrada.hu/v1/location", RequestMethod.PUT, ua, "text/plain", $"{compressed}");
                        if (rd.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Log.Debug("Data sent");
                            csv.Clear();
                        }
                        else
                        {
                            Log.Debug($"Data send failed, response code: {rd.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("{ex}", ex);
                    }

                    if (StopButton.IsChecked) break;
                }
            };
            bw.RunWorkerAsync();

            BackgroundWorker bw2 = new();
            bw2.DoWork += async (sender, x) =>
            {
                while (true)
                {
                    try
                    {
                        GeoCoordinates loc = await LocationService.GetCurrentLocation(CancellationToken.None, 10);
                        Log.Verbose("Location obtained.");
                        csv.Add($"{workerId};{DeviceInfo.Name};{DeviceInfo.Platform};{DeviceInfo.Model};{DeviceInfo.Idiom};{DeviceInfo.VersionString};{DeviceInfo.DeviceType};{loc.Latitude};{loc.Longitude};{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                    }
                    catch (Exception ex)
                    {
                        Log.Error("{ex}", ex);
                    }
                    Thread.Sleep(waitSeconds * 1000);
                    if (StopButton.IsChecked) break;
                }
                try
                {
                    Status.Text = "Thread stopped";
                    StopButton.IsChecked = false;
                    StopButton.IsEnabled = false;
                    StartButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    Log.Error("{ex}", ex);
                }
            };
            bw2.RunWorkerAsync();
        }

        #region RunCodeInBackgroundMode
#if ANDROID
        public async Task RunCodeInBackgroundMode(Func<Task> action, string name = "MyBackgroundTaskName")
        {

            var powerManager = (PowerManager)Android.App.Application.Context.GetSystemService(Context.PowerService);
            var wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, name);
            //acquire a partial wakelock. This prevents the phone from going to sleep as long as it is not released.
            wakeLock.Acquire();
            var taskEnded = false;

            await Task.Factory.StartNew(async () =>
            {
                //here we run the actual code
                Console.WriteLine($"Background task '{name}' started");
                await action();
                Console.WriteLine($"Background task '{name}' finished");
                wakeLock.Release();
                taskEnded = true;
            });

            await Task.Factory.StartNew(async () =>
            {
                //just a method to keep track of how long the task runs
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (!taskEnded)
                {
                    Console.WriteLine($"Background '{name}' task with wakelock still running ({stopwatch.Elapsed.TotalSeconds} seconds)");
                    await Task.Delay(1000);
                }
                stopwatch.Stop();
            });
        }
#elif IOS
        public async Task RunCodeInBackgroundMode(Func<Task> action, string name = "MyBackgroundTaskName")
        {
            nint taskId = 0;
            var taskEnded = false;
            taskId = UIApplication.SharedApplication.BeginBackgroundTask(name, () =>
            {
                //when time is up and task has not finished, call this method to finish the task to prevent the app from being terminated
                Console.WriteLine($"Background task '{name}' got killed");
                taskEnded = true;
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });
            await Task.Factory.StartNew(async () =>
            {
                //here we run the actual task
                Console.WriteLine($"Background task '{name}' started");
                await action();
                taskEnded = true;
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
                Console.WriteLine($"Background task '{name}' finished");
            });

            await Task.Factory.StartNew(async () =>
            {
                //Just a method that logs how much time we have remaining. Usually a background task has around 180 seconds to complete. 
                while (!taskEnded)
                {
                    Console.WriteLine($"Background task '{name}' time remaining: {UIApplication.SharedApplication.BackgroundTimeRemaining}");
                    await Task.Delay(1000);
                }
            });
        }
#endif
        #endregion
    }
}
