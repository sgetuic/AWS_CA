namespace UIC.Framework.Interfaces.Eapi
{
    /// <summary>
    /// Sets the guidelines for Initialize and Uninitialize of Eapi to allow for better testing.
    /// </summary>
    public interface IEapiInitializer
    {
        /// <summary>
        /// Initialize the EApi
        /// </summary>
        void Init();
        /// <summary>
        /// Uninitialize the EApi
        /// </summary>
        void Dispose();

        // Flags

        /// <summary>
        /// Get the flag of the state if the EApi was successfully initializiert
        /// </summary>
        /// <returns>true if active, false if not</returns>
        bool GetInitDone();
        /// <summary>
        /// Get the flag of the state if the EApi was uninitializiert
        /// </summary>
        /// <returns>true if active, false if not</returns>
        bool GetDispose();
        /// <summary>
        /// Get the flag of the state if the EApi fialed to initializiert or uninitializiert
        /// </summary>
        /// <returns>true if failed, false if not</returns>
        bool GetInitFailed();
    }
}
