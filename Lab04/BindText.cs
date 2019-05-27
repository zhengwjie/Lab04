using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab04
{
    class BindText : INotifyPropertyChanged
    {
        public String show;
        public event PropertyChangedEventHandler PropertyChanged;
        public String Show
        {
            get { return show; }
            set
            {
                show = value;
                if(PropertyChanged!=null)
                PropertyChanged(this, new PropertyChangedEventArgs("Show"));
            }
        }
    }
}
