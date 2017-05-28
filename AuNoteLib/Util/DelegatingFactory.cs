// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2017-01-12
// Comment		
// **********************************************************************************************/

using System;

namespace FulltextStorageLib.Util
{
    /// <summary>
    ///     Factory delegating work to lambda expression.
    /// </summary>
    /// <typeparam name="T">
    ///     Type of retrieved instance
    /// </typeparam>
    public class DelegatingFactory<T> : IFactory<T>
    {
        private readonly Func<T> _factory;

        public DelegatingFactory(Func<T> factory)
        {
            Check.DoRequireArgumentNotNull(factory, nameof(factory));

            _factory = factory;
        }

        public T Create()
        {
            return _factory.Invoke();
        }
    }
}