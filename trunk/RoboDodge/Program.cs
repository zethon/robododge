using System;

namespace RoboDodge
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (RDGame game = new RDGame())
            {
                game.Run();
            }
        }
    }
}

