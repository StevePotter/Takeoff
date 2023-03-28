namespace Takeoff.Data
{
    public interface ISetting : ITypicalEntity
    {
        /// <summary>
        /// The name of the setting.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Value of the setting.
        /// </summary>
//Note: this is not a data property because the logic to set/get the data value is kinda funky
        object Value { get; set; }
    }
}