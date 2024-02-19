using RandomStudent.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

namespace RandomStudent.Views;

public partial class EditClass : ContentPage
{
    private int classIndex = 0;

    public EditClass()
	{
		InitializeComponent();
        editStudentList.Loaded += EditStudentList_Loaded;
    }

    ~EditClass()
    {
        editStudentList.Loaded -= EditStudentList_Loaded;
    }

    private void EditStudentList_Loaded(object? sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;
        if (model.Students.Count == 0)
            EditStudentListFrame.IsVisible = false;
    }

    private void DeleteStudentButton_Clicked(object sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;

        StudentModel student = ((Button)sender).BindingContext as StudentModel;

        model.Students.Remove(student);

        BindingContext = model;

        if (model.Students.Count == 0)
            EditStudentListFrame.IsVisible = false;
    }

    private void AddStudent_Clicked(object sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;

        model.Students ??= new ObservableCollection<StudentModel>();
        int length = model.Students.Count;
        model.Students.Add(new StudentModel() { Id = length+1, Name = "", Surname = "", IsPresent = true, PickedCountdown = 0 });

        BindingContext = model;

        EditStudentListFrame.IsVisible = true;
    }

    private async void EditClass_Clicked(object sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;

        string path = Path.Combine(FileSystem.AppDataDirectory, "classes.txt");
        XDocument xmlDocument = XDocument.Load(path);

        model.Students = new ObservableCollection<StudentModel>(model.Students.OrderBy(x => x.Surname).Select((sm, i) => {
            sm.Id = i + 1;
            return sm;
        }).ToList());

        xmlDocument.Element("Classes").Descendants("Class").Where(
            e => e.Element("Id").Value == model.Id.ToString()
        ).Single().Element("Students").ReplaceAll(model.Students.Select((s,i) => new XElement(
                    "Student",
                    new XAttribute("PickedCountdown", s.PickedCountdown),
                    new XElement("Id", i+1),
                    new XElement("Name", s.Name),
                    new XElement("Surname", s.Surname),
                    new XElement("IsPresent", s.IsPresent)
                ))
        );

        xmlDocument.Save(path);

        string s = JsonSerializer.Serialize(model.Students);
        await Shell.Current.GoToAsync($"{nameof(ClassPage)}?ClassId={model.Id}&Name={model.Name}&Students={s}");

    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}