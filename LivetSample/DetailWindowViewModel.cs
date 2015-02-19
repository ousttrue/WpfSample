using Livet.Messaging.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LivetSample
{
    class MemberViewModel : Livet.ViewModel, IDataErrorInfo
    {
        Member m_member;
        MainWindowViewModel m_parent;

        Livet.EventListeners.WeakEvents.PropertyChangedWeakEventListener m_weak;

        Livet.Commands.ViewModelCommand m_removeCommand;
        public ICommand RemoveCommand
        {
            get
            {
                if (m_removeCommand == null)
                {
                    m_removeCommand = new Livet.Commands.ViewModelCommand(() =>
                    {
                        m_member.RemoveThisFromMainCollection();
                    });
                }
                return m_removeCommand;
            }
        }

        public MemberViewModel(Member member, MainWindowViewModel parent)
        {
            m_member = member;
            m_parent = parent;

            InitializeInput();

            // こうか？
            m_weak = new Livet.EventListeners.WeakEvents.PropertyChangedWeakEventListener(member,
                (o, e) =>
                {
                    RaisePropertyChanged(e.PropertyName);
                    if (e.PropertyName == "Birthday")
                    {
                        RaisePropertyChanged("Age");
                    }
                });
        }

        public String Name
        {
            get { return m_member.Name; }
            set { m_member.Name = value; }
        }
        public DateTime Birthday
        {
            get { return m_member.Birthday; }
            set { m_member.Birthday = value; }
        }
        public String Memo
        {
            get { return m_member.Memo; }
            set { m_member.Memo = value; }
        }
        public Int32 Age
        {
            get { return (DateTime.Now - Birthday).Days / 365; }
        }

        bool m_isChecked;
        public bool IsChecked
        {
            get { return m_isChecked; }
            set
            {
                if (m_isChecked == value) return;
                m_isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        #region Input
        String m_inputName;
        public String InputName
        {
            get { return m_inputName; }
            set
            {
                if (m_inputName == value) return;
                m_inputName = value;
                if (value==null || String.IsNullOrEmpty(value.Trim()))
                {
                    m_errors["InputName"] = "名前は必須です";
                }
                else
                {
                    m_errors["InputName"] = null;
                }
                RaisePropertyChanged("Error");
            }
        }

        String m_inputBirthday;
        public String InputBirthday
        {
            get{return m_inputBirthday;}
            set{
                if(m_inputBirthday==value)return;
                m_inputBirthday=value;
                DateTime inputDateTime;
                if(String.IsNullOrEmpty(m_inputBirthday)){
                    m_errors["InputBirthday"] = "生年月日は必須です";
                }
                else if(!DateTime.TryParse(m_inputBirthday, out inputDateTime)){
                    m_errors["InputBirthday"] = "年月日として不正な形式です";
                }
                else if(inputDateTime > DateTime.Now){
                    m_errors["InputBirthday"] = "未来の日付は指定できません";
                }
                else{
                    m_errors["InputBirthday"] = null;
                }
                RaisePropertyChanged("Error");
            }
        }

        public String InputMemo{get;set;}

        void InitializeInput()
        {
            InputName = m_member.Name;
            if (m_member.Birthday != DateTime.MinValue)
            {
                InputBirthday = m_member.Birthday.ToString("yyyy/MM/dd");
            }
            InputMemo = m_member.Memo;
            m_errors.Clear();
        }
        #endregion

        Livet.Commands.ViewModelCommand m_saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (m_saveCommand == null)
                {
                    m_saveCommand = new Livet.Commands.ViewModelCommand(() =>
                    {
                        Name = InputName;
                        Birthday = DateTime.Parse(InputBirthday);
                        Memo = InputMemo;

                        if (!m_member.IsIncludedInMainCollection())
                        {
                            m_member.AddThisToMainCollection();
                        }

                        // Viewに画面遷移用メッセージを送信しています。
                        // Viewは対応するメッセージキーを持つInteractionTransitionMessageTriggerでこのメッセージを受信します。
                        Messenger.Raise(new Livet.Messaging.Windows.WindowActionMessage(Livet.Messaging.Windows.WindowAction.Close, "Close"));
                    },
                    () =>
                    {
                        if(!String.IsNullOrEmpty(Error)){
                            return false;
                        }
                        if(String.IsNullOrEmpty(InputName)){
                            return false;
                        }
                        if(String.IsNullOrEmpty(InputBirthday)){
                            return false;
                        }
                        return true;
                    });

                    // CanExecuteの更新
                    PropertyChanged += (o, e) =>
                    {
                        if (e.PropertyName == "Error")
                        {
                            m_saveCommand.RaiseCanExecuteChanged();
                        }
                    };
                }
                
                return m_saveCommand;
            }
        }

        Livet.Commands.ViewModelCommand m_cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (m_cancelCommand == null)
                {
                    m_cancelCommand = new Livet.Commands.ViewModelCommand(() =>
                    {
                        // 入力情報初期化
                        InitializeInput();

                        // Viewに画面遷移用メッセージを送信しています。
                        // Viewは対応するメッセージキーを持つInteractionTransitionMessageTriggerでこのメッセージを受信します。
                        Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
                    });
                }
                return m_cancelCommand;
            }
        }

        #region IDataErrorInfo
        Dictionary<String, String> m_errors = new Dictionary<string, string>
        {
            {"InputName", null}, {"InputBirthday", null}
        };


        public string Error
        {
            get
            {
                var list = new List<String>();

                if (!String.IsNullOrEmpty(this["InputName"]))
                {
                    list.Add("名前");
                }
                if (!String.IsNullOrEmpty(this["InputBirthday"]))
                {
                    list.Add("生年月日");
                }
                if (!list.Any())
                {
                    return null;
                }

                return String.Join("・", list) + "が不正です";
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (m_errors.ContainsKey(columnName))
                {
                    return m_errors[columnName];
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
    }

}
