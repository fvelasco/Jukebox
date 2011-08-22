using System;
using System.Linq;
using System.Collections.Generic;

using SimpleMvvmToolkit;

namespace Jukebox.SilverlightApplication
{
    // Model classes derive from ModelBase<T> to support two-way
    // data binding in property setters.

    public class Customer : ModelBase<Customer>
    {
        private int customerId;
        public int CustomerId
        {
            get { return customerId; }
            set
            {
                customerId = value;
                NotifyPropertyChanged(m => m.CustomerId);
            }
        }

        private string customerName;
        public string CustomerName
        {
            get { return customerName; }
            set
            {
                customerName = value;
                NotifyPropertyChanged(m => m.CustomerName);
            }
        }

        private string city;
        public string City
        {
            get { return city; }
            set
            {
                city = value;
                NotifyPropertyChanged(m => m.City);
            }
        }
    }
}
