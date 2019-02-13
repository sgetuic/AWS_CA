namespace UIC.Framework.Interfaces.Edm
{
    /// <summary>
    /// Extending the <see cref="UIC.Framework.Interfaces.Edm.EmbeddedDriverModule"/> Interface to allow for better testing of the EDMs.
    /// </summary>
    public interface InterfaceEmbeddedDriverModule : EmbeddedDriverModule
    {
        /// <summary>
        /// Shows the status of the Eapi in a string
        /// </summary>
        /// <returns>A defined string</returns>
        string GetEdmHealthStatus();

        /// <summary>
        /// A Test for EDMs to enable test to see if it's still running
        /// Example:
        /// <code>if((x&-x) == x)</code>
        /// Tests if x is 2 pow x
        /// </summary>
        /// <param name="x">Integer to be tested</param>
        /// <returns>true if its running, false if the nummber was wrong</returns>
        bool IsRunning(int x);
    }
}
