// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2016-07-09
// Comment  
// **********************************************************************************************/
// 
using System.Reflection;
using GalaSoft.MvvmLight;
using log4net;

namespace AuNote.ViewModel
{
    public class NoteViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public NoteViewModel()
        {
            if (IsInDesignMode)
            {
            }
            else
            {
            }
        }
    }
}