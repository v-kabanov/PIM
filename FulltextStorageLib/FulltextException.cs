// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-26
// Comment  
// **********************************************************************************************/
// 

using System;

namespace FulltextStorageLib
{
    public class FulltextException : Exception
    {
        public FulltextException()
        {
        }

        public FulltextException(string message) : base(message)
        {
        }

        public FulltextException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}