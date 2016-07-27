#region using directives

using System;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Exceptions;
using System.Windows.Forms;

#endregion

namespace PokemonGo.RocketAPI.Console
{
    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            

           // Task.Run(() =>
           // {
                var tracker = new Tracker();
                Application.Run(tracker);
                Application.DoEvents();
            //});

            return;

            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));

                Task.Run(() =>
                {
                    try
                    {
                        new Logic.Logic(new Settings(), null).Execute().Wait();
                    }
                    catch (PtcOfflineException)
                    {
                        Logger.Write("PTC Servers are probably down OR your credentials are wrong. Try google",
                            LogLevel.Error);
                        Logger.Write("Trying again in 20 seconds...");
                        Thread.Sleep(20000);
                        new Logic.Logic(new Settings(), null).Execute().Wait();
                    }
                    catch (AccountNotVerifiedException)
                    {
                        Logger.Write("Account not verified. - Exiting");
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"Unhandled exception: {ex}", LogLevel.Error);
                        new Logic.Logic(new Settings(), null).Execute().Wait();
                    }
                });
                System.Console.ReadLine();
            
        }
    }
}