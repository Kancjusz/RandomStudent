using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RandomStudent.Models
{
    [AddINotifyPropertyChangedInterface]
    internal class ClassModel : IQueryAttributable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<StudentModel> Students { get; set; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Id = int.Parse((string)query["ClassId"]);
            Name = (string)query["Name"];

            string s = (string)query["Students"];
            s = s.Replace("%7B", "{");
            s = s.Replace("%22", "\"");
            s = s.Replace("%7D", "}");

            Students = JsonSerializer.Deserialize<ObservableCollection<StudentModel>>(s);
        }
    }
}
