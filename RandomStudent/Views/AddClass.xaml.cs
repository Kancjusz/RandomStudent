using RandomStudent.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace RandomStudent.Views;

public partial class AddClass : ContentPage
{
	public AddClass()
	{
		InitializeComponent();
    }

    private async void AddClass_Clicked(object sender, EventArgs e)
    {
		ClassModel model = (ClassModel)BindingContext;

        string path = Path.Combine(FileSystem.AppDataDirectory, "classes.txt");
        XDocument xmlDocument = XDocument.Load(path);

        model.Id = xmlDocument.Element("Classes").Elements().Count();
        model.Students ??= new ObservableCollection<StudentModel>();

        model.Students = new ObservableCollection<StudentModel>(model.Students.OrderBy(x => x.Surname).Select((sm,i)=> {
            sm.Id = i+1;
            return sm;
        }));
        model.Students = new ObservableCollection<StudentModel>(model.Students.OrderBy(x => x.Id));

       xmlDocument.Element("Classes").Add(new XElement(
                "Class",
                new XElement("Id",model.Id),
                new XElement("Name",model.Name),
                new XElement("Students",model.Students.Select(s => new XElement(
                    "Student",
                    new XAttribute("PickedCountdown",0),
                    new XElement("Id",s.Id),
                    new XElement("Name",s.Name),
                    new XElement("Surname",s.Surname),
                    new XElement("IsPresent", s.IsPresent)
                )))
            )
        );

        xmlDocument.Save(path);

        await Shell.Current.GoToAsync($"{nameof(PickClass)}");
    }

    private void AddStudent_Clicked(object sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;

        model.Students ??= new ObservableCollection<StudentModel>();
        int length = model.Students.Count;
        model.Students.Add(new StudentModel() { Id=length+1, Name="",Surname="", IsPresent=true, PickedCountdown=0});

        StudentListFrame.IsVisible = true;

        BindingContext = model;
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"../");
    }
}