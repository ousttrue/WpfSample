using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivetSample
{
    class Member : Livet.NotificationObject
    {
        Main m_parent;

        String m_name;
        public String Name
        {
            get { return m_name; }
            set
            {
                if (m_name == value) return;
                m_name = value;
                RaisePropertyChanged("Name");
            }
        }

        DateTime m_birthday;
        public DateTime Birthday
        {
            get { return m_birthday; }
            set
            {
                if (m_birthday == value) return;
                m_birthday = value;
                RaisePropertyChanged("BirthDay");
            }
        }

        String m_memo;
        public String Memo
        {
            get { return m_memo; }
            set
            {
                if (m_memo == value) return;
                m_memo = value;
                RaisePropertyChanged("Memo");
            }
        }

        public Member(Main parent)
        {
            m_parent = parent;
        }

        public bool IsIncludedInMainCollection()
        {
            return m_parent.Members.Contains(this);
        }

        public void AddThisToMainCollection()
        {
            m_parent.Members.Add(this);
        }

        public void RemoveThisFromMainCollection()
        {
            m_parent.Members.Remove(this);
        }
    }

    class Main : Livet.NotificationObject
    {
        ObservableCollection<Member> m_members;
        public ObservableCollection<Member> Members
        {
            get
            {
                if (m_members == null)
                {
                    m_members = new ObservableCollection<Member>
                    {
                        new Member(this){ Name="hoge", Birthday=new DateTime(1988, 12, 12), Memo="HogeHoge" },
                        new Member(this){ Name="fuga", Birthday=new DateTime(1999, 11, 11), Memo="FugaFuga" },
                    };
                }
                return m_members;
            }
        }
    }

}
