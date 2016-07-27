using System;
using System.Text;
using PokemonGo.RocketAPI.Logging;
using System.Windows.Forms;

namespace PokemonGo.RocketAPI.Console
{
    public class FormLogger : ILogger
    {
        private readonly LogLevel _maxLogLevel;
        private readonly RichTextBox _textbox;

        /// <summary>
        ///     To create a ConsoleLogger, we must define a maximum log level.
        ///     All levels above won't be logged.
        /// </summary>
        /// <param name="maxLogLevel"></param>
        public FormLogger(RichTextBox p_TextBox, LogLevel maxLogLevel)
        {
            _maxLogLevel = maxLogLevel;
            _textbox = p_TextBox;
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

            var colorName = color.ToString();

            if (colorName == "DarkYellow")
            {
                colorName = "Goldenrod";
            }

            var systemColor = System.Drawing.Color.FromName(colorName);

            if (_textbox.InvokeRequired)
            {
                // after we've done all the processing, 
                _textbox.Invoke(new MethodInvoker(delegate {
                    // load the control with the appropriate data

                    switch (level)
            {
                case LogLevel.Error:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Red : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (ERROR) {message}");

                    break;
                case LogLevel.Warning:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Gold : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (ATTENTION) {message}");
                    break;
                case LogLevel.Info:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.DarkCyan : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (INFO) {message}");
                    break;
                case LogLevel.Pokestop:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Cyan : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (POKESTOP) {message}");
                    break;
                case LogLevel.Farming:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Magenta : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (FARMING) {message}");
                    break;
                case LogLevel.Recycling:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.DarkMagenta : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (RECYCLING) {message}");
                    break;
                case LogLevel.Caught:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Green : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (PKMN) {message}");
                    break;
                case LogLevel.Transfer:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.DarkGreen : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (TRANSFERED) {message}");
                    break;
                case LogLevel.Evolve:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Yellow : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (EVOLVED) {message}");
                    break;
                case LogLevel.Berry:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Goldenrod : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (BERRY) {message}");
                    break;
                case LogLevel.Egg:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Goldenrod : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (EGG) {message}");
                    break;
                case LogLevel.Debug:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.Gray : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (DEBUG) {message}");
                    break;
                default:
                    _textbox.SelectionColor = (systemColor == System.Drawing.Color.Black ? System.Drawing.Color.White : systemColor);
                    _textbox.AppendText($"{Environment.NewLine}[{DateTime.Now.ToString("HH:mm:ss")}] (ERROR) {message}");
                    break;
            }

                    _textbox.ScrollToCaret();

                }));
                return;
            }

            
        }
    }
}
