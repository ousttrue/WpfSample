using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LivetSample
{

    class MainWindowViewModel: Livet.ViewModel
    {
        Main m_model;
        public Main Model
        {
            get
            {
                if (m_model == null)
                {
                    m_model = new Main();
                }
                return m_model;
            }
        }

        Livet.ReadOnlyDispatcherCollection<MemberViewModel> m_members;
        public Livet.ReadOnlyDispatcherCollection<MemberViewModel> Members
        {
            get{
                if(m_members==null){
                    m_members = Livet.ViewModelHelper.CreateReadOnlyDispatcherCollection(Model.Members
                        , m =>new MemberViewModel(m, this)
                        , Livet.DispatcherHelper.UIDispatcher
                        );
                    CompositeDisposable.Add(m_members);
                }
                return m_members;
            }
        }

        Livet.Commands.ViewModelCommand m_editNewCommand;
        public ICommand EditNewCommand
        {
            get
            {
                if (m_editNewCommand == null)
                {
                    m_editNewCommand = new Livet.Commands.ViewModelCommand(() =>
                    {
                        Messenger.Raise(new Livet.Messaging.TransitionMessage(
                            new MemberViewModel(new Member(m_model), this), "Transition"));
                    });
                }
                return m_editNewCommand;
            }
        }

        Livet.Commands.ViewModelCommand m_removeCommand;
        public ICommand RemoveCommand
        {
            get
            {
                if(m_removeCommand==null)
                {
                    m_removeCommand = new Livet.Commands.ViewModelCommand(() =>
                    {
                        foreach(var m in Members.Where(m=>m.IsChecked).ToArray())
                        {
                            m.RemoveCommand.Execute(null);
                        }
                    });
                }
                return m_removeCommand;
            }
        }
    }
}
