using System;
using System.ComponentModel;

namespace AssetBuilder.Controls
{
    public class QuestionAnswer : INotifyPropertyChanged
    {
        private int _QuestionID;
        public int QuestionID { get { return _QuestionID; } set { _QuestionID = value; NotifyPropertyChanged("QuestionID"); } }
        private int _AnswerID;
        public int AnswerID { get { return _AnswerID; } set { _AnswerID = value; NotifyPropertyChanged("AnswerID"); } }
        private int _ConclusionID;
        public int ConclusionID { get { return _ConclusionID; } set { _ConclusionID = value; NotifyPropertyChanged("ConclusionID"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class QuestionAnswerValue : INotifyPropertyChanged
    {
        private int _QuestionID;
        public int QuestionID { get { return _QuestionID; } set { _QuestionID = value; NotifyPropertyChanged("QuestionID"); } }
        private int _AnswerID;
        public int AnswerID { get { return _AnswerID; } set { _AnswerID = value; NotifyPropertyChanged("AnswerID"); } }
        private string _Value;
        public string Value { get { return _Value; } set { _Value = value; NotifyPropertyChanged("Value"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
