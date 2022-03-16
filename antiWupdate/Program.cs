using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace antiWupdate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Do you want to disable Windows Updates? (y/n)>");
            var resp = Console.ReadLine();
            switch (resp.ToLower())
            {
                case "y":
                    {
                        StopUpdateService();
                        WriteRegKeys();
                        Console.Write("Reboot PC? (y/n)");
                        var rs1 = Console.ReadLine();
                        switch (rs1.ToLower())
                        {
                            case "y":
                                {
                                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                                    p.StartInfo.FileName = "shutdown";
                                    p.StartInfo.Arguments = "/a";
                                    p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                    p.Start();
                                    p.WaitForExit();
                                    p.StartInfo.Arguments = "/r /t 00";
                                    p.Start();
                                    break;
                                }
                            case "n":
                                {
                                    Environment.Exit(0);
                                    break;
                                }
                        }
                        break;
                    }
                case "n":
                    {
                        Environment.Exit(0);
                        break;
                    }
            }
        }
        private static void StopUpdateService()
        {
            ServiceController sc = new ServiceController("wuauserv");
            try
            {
                Console.WriteLine("Stopping Windows Update Service...");
                if (sc != null && sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }

                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                sc.Close();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Service stopped.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

            }
            try
            {
                Console.WriteLine("Changing StartType");
                ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Disabled);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Service StartType Changed");
                Console.ForegroundColor = ConsoleColor.White;
               
            }catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        private static void WriteRegKeys()
        {
            Console.WriteLine("Writing Regkeys...");
            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AutoUpdate");
                key.SetValue("NoAutoUpdate", 1, RegistryValueKind.DWord);
                key.SetValue("AUOptions", 1, RegistryValueKind.DWord);
                key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU");
                key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AutoUpdate");
                key.SetValue("NoAutoUpdate", 1, RegistryValueKind.DWord);
                key.SetValue("AUOptions", 1, RegistryValueKind.DWord);
                key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\WOW6432Node\Policies\Microsoft\Windows\WindowsUpdate\AU");
                key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AutoUpdate");
                key.SetValue("NoAutoUpdate", 1, RegistryValueKind.DWord);
                key.SetValue("AUOptions", 1, RegistryValueKind.DWord);
                key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU");
                key.SetValue("NoAutoUpdate", 1, RegistryValueKind.DWord);
                key.SetValue("AUOptions", 1, RegistryValueKind.DWord);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Keys added!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }


        }
    }
    public static class ServiceHelper
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            UInt32 nServiceType,
            UInt32 nStartType,
            UInt32 nErrorControl,
            String lpBinaryPathName,
            String lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In] char[] lpDependencies,
            String lpServiceStartName,
            String lpPassword,
            String lpDisplayName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(
            IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(
            string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                Console.WriteLine("Open Service Manager Error");
            }

            var serviceHandle = OpenService(
                scManagerHandle,
                svc.ServiceName,
                SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Open Service Error");
            }

            var result = ChangeServiceConfig(
                serviceHandle,
                SERVICE_NO_CHANGE,
                (uint)mode,
                SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                Console.WriteLine("Could not change service start type: "
                    + win32Exception.Message);
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }
    } 
}
