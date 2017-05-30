// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2015-09-17
// Comment		
// **********************************************************************************************/

namespace FulltextStorageLib.Util
{

    /// <summary>
    /// Defines interface of a stateful task
    /// </summary>
    /// <remarks>
    /// Provides an alternative to Action delegate to facilitate reusable processing components
    /// </remarks>
    public interface ITask
    {

        void Execute();
        string Name { get; }
    }
}
