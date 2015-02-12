using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace UriListModel
{
    public class UriListModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(String prop)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(prop));
            }
        }

        #region Items
        ObservableCollection<Uri> m_items;
        public ObservableCollection<Uri> Items
        {
            get
            {
                if (m_items == null)
                {
                    m_items = new ObservableCollection<Uri>();
                }
                return m_items;
            }
        }
        #endregion

        #region Bytes
        public Byte[] ToBytes()
        {
            return Items
                .Select(item => item.ToString() + "\n")
                .SelectMany(s => Encoding.UTF8.GetBytes(s))
                .ToArray()
                ;
            ;
        }

        public void FromBytes(Byte[] bytes)
        {
            Items.Clear();
            var str = Encoding.UTF8.GetString(bytes);
            foreach(var item in str.Split('\n')
                .Where(s => !String.IsNullOrEmpty(s))
                .Select(s => new Uri(s))
                )
            {
                Items.Add(item);
            }
        }
        #endregion
    }
}
