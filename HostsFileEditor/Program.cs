using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace HostsFileEditor
{
    /// <summary>
    /// A helper program enabling one to add and delete entries from the host file.
    /// </summary>
    class Program
    {
        const string HOSTS_FILE_PATH = @"C:\Windows\System32\drivers\etc\hosts";
        const string DEFAULT_IP_ADDRESS = "127.0.0.1";

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">arguments passed to the command</param>
        static void Main(string[] args)
        {
            string errorMessage = "";
            if (validateArgs(args, ref errorMessage))
            {
                // The validateArgs function should have ensured the args are safe to use now
                string action = args[0];
                switch (action)
                {
                    case "-a":
                        if (args.Length == 2) {
                            addIpHostnamePair(args[1]);
                        }
                        else if (args.Length == 3) {
                            addIpHostnamePair(args[1], args[2]);
                        }
                        break;
                    case "-d":
                        deleteHostName(args[1]);
                        break;
                    case "-s":
                        showEntries();
                        break;
                }
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }

        /// <summary>
        /// Validates the arguments. If anything is wrong, show usage message.
        /// </summary>
        /// <param name="args">The arguments given</param>
        /// <param name="error">An error message</param>
        /// <returns>True if arguments are valid.</returns>
        private static bool validateArgs(string[] args, ref string error)
        {
            // If using -a, should be 3 arguments, if -d then two arguments otherwise it's invalid
            if (
                args.Length < 1 || 
                (args[0] == "-a" && args.Length < 2 || args.Length > 3) ||
                (args[0] == "-d" && args.Length != 2) || 
                (args[0] == "-s" && args.Length != 1) ||
                args.Length > 3)
            {
                error = "hostsedit adds, deletes and shows hosts file entries. " + Environment.NewLine +
                    "Usage should be: hostsedit -a <IP Address> <hostname> (Adds an entry) OR hostsedit -d <hostname> (deletes an entry) OR hostsedit -s (shows all entries)";
                return false;
            }
            if (args[0] == "-a" && args.Length == 2)
            {
                return validateAdd(args[1], ref error);
            }
            else if (args[0] == "-a" && args.Length == 3)
            {
                return validateAdd(args[1], args[2], ref error);
            }
            error = "";
            return true;
        }

        /// <summary>
        /// Validates the arguments given for an addition to the hosts file.
        /// </summary>
        /// <param name="hostname">The host name being added.</param>
        /// <param name="error">Holds error messages.</param>
        /// <returns>True if the arguments are valid.</returns>
        private static bool validateAdd(string hostname, ref string error)
        {
            return validateAdd(hostname, "127.0.0.1", ref error);
        }

        /// <summary>
        /// Validates the arguments given for an addition to the hosts file.
        /// </summary>
        /// <param name="hostname">The host name being added.</param>
        /// <param name="ipAddress">The IP address being added.</param>
        /// <param name="error">Holds error messages.</param>
        /// <returns>True if the arguments are valid.</returns>
        private static bool validateAdd(string hostname, string ipAddress, ref string error)
        {
            Regex ipAddressRegex = new Regex("^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
            Regex hostnameRegex = new Regex("^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])$");
            if (!ipAddressRegex.Match(ipAddress).Success)
            {
                error = "Given IP address does not seem to be valid";
                return false;
            }
            if (!hostnameRegex.Match(hostname).Success)
            {
                error = "Given hostname does not seem to be valid";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds a line to the hosts file containing the IP address and hostname.
        /// </summary>
        /// <param name="hostname">The hostname being added.</param>
        /// <param name="iPAddress">The IP address being added.</param>
        private static void addIpHostnamePair(string hostname, string iPAddress = "127.0.0.1")
        {
            string hosts = File.ReadAllText(HOSTS_FILE_PATH);
            string pattern = string.Format(@"\s\b{0}\b[\s\n]", Regex.Escape(hostname));
            Regex targetHostnameRegex = new Regex(pattern);
            if (targetHostnameRegex.IsMatch(hosts))
            {
                Console.WriteLine("Not updating hosts file as the hostname {0} seems to already exist in the hosts file. Please try deleting it first.", hostname);
            }
            else
            {
                int hLength = hosts.Length;
                string lastTwoChar = new string(new char[] { hosts[hLength - 2], hosts[hLength - 1] });
                bool needNewLine = lastTwoChar != Environment.NewLine;
                try
                {
                    File.AppendAllText(HOSTS_FILE_PATH, (needNewLine ? Environment.NewLine : "") + iPAddress + "\t\t" + hostname + Environment.NewLine);
                    Console.WriteLine(String.Format("{0} {1} has been added to the hosts file", iPAddress, hostname));
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Insufficient permissions to edit the host file. Try running in a console window with administrator permissions.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Could not delete hostname: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Deletes the line in the hosts file containing the given host name.
        /// </summary>
        /// <param name="hostname"></param>
        private static void deleteHostName(string hostname)
        {
            string[] hostsFileContents = File.ReadAllLines(HOSTS_FILE_PATH);
            string newHostsFileContents = "";
            string pattern = string.Format(@"\s\b{0}\b\s*$", Regex.Escape(hostname));
            Regex targetHostnameRegex = new Regex(pattern);
            bool found = false;
            bool lastLineIsComment = false;
            foreach (string line in hostsFileContents)
            {
                if (!targetHostnameRegex.Match(line).Success && (line.Trim() != String.Empty || lastLineIsComment))
                {
                    newHostsFileContents += line + Environment.NewLine;
                }
                else
                {
                    found = true;
                }
                lastLineIsComment = line.Length > 0 && line.IndexOf("#") >= 0;
            }
            if (!found)
            {
                Console.WriteLine("Hostname {0} was not deleted because it was not found in the hosts file.", hostname);
            }
            else
            {
                try
                {
                    File.WriteAllText(HOSTS_FILE_PATH, newHostsFileContents);
                    Console.WriteLine("Hostname {0} has been deleted from the hosts file.", hostname);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Insufficient permissions to edit the host file. Try running in a console window with administrator permissions.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Could not delete hostname: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Backs up the hosts file. Current unused.
        /// </summary>
        private static void backupHostsFile()
        {
            string backupFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\hosts.bak";
            File.Copy(HOSTS_FILE_PATH, backupFilePath, true);
            Console.WriteLine("Hosts file backed up to: {0}", backupFilePath);
        }

        /// <summary>
        /// Shows the current entries in the hosts file. Ignores comments.
        /// </summary>
        private static void showEntries()
        {
            string[] hostsFileContents = File.ReadAllLines(HOSTS_FILE_PATH);
            foreach (string line in hostsFileContents)
            {
                if (line.Length > 1 && line.Trim() != String.Empty && line.ToCharArray()[0] != '#') {
                    Console.WriteLine(line);
                }
            }
        }
    }
}
