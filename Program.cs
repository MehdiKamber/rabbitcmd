using System;
using System.Diagnostics;
using System.IO;
using LibGit2Sharp;
using System.Net;
using System.Net.NetworkInformation;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "RabbitCMD";

        while (true)
        {
            Console.Write("> ");
            string command = Console.ReadLine()?.Trim().ToLower();

            if (command.StartsWith("rbt i "))
            {
                string[] parts = command.Split(' ');

                if (parts.Length == 3)
                {
                    string gitUrl = parts[2];

                    if (Uri.IsWellFormedUriString(gitUrl, UriKind.Absolute) && 
                        (gitUrl.StartsWith("http://") || gitUrl.StartsWith("https://")))
                    {
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string repoName = Path.GetFileNameWithoutExtension(new Uri(gitUrl).LocalPath);
                        string clonePath = Path.Combine(desktopPath, repoName);

                        if (Directory.Exists(clonePath))
                        {
                            Console.WriteLine($"'{repoName}' exists.");
                        }
                        else
                        {
                            Directory.CreateDirectory(clonePath);

                            try
                            {
                                Console.WriteLine($"Cloning from {gitUrl}...");
                                Repository.Clone(gitUrl, clonePath);
                                Console.WriteLine("Cloned successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Clone error: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid URL. Use: rbt i <https://github.com/username/repo>");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid format. Use: rbt i <github_link>");
                }
            }
            else if (command == "help")
            {
                Console.WriteLine("╔════════════════════════════════════╗");
                Console.WriteLine("║          Available Commands        ║");
                Console.WriteLine("╠════════════════════════════════════╣");
                Console.WriteLine("║ rbt i <github_url> - Clone repo    ║");
                Console.WriteLine("║ rbt ip - Show IP                   ║");
                Console.WriteLine("║ rbt chk <domain> - Check domain IP ║");
                Console.WriteLine("║ rbt prcs - List processes          ║");
                Console.WriteLine("║ exit - Exit                        ║");
                Console.WriteLine("╚════════════════════════════════════╝");
            }
            else if (command == "rbt prcs")
            {
                ListRunningProcesses();
            }
            else if (command == "rbt ip")
            {
                string ipAddress = GetLocalIPAddress();
                Console.WriteLine($"IP: {ipAddress}");
            }
            else if (command.StartsWith("rbt chk "))
            {
                string[] parts = command.Split(' ', 3);

                if (parts.Length == 3)
                {
                    string domain = parts[2];

                    if (Uri.IsWellFormedUriString(domain, UriKind.Absolute))
                    {
                        try
                        {
                            Uri uri = new Uri(domain.StartsWith("http://") || domain.StartsWith("https://") ? domain : "http://" + domain);
                            string host = uri.Host;

                            var hostEntry = Dns.GetHostEntry(host);
                            string ipAddress = hostEntry.AddressList.Length > 0 ? hostEntry.AddressList[0].ToString() : "No IP";
                            Console.WriteLine($"DMN: {host} \nIP: {ipAddress}");
                        }
                        catch (UriFormatException)
                        {
                            Console.WriteLine("Invalid domain format.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Domain error: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid domain. Use: rbt chk http://example.com");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid format. Use: rbt chk <domain>");
                }
            }
            else if (command == "cls")
            {
                Console.Clear();
            }
            else if (command == "exit")
            {
                break;
            }
            else
            {
                Console.WriteLine("Unknown command.");
            }
        }
    }

    static void ListRunningProcesses()
    {
        try
        {
            Process[] processes = Process.GetProcesses();
            bool accessDeniedPrinted = false;

            foreach (var process in processes)
            {
                try
                {
                    string fileName = process.MainModule?.FileName;
                    if (fileName != null)
                    {
                        string extension = Path.GetExtension(fileName);
                        string name = Path.GetFileName(fileName);
                        Console.WriteLine($"ID: {process.Id}, Name: {name}, Ext: {extension}");
                    }
                    else
                    {
                        Console.WriteLine($"ID: {process.Id}, Name: {process.ProcessName}");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Unable to enumerate") || ex.Message.Contains("Access is denied"))
                    {
                        if (!accessDeniedPrinted)
                        {
                            Console.WriteLine("Access denied for some processes.");
                            accessDeniedPrinted = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Process error: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Processes error: {ex.Message}");
        }
    }

    static string GetLocalIPAddress()
    {
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            foreach (var ipAddress in networkInterface.GetIPProperties().UnicastAddresses)
            {
                if (ipAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ipAddress.Address.ToString();
                }
            }
        }
        return "No IP found.";
    }
}