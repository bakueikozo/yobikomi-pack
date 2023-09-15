using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yobikomi_pack
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length==0)
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                if (args.Length >= 2)
                {
                    if (args[0].ToLower().EndsWith(".vox") && args[1].ToLower().EndsWith(".vox"))
                    {
                        string outfile = "output.bin";
                        if (args.Length == 3)
                        {
                            outfile = args[2];
                        }
                        Console.WriteLine("input ch1 =" + args[0] + "\nch2=" + args[1] + "\noutfile=" + outfile);

                        Form1._Pack(args[0], args[1], outfile);
                    }

                }
                else if (args.Length > 0)
                {
                    if (System.IO.Directory.Exists(args[0]))
                    {
                        Console.WriteLine("directory mode");
                        string[] files = System.IO.Directory.GetFiles(args[0], "*.vox");

                        Array.Sort(files);
                        foreach (var f in files)
                        {
                            Console.WriteLine("-> " + f);
                        }
                        if (files.Count() == 2)
                        {
                            string outfile = args[0] + ".bin";
                            Console.WriteLine("input ch1 =" + files[0] + "\nch2=" + files[1] + "\noutfile=" + outfile);
                            Form1._Pack(files[0], files[1], outfile);
                        }
                        else
                        {
                            Console.WriteLine("must be two .vox file");
                        }
                    }
                    else
                    {

                    }
                }

        }
    }
    }
}
