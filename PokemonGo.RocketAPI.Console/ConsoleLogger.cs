#region using directives

using System;
using System.Text;
using PokemonGo.RocketAPI.Logging;

#endregion

namespace PokemonGo.RocketAPI.Console
{
    /// <summary>
    ///     The ConsoleLogger is a simple logger which writes all logs to the Console.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly LogLevel _maxLogLevel;

        /// <summary>
        ///     To create a ConsoleLogger, we must define a maximum log level.
        ///     All levels above won't be logged.
        /// </summary>
        /// <param name="maxLogLevel"></param>
        public ConsoleLogger(LogLevel maxLogLevel)
        {
            _maxLogLevel = maxLogLevel;
        }

        /// <summary>
        ///     Log a specific message by LogLevel. Won't log if the LogLevel is greater than the maxLogLevel set.
        /// </summary>
        /// <param name="message">The message to log. The current time will be prepended.</param>
        /// <param name="level">Optional. Default <see cref="LogLevel.Info" />.</param>
        /// <param name="color">Optional. Default is auotmatic</param>
        public void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            //Remember to change to a font that supports your language, otherwise it'll still show as ???
            System.Console.OutputEncoding = Encoding.Unicode;
            if (level > _maxLogLevel)
                return;

            switch (level)
            {
                case LogLevel.Error:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.Red : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (ERROR) {message}");
                    break;
                case LogLevel.Warning:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.DarkYellow : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (ATTENTION) {message}");
                    break;
                case LogLevel.Info:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.DarkCyan : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (INFO) {message}");
                    break;
                case LogLevel.Pokestop:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.Cyan : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (POKESTOP) {message}");
                    break;
                case LogLevel.Farming:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.Magenta : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (FARMING) {message}");
                    break;
                case LogLevel.Recycling:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.DarkMagenta : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (RECYCLING) {message}");
                    break;
                case LogLevel.Caught:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.Green : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (PKMN) {message}");
                    break;
                case LogLevel.Transfer:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.DarkGreen : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (TRANSFERED) {message}");
                    break;
                case LogLevel.Evolve:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.Yellow : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (EVOLVED) {message}");
                    break;
                case LogLevel.Berry:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.DarkYellow : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (BERRY) {message}");
                    break;
                case LogLevel.Egg:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.DarkYellow : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (EGG) {message}");
                    break;
                case LogLevel.Debug:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.Gray : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (DEBUG) {message}");
                    break;
                default:
                    System.Console.ForegroundColor = (color == ConsoleColor.Black ? ConsoleColor.White : color);
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] (ERROR) {message}");
                    break;
            }
        }
    }
}