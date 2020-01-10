namespace BfshaLibrary.Core
{
    /// <summary>
    /// Represents the common interface for <see cref="BfshaFile"/> data instances.
    /// </summary>
    public interface IResData
    {
        // ---- METHODS ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Loads raw data from the <paramref name="loader"/> data stream into instances.
        /// </summary>
        /// <param name="loader">The <see cref="BfshaFileLoader"/> to load data with.</param>
        void Load(BfshaFileLoader loader);
        
        /// <summary>
        /// Saves header data of the instance and queues referenced data in the given <paramref name="saver"/>.
        /// </summary>
        /// <param name="saver">The <see cref="BfshaFileSaver"/> to save headers and queue data with.</param>
        void Save(BfshaFileSaver saver);
    }
}
