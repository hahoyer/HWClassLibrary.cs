using System;

namespace HWClassLibrary.Helper
{
    /// <summary>
    /// Helper functions for numbers
    /// </summary>
    public static class Number
    {
        /// <summary>
        /// Gets the giga.
        /// </summary>
        /// <value>The giga.</value>
        /// [created 30.08.2006 22:24]
        public static int Giga { get { return 1000 * 1000 * 1000; } }
        /// <summary>
        /// Gets the mega.
        /// </summary>
        /// <value>The mega.</value>
        /// [created 30.08.2006 22:24]
        public static int Mega { get { return 1000 * 1000; } }
        /// <summary>
        /// Gets the kilo.
        /// </summary>
        /// <value>The kilo.</value>
        /// [created 30.08.2006 22:24]
        public static int Kilo { get { return 1000; } }

        /// <summary>
        /// Formats a number by use of Giga, Mega anf kilo symbols.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// [created 30.08.2006 22:24]
        public static string SmartDump(int i)
        {
            Single x = i;
            if (i < Kilo)
                return i.ToString();
            if (i < 10 * Kilo)
                return (x / Kilo).ToString("N2") + "k";
            if (i < 100 * Kilo)
                return (x / Kilo).ToString("N1") + "k";
            if (i < Mega)
                return (x / Kilo).ToString("N0") + "k";
            if (i < 10 * Mega)
                return (x / Mega).ToString("N2") + "M";
            if (i < 100 * Mega)
                return (x / Mega).ToString("N1") + "M";
            if (i < Giga)
                return (x / Mega).ToString("N0") + "M";
            if (i < 10 * Giga)
                return (x / Giga).ToString("N2") + "G";
            if (i < 100 * Giga)
                return (x / Giga).ToString("N1") + "G";

            throw new NotImplementedException();
        }
    }
}